using UnityEngine;

public class CannonDebuff : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bombPrefab;

    public BombDebuff Fire(Transform target)
    {
        GameObject bomb =
            Instantiate(
                bombPrefab,
                firePoint.position,
                Quaternion.identity
            );

        BombDebuff bombScript =
            bomb.GetComponent<BombDebuff>();

        bombScript.target = target;

        return bombScript;
    }
}