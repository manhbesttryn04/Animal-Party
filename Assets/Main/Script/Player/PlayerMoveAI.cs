using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveAI : MonoBehaviour
{
    #region Variables

    [Header("Manager")]
    public PlayerManager manager;
    public PlayerTrapState playerTrapState;

    [Header("NavMesh")]
    public NavMeshAgent navMeshAgent;

    [Header("Board")]
    public List<GameObject> pointCheck = new List<GameObject>();
    public int currentIndex = 0;

    [Header("State")]
    public bool isMoving = false;

    #endregion

    #region Unity Events

    private void Start()
    {
        playerTrapState = GetComponent<PlayerTrapState>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navMeshAgent.avoidancePriority = 50;
        navMeshAgent.updateRotation = false;

        pointCheck = FindAnyObjectByType<PointCheck>().point.ToList();

        FindPoint();
    }

    #endregion

    #region Move Main

    public void StartMove(int value)
    {
        if (isMoving)
            return;

        if (CheckFinishIndex())
            return;

        StartCoroutine(AIToPoint(value));
    }

    public IEnumerator AIToPoint(int value)
    {
        isMoving = true;

        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        int finishIndex = pointCheck.Count - 1;
        int stepCanMove = Mathf.Min(value, finishIndex - currentIndex);

        for (int i = 0; i <= stepCanMove; i++)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);

            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = GetPlayerOffset();

            yield return StartCoroutine(JumpTo(target.transform.position + offset));

            if (CheckFinishIndex())
            {
                StopAllCameraFollow();
                isMoving = false;
                yield break;
            }
        }

        yield return new WaitForSeconds(0.5f);

        LookNextPoint();

        if (manager.playerBuff.isBuffDice > 0)
        {
            manager.playerBuff.isBuffDice--;

            yield return StartCoroutine(MoveBonus());
            yield break;
        }

        yield return StartCoroutine(playerTrapState.CheckCurrentTile());

        if (CheckFinishIndex())
        {
            StopAllCameraFollow();
            isMoving = false;
            yield break;
        }

        EndTurn();
    }

    #endregion

    #region Bonus Move

    public IEnumerator MoveBonus()
    {
        StartCoroutine(UIManager.Instance.HideBonusPanel());

        yield return new WaitForSeconds(1f);

        int finishIndex = pointCheck.Count - 1;
        int bonusStep = Mathf.Min(2, finishIndex - currentIndex);

        for (int i = 0; i <= bonusStep; i++)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);

            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = GetPlayerOffset();

            yield return StartCoroutine(JumpTo(target.transform.position + offset));

            if (CheckFinishIndex())
            {
                StopAllCameraFollow();
                isMoving = false;
                yield break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        LookNextPoint();

        yield return StartCoroutine(playerTrapState.CheckCurrentTile());

        if (CheckFinishIndex())
        {
            isMoving = false;
          
            yield break;
        }

        EndTurn();
    }

    #endregion

    #region Win Check

    public bool CheckFinishIndex()
    {
        if (pointCheck == null || pointCheck.Count == 0)
            return false;

        int finishIndex = pointCheck.Count - 1; // Point 33 = index 32

        if (currentIndex >= finishIndex)
        {
            currentIndex = finishIndex;

            bool isPlayer2 = manager.playerType.isPlayer2;

            bool hasWin = GameManager.Instance.CheckWinnerByIndex(isPlayer2, currentIndex);

            if (hasWin)
            {
                isMoving = false;
                navMeshAgent.ResetPath();
            }

            return hasWin;
        }

        return false;
    }

    #endregion

    #region End Turn
    private void StopAllCameraFollow()
    {
        manager.playerCamera.isFllow2 = false;
        manager.playerCamera.isFllow3 = false;
    }

    public void EndTurn()
    {
        manager.playerCamera.isFllow2 = false;
        manager.playerCamera.isFllow3 = true;

        StartCoroutine(EndTurnRoutine());
    }

    private IEnumerator EndTurnRoutine()
    {
        yield return new WaitForSeconds(1f);

        manager.playerCamera.isFllow3 = false;

        isMoving = false;

        if (!manager.playerRound.isRound1)
        {
            manager.playerRound.isRound1 = true;
        }

        if (!manager.playerRound.nextRound)
        {
            manager.playerRound.nextRound = true;
        }
    }

    #endregion

    #region Effects

    public IEnumerator BoomHitEffect(int power)
    {
        isMoving = true;

        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        currentIndex = Mathf.Max(0, currentIndex - power);

        GameObject targetPoint = pointCheck[currentIndex];

        Vector3 finalPos = targetPoint.transform.position + GetPlayerOffset();

        navMeshAgent.speed = 25f;
        navMeshAgent.acceleration = 999f;

        navMeshAgent.SetDestination(finalPos);
        yield return StartCoroutine(playerTrapState.CheckCurrentTile());
        while (navMeshAgent.pathPending ||
               navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        navMeshAgent.ResetPath();
        navMeshAgent.Warp(transform.position);

       

        isMoving = false;
    }

    public IEnumerator TeleportEffect(int targetIndex)
    {
        isMoving = true;

        currentIndex = Mathf.Clamp(targetIndex, 0, pointCheck.Count - 1);

        GameObject targetPoint = pointCheck[currentIndex];

        Vector3 finalPos = targetPoint.transform.position + GetPlayerOffset();

        navMeshAgent.enabled = false;
        transform.position = finalPos;
        navMeshAgent.enabled = true;
        navMeshAgent.Warp(finalPos);

        if (CheckFinishIndex())
        {
            isMoving = false;
            yield break;
        }

        isMoving = false;
        yield return null;
    }

    private IEnumerator JumpTo(Vector3 targetPos)
    {
        navMeshAgent.enabled = false;

        Vector3 startPos = transform.position;

        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        while (!manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        float duration = 0.4f;
        float height = 0.8f;
        float t = 0f;

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

        while (manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        navMeshAgent.enabled = true;
        navMeshAgent.Warp(targetPos);
    }


    #endregion

    #region Utility

    public void FindPoint()
    {
        for (int i = 0; i < 33; i++)
        {
            pointCheck[i] = GameObject.Find($"Point {i + 1}");
        }
    }

    private Vector3 GetPlayerOffset()
    {
        return manager.playerType.isPlayer2
            ? new Vector3(0, 0, -0.3f)
            : new Vector3(0, 0, 0.3f);
    }

    private void LookNextPoint()
    {
        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt(pointCheck[currentIndex + 1].transform.position);
        }
    }

    #endregion
}