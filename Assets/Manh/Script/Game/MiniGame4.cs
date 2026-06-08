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
    public float watchTime = 4f;
    public float rotateSpeed = 5f;
    public float respawnDelay = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip scanSound;     // hồi hộp khi quan sát
    public AudioClip laughSound;    // cười khi phát hiện
    public AudioClip gunShotSound;  // tiếng súng

    private bool isRunning;
    private bool isAttacking;

    private Quaternion backRotation;
    private Quaternion lookRotation;

    private Queue<GameObject> attackQueue =
        new Queue<GameObject>();

    private HashSet<GameObject> detectedPlayers =
        new HashSet<GameObject>();

    private void Start()
    {
        backRotation = transform.rotation;
        lookRotation = backRotation * Quaternion.Euler(0f, 90f, 0f);
    }

    public void StartMiniGame()
    {
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

        transform.rotation = backRotation;
    }

    IEnumerator PirateRoutine()
    {
        isRunning = true;

        yield return new WaitForSeconds(firstWaitTime);

        while (isRunning)
        {
            attackQueue.Clear();
            detectedPlayers.Clear();

            yield return StartCoroutine(RotateTo(lookRotation));

            // 🔊 bật nhạc hồi hộp khi quan sát
            if (audioSource != null && scanSound != null)
            {
                audioSource.clip = scanSound;
                audioSource.loop = true;
                audioSource.Play();
            }

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

            yield return StartCoroutine(RotateTo(backRotation));

            float randomTime = Random.Range(1f, 4f);
            yield return new WaitForSeconds(randomTime);
        }
    }

    void CheckPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        if (detectedPlayers.Contains(playerObj)) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        if (move == null) return;

        if (move.IsMoving)
        {
            detectedPlayers.Add(playerObj);
            move.isJumpAndMove = false;
            attackQueue.Enqueue(playerObj);
        }
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

        if (audioSource != null && gunShotSound != null)
        {
            audioSource.PlayOneShot(gunShotSound);
        }

        yield return new WaitForSeconds(respawnDelay);

        PlayerMiniGame mini = target.GetComponent<PlayerMiniGame>();
        if (mini != null)
        {
            mini.Respawn();
        }

        PlayerMove move = target.GetComponent<PlayerMove>();
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
}