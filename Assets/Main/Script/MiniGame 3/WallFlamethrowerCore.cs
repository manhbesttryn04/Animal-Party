using UnityEngine;
using System.Collections.Generic;
public class WallFlamethrowerCore : MonoBehaviour
{
    [Header("--- Kích Hoạt Hệ Thống ---")]
    [Tooltip("Bật/Tắt lửa trực tiếp để test nhanh trong Editor")]
    public bool isFiring = true;

    [Header("--- Cấu Hình Thành Phần (References) ---")]
    [Tooltip("Kéo Particle System hiệu ứng lửa vào đây")]
    public ParticleSystem fireParticles;
    
    [Tooltip("Kéo Box Collider (Vùng gây sát thương) vào đây")]
    public BoxCollider fireCollider;

    [Header("--- Cấu Hình Sát Thương & Vật Lý ---")]
    [Tooltip("Lực hất văng người chơi ra khỏi luồng lửa")]
    public float knockbackForce = 12f;
    
    [Tooltip("Thời gian giãn cách giữa các lần đốt máu (giây)")]
    public float hitCooldown = 0.5f;

    // Bộ nhớ đệm lưu lại thời gian một Collider bị đốt gần nhất
    private readonly Dictionary<Collider, float> _lastHitTimes = new Dictionary<Collider, float>();
    private bool _lastState = false;

    void Start()
    {
        // Đồng bộ trạng thái ban đầu của bẫy
        SyncFlamethrowerState(isFiring);
    }

    void Update()
    {
        // Hỗ trợ cập nhật trạng thái thời gian thực khi bạn tick chọn nút trên Inspector lúc đang chạy game
        if (isFiring != _lastState)
        {
            SyncFlamethrowerState(isFiring);
        }
    }

    public void SyncFlamethrowerState(bool state)
    {
        isFiring = state;
        _lastState = state;
        var audio = AudioManager.Instance;
        if (fireCollider != null) 
        {
            fireCollider.enabled = state;
        }

        if (fireParticles != null)
        {
            if (state)
            {
                fireParticles.Play();
                if (audio)
                {
                    audio.PlaySFXNoOneShot(audio.openFireClip);
                }

            }
            else
            {
                fireParticles.Stop();
                if (audio) { audio.StopSFXNoOneShot(); }
            }
        }

        // Nếu tắt lửa thì dọn dẹp bộ nhớ đệm để sẵn sàng cho lần xịt kế tiếp
        if (!state) 
        {
            _lastHitTimes.Clear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isFiring) return;

        if (!CanHitTarget(other)) return;

        // CHỈ TẬP TRUNG XỬ LÝ ĐẨY VĂNG VẬT LÝ (KNOCKBACK)
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            // Triệt tiêu một phần quán tính cũ
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.2f, 0f, rb.linearVelocity.z * 0.2f);
            
            // Đẩy nhân vật bay theo hướng họng súng
            Vector3 pushDir = transform.forward; 
            pushDir.y = 0.7f; 
            
            rb.AddForce(pushDir * knockbackForce, ForceMode.Impulse);
            
            // Ghi nhận thời gian va chạm
            _lastHitTimes[other] = Time.time;
        }
    }

    private bool CanHitTarget(Collider target)
    {
        if (_lastHitTimes.TryGetValue(target, out float lastTime))
        {
            return (Time.time - lastTime) >= hitCooldown;
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 1. Tự động kiểm tra lỗi lật ngược Scale (Negative Scale) gây hỏng ma trận vật lý
        if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
        {
            Debug.LogError($"<color=red><b>[LỖI NGHIÊM TRỌNG]:</b></color> Object '{gameObject.name}' đang để Scale âm! " +
                           $"Hãy trả Scale về số dương (1, 1, 1) và dùng góc xoay Rotation Y để lật hướng bẫy.");
        }

        // 2. Tự động nhắc nhở nếu quên chưa tích chọn thuộc tính Trigger trên Collider của lửa
        if (fireCollider != null && !fireCollider.isTrigger)
        {
            Debug.LogWarning($"<color=yellow><b>[CẢNH BÁO]:</b></color> BoxCollider trên '{gameObject.name}' chưa bật 'Is Trigger'. " +
                             $"Hệ thống đã tự động bật nó lên để tránh nhân vật bị kẹt vật lý khi va chạm.");
            fireCollider.isTrigger = true;
        }
    }
#endif
}