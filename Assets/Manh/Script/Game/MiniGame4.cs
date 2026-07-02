using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame4 : MonoBehaviour
{
    [Header("Manager")]
    public MiniGameManager manager;

    [Header("Pirate")]
    public Animator animator;

    [Header("Settings")]
    public float firstWaitTime = 3f;
    public float watchTime = 5f;
    public float rotateSpeed = 5f;
    public float fastAimSpeed = 25f;

    [Header("Player Speed")]
    public float startMoveSpeed = 1f;
    public float detectedMoveSpeed = 0.3f;
    public float speedIncreaseAfterRespawn = 0.2f;
    public float maxMoveSpeed = 1.0f;

    [Header("Detect")]
    public float moveTolerance = 0.05f;
    public float freezeDelayAfterDetected = 0.18f;

    [Header("Shoot Fake Bullet")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 80f;
    public float bulletHitDistance = 0.15f;
    public float bulletTargetHeight = 1.0f;

    [Header("Finish Line")]
    public float finishLineZ;

    [Header("VFX")]
    public GameObject muzzleFlash;
    public float blinkTime = 1f;
    public float blinkSpeed = 0.12f;

    private bool isRunning;
    private bool isWatching;
    private bool isAttacking;
    private bool hasLaughThisWatch;

    private GameObject currentAttackTarget;
    private bool isWaitingAttackEvent;

    private Quaternion backRotation;
    private Quaternion lookRotation;

    private Queue<GameObject> attackQueue = new Queue<GameObject>();

    private HashSet<GameObject> detectedPlayers = new HashSet<GameObject>();
    private HashSet<GameObject> deadPlayers = new HashSet<GameObject>();
    private List<GameObject> finishedPlayers = new List<GameObject>();

    private Dictionary<GameObject, Vector3> redStartPositions =
        new Dictionary<GameObject, Vector3>();

    private Dictionary<GameObject, float> playerCurrentSpeeds =
        new Dictionary<GameObject, float>();

    private Transform respawnPoint1;
    private Transform respawnPoint2;
    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.rotation;
        backRotation = Quaternion.Euler(0f, 0f, 0f);
        lookRotation = backRotation * Quaternion.Euler(0f, 180f, 0f);
    }

    public void StartMiniGame()
    {
        if (isRunning) return;

        // Tạo 2 transform lưu vị trí ban đầu để làm điểm hồi sinh
        if (respawnPoint1 == null) respawnPoint1 = new GameObject("RespawnPoint1_MG4").transform;
        if (respawnPoint2 == null) respawnPoint2 = new GameObject("RespawnPoint2_MG4").transform;

        if (manager.currentPlayer1 != null)
        {
            respawnPoint1.position = manager.currentPlayer1.transform.position;
            PlayerMiniGame mini1 = manager.currentPlayer1.GetComponent<PlayerMiniGame>();
            if (mini1 != null) mini1.checkPoint = respawnPoint1;
        }

        if (manager.currentPlayer2 != null)
        {
            respawnPoint2.position = manager.currentPlayer2.transform.position;
            PlayerMiniGame mini2 = manager.currentPlayer2.GetComponent<PlayerMiniGame>();
            if (mini2 != null) mini2.checkPoint = respawnPoint2;
        }

        SetUpAllPlayer();
        isRunning = true;
        StartCoroutine(PirateRoutine());
    }

    public void StopMiniGame()
    {
        isRunning = false;

        StopAllCoroutines();

        AudioManager.Instance.StopEnvironment();
        AudioManager.Instance.StopSpecial();
     

        // Tính thưởng trước khi Clear
        CheckFinishReward(manager.currentPlayer1);
        CheckFinishReward(manager.currentPlayer2);

        attackQueue.Clear();
        detectedPlayers.Clear();
        deadPlayers.Clear();
        finishedPlayers.Clear();
        redStartPositions.Clear();

        currentAttackTarget = null;
        isWaitingAttackEvent = false;

        transform.rotation = startRotation;

        // Khôi phục Layer Overrides (xoá layer của người kia khỏi danh sách Exclude)
        if (manager.currentPlayer1 != null && manager.currentPlayer2 != null)
        {
            CharacterController col1 = manager.currentPlayer1.GetComponent<CharacterController>();
            CharacterController col2 = manager.currentPlayer2.GetComponent<CharacterController>();

            if (col1 != null && col2 != null)
            {
                col1.excludeLayers &= ~(1 << manager.currentPlayer2.layer);
                col2.excludeLayers &= ~(1 << manager.currentPlayer1.layer);
            }
        }
    }

    IEnumerator PirateRoutine()
    {

        transform.rotation = backRotation;

        yield return new WaitForSeconds(firstWaitTime);

        while (isRunning)
        {
            attackQueue.Clear();
            detectedPlayers.Clear();
            redStartPositions.Clear();
            hasLaughThisWatch = false;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.scanPiratesClip);

            yield return StartCoroutine(RotateTo(lookRotation, rotateSpeed));

            isWatching = true;

            SaveRedStartPosition(manager.currentPlayer1);
            SaveRedStartPosition(manager.currentPlayer2);

            float timer = 0f;

            while (timer < watchTime)
            {
                CheckFinish(manager.currentPlayer1);
                CheckFinish(manager.currentPlayer2);

                CheckPlayer(manager.currentPlayer1);
                CheckPlayer(manager.currentPlayer2);

                if (!isAttacking && attackQueue.Count > 0)
                    StartCoroutine(ProcessAttackQueue());

                timer += Time.deltaTime;
                yield return null;
            }

            isWatching = false;

            while (isAttacking || attackQueue.Count > 0)
            {
                if (!isAttacking && attackQueue.Count > 0)
                    StartCoroutine(ProcessAttackQueue());

                yield return null;
            }
            AudioManager.Instance.StopSpecial();

            yield return StartCoroutine(RotateTo(backRotation, rotateSpeed));

            RespawnDeadPlayers();

            int randomGreenTime = Random.Range(2, 5);

            PlayRandomPirateVoice(randomGreenTime);

            yield return new WaitForSeconds(randomGreenTime);
        }
    }

    void CheckPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;
        if (!isWatching) return;
        if (finishedPlayers.Contains(playerObj)) return;
        if (deadPlayers.Contains(playerObj)) return;
        if (detectedPlayers.Contains(playerObj)) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        if (move == null) return;

        bool isMovingByInput = move.IsMoving;
        bool isMovingByPosition = HasMovedFromRedStart(playerObj);

        if (isMovingByInput || isMovingByPosition)
        {
            detectedPlayers.Add(playerObj);

            move.speed = detectedMoveSpeed;

            attackQueue.Enqueue(playerObj);

            if (!isAttacking)
                StartCoroutine(ProcessAttackQueue());
        }
    }

    IEnumerator ProcessAttackQueue()
    {
        isAttacking = true;

        if (!hasLaughThisWatch)
        {
            hasLaughThisWatch = true;

            AudioManager.Instance.StopEnvironment();

        }

        while (attackQueue.Count > 0)
        {
            GameObject target = attackQueue.Dequeue();

            if (target != null &&
                !deadPlayers.Contains(target) &&
                !finishedPlayers.Contains(target))
            {
                yield return StartCoroutine(AttackPlayerPirate(target));
            }
        }

        isAttacking = false;
    }

    IEnumerator AttackPlayerPirate(GameObject target)
    {
        if (target == null) yield break;

        PlayerMove move = target.GetComponent<PlayerMove>();
        PlayerAnimator playerAnimator = target.GetComponent<PlayerAnimator>();

        if (move != null)
            move.speed = detectedMoveSpeed;

        yield return new WaitForSeconds(freezeDelayAfterDetected);

        if (move != null)
            move.isJumpAndMove = false;

        if (playerAnimator != null && playerAnimator.playerAnimator != null)
            playerAnimator.playerAnimator.SetFloat("Walk", 0f);

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    fastAimSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.rotation = targetRot;
        }

        currentAttackTarget = target;
        isWaitingAttackEvent = true;

        if (animator != null)
            animator.SetTrigger("Attack");
        else
            PiraterAttack();

        while (isWaitingAttackEvent)
            yield return null;

        currentAttackTarget = null;
    }

    // GỌI HÀM NÀY TRONG ANIMATION EVENT ATTACK
    public void PiraterAttack()
    {
        StartCoroutine(ShowMuzzleFlash());

        if (currentAttackTarget != null)
            ShootFakeBullet(currentAttackTarget);

        isWaitingAttackEvent = false;
    }

    void ShootFakeBullet(GameObject target)
    {
        if (target == null)
            return;

        if (bulletPrefab == null || firePoint == null)
        {
            KillFakePlayer(target);
            return;
        }

        Vector3 targetPos =
            target.transform.position + Vector3.up * bulletTargetHeight;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        StartCoroutine(FakeBulletFly(bullet, target, targetPos));
    }

    IEnumerator FakeBulletFly(GameObject bullet, GameObject target, Vector3 targetPos)
    {
        if (bullet == null)
        {
            KillFakePlayer(target);
            yield break;
        }

        while (Vector3.Distance(bullet.transform.position, targetPos) > bulletHitDistance)
        {
            Vector3 dir = targetPos - bullet.transform.position;

            if (dir != Vector3.zero)
                bullet.transform.rotation = Quaternion.LookRotation(dir);

            bullet.transform.position = Vector3.MoveTowards(
                bullet.transform.position,
                targetPos,
                bulletSpeed * Time.deltaTime
            );

            yield return null;
        }

        Destroy(bullet);
        KillFakePlayer(target);
    }

    void KillFakePlayer(GameObject target)
    {
        if (target == null) return;
        if (finishedPlayers.Contains(target)) return;
        if (deadPlayers.Contains(target)) return;

        deadPlayers.Add(target);

        PlayerMove move = target.GetComponent<PlayerMove>();
        if (move != null)
            move.isJumpAndMove = false;

        PlayerAnimator playerAnimator = target.GetComponent<PlayerAnimator>();
        if (playerAnimator != null && playerAnimator.playerAnimator != null)
        {
            playerAnimator.playerAnimator.SetFloat("Walk", 0f);
            playerAnimator.playerAnimator.SetBool("Die", true);
        }
    }

    void RespawnDeadPlayers()
    {
        foreach (GameObject player in deadPlayers)
        {
            if (player == null) continue;

            PlayerMiniGame mini = player.GetComponent<PlayerMiniGame>();
            PlayerMove move = player.GetComponent<PlayerMove>();
            PlayerAnimator playerAnimator = player.GetComponent<PlayerAnimator>();

            if (mini != null)
                mini.Respawn();

            if (playerAnimator != null && playerAnimator.playerAnimator != null)
                playerAnimator.playerAnimator.SetBool("Die", false);

            StartCoroutine(BlinkPlayer(player));

            if (move != null)
            {
                move.isJumpAndMove = true;

                if (!playerCurrentSpeeds.ContainsKey(player))
                    playerCurrentSpeeds[player] = startMoveSpeed;

                playerCurrentSpeeds[player] = Mathf.Min(
                    playerCurrentSpeeds[player] + speedIncreaseAfterRespawn,
                    maxMoveSpeed
                );

                move.speed = playerCurrentSpeeds[player];
            }
        }

        deadPlayers.Clear();
    }

    IEnumerator BlinkPlayer(GameObject player)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        float timer = 0f;
        bool visible = true;

        while (timer < blinkTime)
        {
            visible = !visible;

            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = visible;
            }

            timer += blinkSpeed;
            yield return new WaitForSeconds(blinkSpeed);
        }

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = true;
        }
    }

    void SaveRedStartPosition(GameObject playerObj)
    {
        if (playerObj == null) return;
        if (finishedPlayers.Contains(playerObj)) return;
        if (deadPlayers.Contains(playerObj)) return;

        redStartPositions[playerObj] = playerObj.transform.position;
    }

    bool HasMovedFromRedStart(GameObject playerObj)
    {
        if (!redStartPositions.ContainsKey(playerObj))
            return false;

        Vector3 startPos = redStartPositions[playerObj];
        Vector3 currentPos = playerObj.transform.position;

        startPos.y = 0f;
        currentPos.y = 0f;

        return Vector3.Distance(startPos, currentPos) > moveTolerance;
    }

    void CheckFinish(GameObject player)
    {
        if (player == null) return;
        if (finishedPlayers.Contains(player)) return;
        if (detectedPlayers.Contains(player)) return;
        if (deadPlayers.Contains(player)) return;

        // Nếu player chạy theo hướng Z giảm
        if (player.transform.position.z >= finishLineZ)
        {
            finishedPlayers.Add(player);

            PlayerMove move = player.GetComponent<PlayerMove>();
            if (move != null)
            {
                move.isJumpAndMove = false;
            }

            PlayerAnimator playerAnimator = player.GetComponent<PlayerAnimator>();
            if (playerAnimator != null && playerAnimator.playerAnimator != null)
            {
                playerAnimator.playerAnimator.SetFloat("Walk", 0f);
                playerAnimator.playerAnimator.SetBool("Die", false);
            }

            // Quay player về phía sau
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // Rút ngắn thời gian nếu cả 2 đã về đích
            if (finishedPlayers.Count >= 2 && manager != null)
            {
                manager.timer = 2f;
            }
        }
    }


    void CheckFinishReward(GameObject player)
    {
        if (player == null) return;

        PlayerMiniGame mini = player.GetComponent<PlayerMiniGame>();
        if (mini == null) return;

        if (finishedPlayers.Contains(player))
        {
            int finishRank = finishedPlayers.IndexOf(player);
            if (finishRank == 0)
            {
                mini.UpCoin(1, 200); // 1st place gets more
            }
            else
            {
                mini.UpCoin(1, 100); // 2nd place gets normal amount
            }
        }
        else
        {
            mini.UpCoin(0, 100);
        }
    }

    IEnumerator RotateTo(Quaternion targetRotation, float speed)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                speed * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = targetRotation;
    }

    void PlayRandomPirateVoice(int random)
    {
        // 2 -> index 0
        // 3 -> index 1
        // 4 -> index 2
        int clipIndex = random - 2;

        AudioClip[] clipList = AudioManager.Instance.piratesSingClipList;

        if (clipList == null || clipList.Length == 0)
            return;

        if (clipIndex < 0 || clipIndex >= clipList.Length)
            return;

        AudioManager.Instance.PlaySpecialOneShot(clipList[clipIndex]);
        Debug.Log(clipIndex);

    }

    IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlash != null)
            muzzleFlash.SetActive(true);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.gunShotPiratesClip);

        yield return new WaitForSeconds(0.15f);

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    public void SetUpAllPlayer()
    {
        SetUpPlayer(manager.currentPlayer1);
        SetUpPlayer(manager.currentPlayer2);

        // Sử dụng Layer Overrides để bỏ qua va chạm với layer của người kia
        if (manager.currentPlayer1 != null && manager.currentPlayer2 != null)
        {
            CharacterController col1 = manager.currentPlayer1.GetComponent<CharacterController>();
            CharacterController col2 = manager.currentPlayer2.GetComponent<CharacterController>();

            if (col1 != null && col2 != null)
            {
                col1.excludeLayers |= (1 << manager.currentPlayer2.layer);
                col2.excludeLayers |= (1 << manager.currentPlayer1.layer);
            }
        }
    }

    void SetUpPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        PlayerAnimator playerAnimator = playerObj.GetComponent<PlayerAnimator>();

        if (move != null)
        {
            move.speed = startMoveSpeed;
            move.isJumpAndMove = true;
            move.isWalk = true;
            playerCurrentSpeeds[playerObj] = startMoveSpeed;
        }

        if (playerAnimator != null && playerAnimator.playerAnimator != null)
            playerAnimator.playerAnimator.SetBool("Die", false);
    }
}