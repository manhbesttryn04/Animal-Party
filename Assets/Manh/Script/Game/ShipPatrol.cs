using UnityEngine;

public class ShipPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 20f;
    public float moveSpeed = 5f;
    public float rotateSpeed = 3f;

    private Vector3 startPos;
    private Vector3 targetPos;
    public bool isStop = false;

    private void Start()
    {
        startPos = transform.position;

        targetPos =
            startPos +
            transform.forward * moveDistance;
    }

    private void Update()
    {if (isStop) return;
        Vector3 direction =
            (targetPos - transform.position).normalized;

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
        }

        if (
            Vector3.Distance(
                transform.position,
                targetPos
            ) < 0.5f
        )
        {
            if (
                Vector3.Distance(
                    targetPos,
                    startPos + transform.forward * moveDistance
                ) < 1f
            )
            {
                targetPos =
                    startPos -
                    transform.forward * moveDistance;
            }
            else
            {
                targetPos =
                    startPos +
                    transform.forward * moveDistance;
            }
        }
    }
}