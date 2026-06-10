using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public float respawnDelay = 1.5f;


    [Header("Finish Line")]
    public float finishLineZ = 18.36718f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip scanSound;     // hồi hộp khi quan sát
    public AudioClip laughSound;    // cười khi phát hiện
    public AudioClip gunShotSound;  // tiếng súng

    [Header("VFX")]
    public GameObject muzzleFlash;

    private bool isRunning;
    private bool isAttacking;
    private bool isWatching;

    private Quaternion backRotation;
    private Quaternion lookRotation;

    private Queue<GameObject> attackQueue =
        new Queue<GameObject>();

    private HashSet<GameObject> detectedPlayers =
        new HashSet<GameObject>();

    private void Start()
    {
        backRotation = Quaternion.Euler(0, 0, 0);
        lookRotation = backRotation * Quaternion.Euler(0f, 180f, 0f);
    }

    public void StartMiniGame()
    { 
        PlayerManager p1 = manager.currentPlayer1.GetComponent<PlayerManager>();
        PlayerManager p2 = manager.currentPlayer2.GetComponent<PlayerManager>();
        if(p1 != null && p2 != null)
        {
            //Set speed
            p1.playerMove.speed = 1.5f;
            p2.playerMove.speed = 1.5f;
            //Set funny
            p1.playerFunnyItem.isBoxFunny = true;
            p2.playerFunnyItem.isBoxFunny = true;
            


        }

        if (isRunning)
            return;

        StartCoroutine(PirateRoutine());
    }

    public void StopMiniGame()
    {
        isRunning = false;

        StopAllCoroutines();

        attackQueue.Clear();
        detectedPlayers.Clear();

        if (audioSource != null)
        {
            audioSource.Stop();
        }
        CheckFinishReward(manager.currentPlayer1);
        CheckFinishReward(manager.currentPlayer2);
        transform.rotation = backRotation;
    }

    IEnumerator PirateRoutine()
    {
        transform.rotation = lookRotation;
        isRunning = true;

        yield return new WaitForSeconds(firstWaitTime);

        while (isRunning)
        {
            attackQueue.Clear();
            detectedPlayers.Clear();

            // 🔊 bật nhạc hồi hộp khi quan sát
            if (audioSource != null && scanSound != null)
            {
                audioSource.clip = scanSound;
                audioSource.loop = false;
                audioSource.Play();
            }
            isWatching = true;
            yield return StartCoroutine(RotateTo(lookRotation));

           

            float timer = 0f;

            while (timer < watchTime)
            {
                CheckPlayer(manager.currentPlayer1);
                CheckPlayer(manager.currentPlayer2);

                if (!isAttacking && attackQueue.Count > 0)
                {
                    StartCoroutine(ProcessAttackQueue());
                }

                timer += Time.deltaTime;
                yield return null;
            }

            while (isAttacking)
            {
                yield return null;
            }

            // 🔊 tắt nhạc hồi hộp
            if (audioSource != null && audioSource.clip == scanSound)
            {
                audioSource.Stop();
            }
            isWatching = false;
            yield return StartCoroutine(RotateTo(backRotation));

            float randomTime = Random.Range(1f, 4f);
            yield return new WaitForSeconds(randomTime);
        }
    }

    void CheckPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        // Đã qua vạch đích thì không bị kiểm tra
        if (playerObj.transform.position.z >= finishLineZ)
            return;

        if (detectedPlayers.Contains(playerObj)) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();

        if (move == null) return;

        if (move.IsMoving)
        {
            detectedPlayers.Add(playerObj);
            StartCoroutine(CatchPlayer(playerObj, move));
        }
    }
    void CheckFinishReward(GameObject player)
    {
        if (player == null) return;

        PlayerMiniGame mini =
            player.GetComponent<PlayerMiniGame>();

        if (mini == null) return;

        if (player.transform.position.z >= finishLineZ)
            mini.UpCoin(1, 100);
        else
            mini.UpCoin(0,100);
    }
    IEnumerator CatchPlayer(GameObject playerObj, PlayerMove move)
    {
       // yield return new WaitForSeconds(0.01f);

        if (playerObj == null || move == null)
            yield break;

        // Người chơi đã dừng lại
        if (!move.IsMoving)
            yield break;

        if (playerObj.transform.position.z >= finishLineZ)
            yield break;

        // Cướp biển không còn quan sát
        if (!isWatching)
            yield break;

        attackQueue.Enqueue(playerObj);
    }

    IEnumerator ProcessAttackQueue()
    {
        isAttacking = true;

        while (attackQueue.Count > 0)
        {
            GameObject target = attackQueue.Dequeue();
            yield return StartCoroutine(AttackPlayer(target));
        }

        isAttacking = false;
    }

    IEnumerator AttackPlayer(GameObject target)
    {
        if (target == null)
            yield break;

        PlayerMove move = target.GetComponent<PlayerMove>();
        PlayerAnimator p = target.GetComponent<PlayerAnimator>();

        if (p != null)
        {
            p.playerAnimator.SetFloat("Run", 0f);
        }

        if (move != null)
        {
            move.isJumpAndMove = false;
        }
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotateSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.rotation = targetRot;
        }

        // 🔊 dừng nhạc hồi hộp
        if (audioSource != null && audioSource.clip == scanSound)
        {
            audioSource.Stop();
        }

        // 😂 cười khi phát hiện
        if (audioSource != null && laughSound != null)
        {
            audioSource.PlayOneShot(laughSound);
        }

        yield return new WaitForSeconds(laughSound != null ? laughSound.length : 0.5f);

        // 🔫 bắn
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
       
        

        yield return new WaitForSeconds(respawnDelay);

        PlayerMiniGame mini = target.GetComponent<PlayerMiniGame>();
      
        if (mini != null)
        {
            mini.Respawn();
        }

        if (move != null)
        {
            move.isJumpAndMove = true;
        }

        yield return StartCoroutine(RotateTo(lookRotation));
    }


    IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
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
        muzzleFlash.SetActive(true);
        if (audioSource != null && gunShotSound != null)
        {
            audioSource.PlayOneShot(gunShotSound);
        }

        yield return new WaitForSeconds(0.5f);

        muzzleFlash.SetActive(false);
    }
}