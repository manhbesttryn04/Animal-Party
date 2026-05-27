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

        pointCheck = FindAnyObjectByType<PointCheck>().point.ToList();
        FindPonit();
    }
    private void Update()
    {
        manager.playerAnimator.playerAnimator.SetFloat("Walk", navMeshAgent.velocity.magnitude);
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

        for (int i = 0; i <= value; i++)
        {
            
            // sang ô tiếp theo
            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = manager.playerType.isPlayer2
     ? new Vector3(0, 0, -1f)
     : new Vector3(0, 0, 1f);

            navMeshAgent.SetDestination(target.transform.position + offset);



            // đợi tới nơi
            while (
                    navMeshAgent.pathPending ||
                    navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                {

                    yield return null;
                }

            // đứng đúng vị tr
           

        }
        yield return new WaitForSeconds(0.5f);
        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt(
                pointCheck[currentIndex + 1].transform.position
            );
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
    }
    public void FindPonit()
    {
        for (int i = 0; i <22; i++)
        {
            pointCheck[i] = GameObject.Find($"Point {i + 1}");
        }
    }

}