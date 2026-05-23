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

            // đi tới ô
            navMeshAgent.SetDestination(target.transform.position);

            // đợi tới nơi
            while (
                navMeshAgent.pathPending ||
                navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance
            )
            {
               // manager.playerAnimator.playerAnimator.SetFloat("Walk", 0);
                yield return null;
            }

            // đứng đúng vị trí
            transform.position = target.transform.position;
           

        }
        yield return new WaitForSeconds(0.5f);
        manager.playerCamera.isFollow = false;


        isMoving = false;
    }
    public void FindPonit()
    {
        for (int i = 0; i <22; i++)
        {
            pointCheck[i] = GameObject.Find($"Point {i + 1}");
        }
    }

}