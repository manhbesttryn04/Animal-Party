using UnityEngine;
using UnityEngine.Events;

namespace AnimalParty.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("--- Chỉ số sinh tồn ---")]
        [Tooltip("Số máu tối đa của nhân vật (Mini game thường để 3)")]
        public int maxHealth = 3;
        
        [Tooltip("Số máu hiện tại")]
        public int currentHealth;

        [Header("--- Hiệu ứng kết nối ngoài (UnityEvent) ---")]
        [Tooltip("Gắn hiệu ứng chớp đỏ, âm thanh á á... vào đây")]
        public UnityEvent OnTakeDamage;
        
        [Tooltip("Gắn hiệu ứng nổ tung, hoặc gọi hàm Game Over vào đây")]
        public UnityEvent OnDie;

        private void Start()
        {
            // Vừa vào game là nạp đầy máu
            currentHealth = maxHealth;
        }

        // TÊN HÀM NÀY PHẢI KHỚP 100% VỚI LỆNH SendMessage Ở FILE LaserTrap
        public void TakeDamage()
        {
            // Nếu đã cạn máu rồi thì không làm gì thêm
            if (currentHealth <= 0) return;

            // Trừ 1 máu
            currentHealth--;
            Debug.Log($"<color=orange>[{gameObject.name}] Bị Lazer giật! Máu còn: {currentHealth}/{maxHealth}</color>");
            
            // --- GỌI ÂM THANH TRÚNG ĐÒN (Lazer Hit) Ở ĐÂY ---
            if (AnimalParty.Audio.MiniGameAudioManager.Instance != null)
            {
                AnimalParty.Audio.MiniGameAudioManager.Instance.PlayHitLaserSound();
            }

            // Kích hoạt các hiệu ứng bị thương
            OnTakeDamage?.Invoke();

            // Kiểm tra xem đã chết chưa
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"<color=red>[{gameObject.name}] ĐÃ BỊ LOẠI KHỎI BÀN CHƠI!</color>");
            OnDie?.Invoke();
            
            // Tắt hiển thị nhân vật (Để lúc sau hồi sinh hoặc chờ ván mới)
            gameObject.SetActive(false);
        }
    }
}