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
    public float startMoveSpeed = 0.4f;
    public float detectedMoveSpeed = 0.3f;
    public float speedIncreaseAfterRespawn = 0.2f;
    public float maxMoveSpeed = 1.0f;

    [Header("Detect")]
    public float moveTolerance = 0.05f;

    [Header("Shoot")]
    public float laughDelay = 0.15f;
    public float shootDelay = 0.25f;

    [Header("Finish Line")]
    public float finishLineZ;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource environmentAudioSource;
    public AudioClip scanSound;
    public AudioClip laughSound;
    public AudioClip gunShotSound;
    public AudioClip rainSound;

    [Header("VFX")]
    public GameObject muzzleFlash;

    private bool isRunning;
    private bool isWatching;
    private bool isAttacking;
    private bool hasLaughThisWatch;

    private Quaternion backRotation;
    private Quaternion lookRotation;

    private Queue<GameObject> attackQueue = new Queue<GameObject>();

    private HashSet<GameObject> detectedPlayers = new HashSet<GameObject>();
    private HashSet<GameObject> deadPlayers = new HashSet<GameObject>();
    private HashSet<GameObject> finishedPlayers = new HashSet<GameObject>();

    private Dictionary<GameObject, Vector3> redStartPositions =
        new Dictionary<GameObject, Vector3>();

    private Dictionary<GameObject, float> playerCurrentSpeeds =
        new Dictionary<GameObject, float>();

    private void Start()
    {
        backRotation = Quaternion.Euler(0f, 0f, 0f);
        lookRotation = backRotation * Quaternion.Euler(0f, 180f, 0f);
    }

    public void StartMiniGame()
    {
        if (isRunning) return;

        SetUpAllPlayer();

        isRunning = true;
        StartCoroutine(PirateRoutine());
    }

    public void StopMiniGame()
    {
        isRunning = false;
        StopAllCoroutines();

        attackQueue.Clear();
        detectedPlayers.Clear();
        deadPlayers.Clear();
        finishedPlayers.Clear();
        redStartPositions.Clear();

        if (audioSource != null)
            audioSource.Stop();

        CheckFinishReward(manager.currentPlayer1);
        CheckFinishReward(manager.currentPlayer2);

        transform.rotation = backRotation;
    }

    IEnumerator PirateRoutine()
    {
        if (environmentAudioSource != null && rainSound != null)
        {
            environmentAudioSource.PlayOneShot(rainSound);
        }

        transform.rotation = backRotation;

        yield return new WaitForSeconds(firstWaitTime);

        while (isRunning)
        {
            // =========================
            // ĐÈN ĐỎ - CƯỚP BIỂN QUAY LẠI
            // =========================

            attackQueue.Clear();
            detectedPlayers.Clear();
            redStartPositions.Clear();
            hasLaughThisWatch = false;

            if (audioSource != null && scanSound != null)
            {
                audioSource.clip = scanSound;
                audioSource.loop = true;
                audioSource.Play();
            }

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
                {
                    StartCoroutine(ProcessAttackQueue());
                }

                timer += Time.deltaTime;
                yield return null;
            }

            isWatching = false;

            while (isAttacking)
            {
                yield return null;
            }

            if (audioSource != null && audioSource.clip == scanSound)
            {
                audioSource.Stop();
            }

            // =========================
            // ĐÈN XANH - CƯỚP BIỂN QUAY LƯNG
            // =========================

            yield return StartCoroutine(RotateTo(backRotation, rotateSpeed));

            RespawnDeadPlayers();

            float randomGreenTime = Random.Range(1f, 4f);
            yield return new WaitForSeconds(randomGreenTime);
        }
    }

    void SaveRedStartPosition(GameObject playerObj)
    {
        if (playerObj == null) return;
        if (finishedPlayers.Contains(playerObj)) return;
        if (deadPlayers.Contains(playerObj)) return;

        redStartPositions[playerObj] = playerObj.transform.position;
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
            {
                StartCoroutine(ProcessAttackQueue());
            }
        }
    }

    bool HasMovedFromRedStart(GameObject playerObj)
    {
        if (!redStartPositions.ContainsKey(playerObj))
            return false;

        Vector3 startPos = redStartPositions[playerObj];
        Vector3 currentPos = playerObj.transform.position;

        startPos.y = 0f;
        currentPos.y = 0f;

        float distance = Vector3.Distance(startPos, currentPos);

        return distance > moveTolerance;
    }

    IEnumerator ProcessAttackQueue()
    {
        isAttacking = true;

        // Cười 1 lần khi phát hiện người đầu tiên
        // Nhưng KHÔNG chờ cười xong mới bắn
        if (!hasLaughThisWatch)
        {
            hasLaughThisWatch = true;

            if (audioSource != null && audioSource.clip == scanSound)
            {
                audioSource.Stop();
            }

            if (audioSource != null && laughSound != null)
            {
                audioSource.PlayOneShot(laughSound);
            }
        }

        // Bắn liên tục các player trong hàng chờ
        while (attackQueue.Count > 0)
        {
            GameObject target = attackQueue.Dequeue();

            if (target != null &&
                !deadPlayers.Contains(target) &&
                !finishedPlayers.Contains(target))
            {
                yield return StartCoroutine(AttackPlayerCowboy(target));
            }
        }

        isAttacking = false;
    }

    IEnumerator AttackPlayerCowboy(GameObject target)
    {
        if (target == null)
            yield break;

        PlayerMove move = target.GetComponent<PlayerMove>();
        PlayerAnimator playerAnimator = target.GetComponent<PlayerAnimator>();

        if (move != null)
        {
            move.speed = detectedMoveSpeed;
        }

        if (playerAnimator != null && playerAnimator.playerAnimator != null)
        {
            playerAnimator.playerAnimator.SetFloat("Run", 0f);
        }

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

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(shootDelay);

        KillFakePlayer(target);
    }

    void KillFakePlayer(GameObject target)
    {
        if (target == null) return;

        if (finishedPlayers.Contains(target))
            return;

        deadPlayers.Add(target);

        PlayerMove move = target.GetComponent<PlayerMove>();
        if (move != null)
        {
            move.isJumpAndMove = false;
        }

        PlayerAnimator playerAnimator = target.GetComponent<PlayerAnimator>();
        if (playerAnimator != null && playerAnimator.playerAnimator != null)
        {
            playerAnimator.playerAnimator.SetFloat("Run", 0f);
            //playerAnimator.playerAnimator.SetTrigger("Dead");
        }
    }

    void RespawnDeadPlayers()
    {
        foreach (GameObject player in deadPlayers)
        {
            if (player == null) continue;

            PlayerMiniGame mini = player.GetComponent<PlayerMiniGame>();
            PlayerMove move = player.GetComponent<PlayerMove>();

            if (mini != null)
            {
                mini.Respawn();
            }

            if (move != null)
            {
                move.isJumpAndMove = true;

                if (!playerCurrentSpeeds.ContainsKey(player))
                {
                    playerCurrentSpeeds[player] = startMoveSpeed;
                }

                playerCurrentSpeeds[player] = Mathf.Min(
                    playerCurrentSpeeds[player] + speedIncreaseAfterRespawn,
                    maxMoveSpeed
                );

                move.speed = playerCurrentSpeeds[player];
            }
        }

        deadPlayers.Clear();
    }

    void CheckFinish(GameObject player)
    {
        if (player == null) return;
        if (finishedPlayers.Contains(player)) return;
        if (detectedPlayers.Contains(player)) return;
        if (deadPlayers.Contains(player)) return;

        if (player.transform.position.z >= finishLineZ)
        {
            finishedPlayers.Add(player);

            PlayerMove move = player.GetComponent<PlayerMove>();
            if (move != null)
            {
                move.isJumpAndMove = false;
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
            mini.UpCoin(1, 100);
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

    public void PiraterAttack()
    {
        StartCoroutine(ShowMuzzleFlash());
    }

    IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
        }

        if (audioSource != null && gunShotSound != null)
        {
            audioSource.PlayOneShot(gunShotSound);
        }

        yield return new WaitForSeconds(0.15f);

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
    }

    public void SetUpAllPlayer()
    {
        SetUpPlayer(manager.currentPlayer1);
        SetUpPlayer(manager.currentPlayer2);
    }

    void SetUpPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();
        PlayerMove move = playerObj.GetComponent<PlayerMove>();

        if (move != null)
        {
            move.speed = startMoveSpeed;
            move.isJumpAndMove = true;

            playerCurrentSpeeds[playerObj] = startMoveSpeed;
        }

    }
}