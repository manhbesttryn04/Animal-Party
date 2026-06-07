using UnityEngine;

public class CannonDebuff : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bombPrefab;

    public BombDebuff Fire(Transform target)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.cannonClip);
        GameObject bomb =
            Instantiate(
                bombPrefab,
                firePoint.position,
                Quaternion.identity
            );

        BombDebuff bombScript =
            bomb.GetComponent<BombDebuff>();

        bombScript.target = target;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fallingBom);

        return bombScript;
    }
}