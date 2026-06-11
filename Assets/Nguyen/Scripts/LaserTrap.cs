using System.Collections.Generic;
using UnityEngine;

// Sử dụng Namespace giúp code không bị đụng độ tên với các thư viện của đồng đội
namespace AnimalParty.Obstacles 
{
    // Bắt buộc Unity phải có Collider thì script này mới chạy (tránh lỗi ngớ ngẩn quên gắn)
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent] // Ngăn chặn việc lỡ tay kéo 2 script vào cùng 1 object
    public class LaserTrap : MonoBehaviour
    {
        #region CẤU HÌNH TRÊN INSPECTOR
        
        [Header("--- Target Detection ---")]
        [Tooltip("Sử dụng LayerMask để lọc đối tượng va chạm (Tối ưu hiệu năng vật lý hơn dùng Tag)")]
        [SerializeField] private LayerMask targetLayer;

        [Header("--- Trap Settings ---")]
        [Tooltip("Thời gian kháng sát thương tạm thời (giây). Ngăn chặn lỗi tụt máu liên tục trong 1 frame.")]
        [SerializeField, Min(0f)] public float hitCooldown = 0.5f;
        
        [Tooltip("Lực hất văng vật lý (Knockback).")]
        [SerializeField, Range(0f, 100f)] private float knockbackForce = 15f;

        #endregion

        #region BIẾN NỘI BỘ (PRIVATE)
        
        // Bộ nhớ Cache: Lưu lại thời điểm cuối cùng một mục tiêu bị chạm để tính Cooldown
        private readonly Dictionary<Collider, float> _lastHitTimes = new Dictionary<Collider, float>();
        private Collider _trapCollider;

        #endregion

        #region LOGIC VA CHẠM

        private void Awake()
        {
            // Tự động chuyển thành Trigger bằng code, giảm thiểu rủi ro Human Error
            _trapCollider = GetComponent<Collider>();
            _trapCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) => ProcessHit(other);
        
        // Dùng OnTriggerStay để xử lý trường hợp người chơi đứng lỳ trong vùng Lazer
     //   private void OnTriggerStay(Collider other) => ProcessHit(other);

        private void OnTriggerExit(Collider other)
        {
            // Tối ưu bộ nhớ: Xóa mục tiêu khỏi danh sách chờ khi họ đã thoát khỏi Lazer
            if (_lastHitTimes.ContainsKey(other))
            {
                _lastHitTimes.Remove(other);
            }
        }

        #endregion

        #region XỬ LÝ CHÍNH TÂM

        /// <summary>
        /// Bộ lọc trung tâm: Xử lý logic an toàn trước khi áp dụng sát thương
        /// </summary>
        private void ProcessHit(Collider targetCollider)
        {
            // 1. Kiểm tra Layer (Toán tử Bitwise tốc độ cao)
            if ((targetLayer.value & (1 << targetCollider.gameObject.layer)) == 0) return;

            // 2. Kiểm tra Cooldown (Có đang trong trạng thái miễn nhiễm không?)
            if (!CanHitTarget(targetCollider)) return;

            // 3. Thực thi va chạm
            ExecuteDamageAndKnockback(targetCollider);

            // 4. Ghi đè lại thời gian chạm mới nhất
            _lastHitTimes[targetCollider] = Time.time;
        }

        private bool CanHitTarget(Collider targetCollider)
        {
            if (_lastHitTimes.TryGetValue(targetCollider, out float lastTime))
            {
                return (Time.time - lastTime) >= hitCooldown;
            }
            return true;
        }

        private void ExecuteDamageAndKnockback(Collider targetCollider)
        {
            // GỌI HÀM TRỪ MÁU BÊN FILE PlayerHealth.cs
          PlayerMiniGame mini = targetCollider.GetComponentInParent<PlayerMiniGame>();
            if (mini != null)
            {
                mini.UpCoin(0, 1);
            }
            else Debug.Log("ko thay");

               // ApplyKnockback(targetCollider);
        }

        private void ApplyKnockback(Collider targetCollider)
        {
            if (targetCollider.TryGetComponent(out CharacterController cc))
            {
                Vector3 pushDirection =
                    (targetCollider.transform.position - transform.position).normalized;

                pushDirection.y = 0.8f;

                // Đẩy lùi ngay lập tức
                cc.Move(pushDirection * 2f);
            }
        }

        #endregion
    }
}