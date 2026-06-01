using System.Collections;
using UnityEngine;

public class BombDebuff : MonoBehaviour
{
    public Transform target;

    [Header("Flight")]
    public float flyTime = 1.5f;
    public float arcHeight = 4f;
    public float rotateSpeed = 720f;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        StartCoroutine(FlyRoutine());
    }

    private IEnumerator FlyRoutine()
    {
        float time = 0f;

        while (time < flyTime)
        {
            time += Time.deltaTime;

            float t = time / flyTime;

            Vector3 pos =
                Vector3.Lerp(
                    startPos,
                    target.position,
                    t
                );

            pos.y +=
                arcHeight *
                Mathf.Sin(t * Mathf.PI);

            transform.position = pos;

            // Xoay bomb trên không
            transform.Rotate(
                Vector3.forward,
                rotateSpeed * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        HitTarget();
    }

    private void HitTarget()
    {
        // Hiệu ứng nổ
        if (explosionPrefab != null)
        {
            GameObject explosion =
                Instantiate(
                    explosionPrefab,
                    transform.position,
                    Quaternion.identity
                );

            Destroy(explosion, 3f);
        }

        PlayerManager player =
            target.GetComponent<PlayerManager>();

        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        // Có khiên Cannon
        if (player.playerBuff.isBuffCanon)
        {
            StartCoroutine(
                player.playerBuff.ShowCanonShield()
            );

            Destroy(gameObject);
            return;
        }

        PlayerMoveAI moveAI =
            target.GetComponent<PlayerMoveAI>();

        if (moveAI == null)
        {
            Destroy(gameObject);
            return;
        }

        // Lùi 3 ô
        moveAI.currentIndex =
            Mathf.Max(
                0,
                moveAI.currentIndex - 3
            );

        GameObject point =
            moveAI.pointCheck[
                moveAI.currentIndex
            ];

        Vector3 offset =
            player.playerType.isPlayer2
            ? new Vector3(0, 0, -1f)
            : new Vector3(0, 0, 1f);

        target.position =
            point.transform.position + offset;

        Debug.Log(
            player.name +
            " bị bắn lùi 3 ô"
        );

        Destroy(gameObject);
    }

}