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

    [Tooltip("Bù góc xoay cho mesh (Capsule mặc định nằm dọc trục Y, cần xoay X=90 để nằm ngang theo hướng bay). Thử đổi giá trị nếu vẫn sai hướng.")]
    public Vector3 meshRotationOffsetEuler = new Vector3(90f, 0f, 0f);

    [Header("--- VFX KHI TRÚNG ---")]
    [Tooltip("Prefab hiệu ứng nổ/va chạm, spawn tại đúng vị trí trúng đạn")]
    public GameObject hitVFXPrefab;
    [Tooltip("Thời gian tự hủy VFX sau khi spawn (giây) - phòng trường hợp prefab VFX không tự hủy sẵn")]
    public float hitVFXLifeTime = 2f;

    private Vector3 direction;
    private PlayerSubmarineController owner;

    public void Launch(Vector3 dir, PlayerSubmarineController shooter)
    {
        direction = dir.normalized;
        owner = shooter;

        // Xoay mesh theo đúng hướng bay + bù thêm offset vì trục dài mặc định của Capsule
        // không nằm theo hướng "forward" như mong muốn.
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(meshRotationOffsetEuler);

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
            SpawnHitVFX(transform.position);
            Destroy(gameObject);
            return;
        }
    }

    void SpawnHitVFX(Vector3 atPosition)
    {
        if (hitVFXPrefab == null) return;

        GameObject vfx = Instantiate(hitVFXPrefab, atPosition, Quaternion.identity);
        Destroy(vfx, hitVFXLifeTime);
    }
}