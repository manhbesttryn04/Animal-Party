using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveAI : MonoBehaviour
{
    public PlayerManager manager;
    public NavMeshAgent navMeshAgent;

    public List<GameObject> pointCheck = new List<GameObject>();

    public int currentIndex = 0;

    public bool isMoving = false;
   


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navMeshAgent.avoidancePriority = 50;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        pointCheck = FindAnyObjectByType<PointCheck>().point.ToList();
        FindPonit();
     
    }
    private void Update()
    {
        //manager.playerAnimator.playerAnimator.SetFloat("Walk", navMeshAgent.velocity.magnitude);
    }
   

    // Hàm gọi khi xúc xắc ra số
    public void StartMove(int value)
    {
        if (!isMoving)
        {
            StartCoroutine(AIToPoint(value));
        }
    }

    public IEnumerator AIToPoint(int value)
    {
        isMoving = true;
        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        for (int i = 0; i <= value; i++)
        {

            // sang ô tiếp theo
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);
            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = manager.playerType.isPlayer2
     ? new Vector3(0, 0, -0.3f)
     : new Vector3(0, 0, 0.3f);

            yield return StartCoroutine(JumpTo(target.transform.position + offset));


        }
        yield return new WaitForSeconds(0.5f);
        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt( pointCheck[currentIndex + 1].transform.position);

        }
       
      

        if (manager.playerBuff.isBuffDice > 0)
        {
            manager.playerBuff.isBuffDice --;
            yield return StartCoroutine(MoveBonus());
        }else
        {
            manager.playerCamera.isFllow2 = false;
            manager.playerCamera.isFllow3 = true;
            yield return new WaitForSeconds(1f);
            manager.playerCamera.isFllow3 = false;
            isMoving = false;
            if (!manager.playerRound.isRound1)
            {
                manager.playerRound.isRound1 = true;
            }
            if(!manager.playerRound.nextRound)
            {
                manager.playerRound.nextRound = true;
            }
        }
    }
    public IEnumerator MoveBonus()
    {

      StartCoroutine(UIManager.Instance.HideBonusPanel());

        yield return new WaitForSeconds(1f);

 

        for (int i = 0; i < 2; i++)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);
            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = manager.playerType.isPlayer2
                ? new Vector3(0, 0, -0.3f)
                : new Vector3(0, 0, 0.3f);

            yield return StartCoroutine(JumpTo(target.transform.position + offset));

            yield return new WaitForSeconds(0.3f);
        }

        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt(pointCheck[currentIndex + 1].transform.position);
        }

        manager.playerCamera.isFllow2 = false;
        manager.playerCamera.isFllow3 = true;

        yield return new WaitForSeconds(1f);

        manager.playerCamera.isFllow3 = false;
        isMoving = false;
        if (!manager.playerRound.isRound1)
        {
            manager.playerRound.isRound1 = true;
        }
        if(!manager.playerRound.nextRound)
        {
            manager.playerRound.nextRound = true;
        }
    }

    public IEnumerator BoomHitEffect(int power)
    {
        isMoving = true;

        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        // =========================
        // 1. TRỪ INDEX
        // =========================
        currentIndex = Mathf.Max(0, currentIndex - power);

        GameObject targetPoint = pointCheck[currentIndex];

        Vector3 offset = manager.playerType.isPlayer2
            ? new Vector3(0, 0, -1f)
            : new Vector3(0, 0, 1f);

        Vector3 finalPos = targetPoint.transform.position + offset;

        // =========================
        // 2. BLINK MESH (CHỚP TẮT)
        // =========================

        // =========================
        // 3. DASH VỀ TILE (CỰC NHANH)
        // =========================
       // navMeshAgent.enabled = true;

        navMeshAgent.speed = 25f;        // 🔥 rất nhanh
        navMeshAgent.acceleration = 999f;

        navMeshAgent.SetDestination(finalPos);

        while (navMeshAgent.pathPending ||
               navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        // =========================
        // RESET SPEED
        // =========================

        navMeshAgent.ResetPath();
        navMeshAgent.Warp(transform.position);
        isMoving = false;
    }
  IEnumerator JumpTo(Vector3 targetPos)
    {
        navMeshAgent.enabled = false;

        Vector3 startPos = transform.position;

        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        // Đợi animation Jump bắt đầu
        while (!manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        float duration = 0.4f;
        float height = 0.8f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            float percent = t / duration;

            Vector3 pos = Vector3.Lerp(startPos, targetPos, percent);
            pos.y += Mathf.Sin(percent * Mathf.PI) * height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        // Đợi Jump kết thúc, Animator quay về Idle/Walk
        while (manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        navMeshAgent.enabled = true;
        navMeshAgent.Warp(targetPos);
    }

    public void FindPonit()
    {
        for (int i = 0; i <34; i++)
        {
            pointCheck[i] = GameObject.Find($"Point {i + 1}");
        }
    }


}