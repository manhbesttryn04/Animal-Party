using UnityEngine;

/// <summary>
/// Ngư lôi bắn ra từ Torpedo_L/Torpedo_R. Bay thẳng theo hướng bắn, va chạm tàu đối phương
/// thì gây stun (khóa điều khiển tạm thời) rồi tự hủy.
/// </summary>
public class TorpedoProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 4f;
    public float stunDuration = 2f;
    public float hitRadius = 0.6f;

    private Vector3 direction;
    private PlayerSubmarineController owner;

    public void Launch(Vector3 dir, PlayerSubmarineController shooter)
    {
        direction = dir.normalized;
        owner = shooter;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        CheckHit();
    }

    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (var hit in hits)
        {
            PlayerSubmarineController target = hit.GetComponentInParent<PlayerSubmarineController>();
            if (target == null || target == owner) continue;

            target.ApplyStun(stunDuration);
            Destroy(gameObject);
            return;
        }
    }
}