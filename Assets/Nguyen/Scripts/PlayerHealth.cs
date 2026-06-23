using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace AnimalParty.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("--- Survival Stats ---")]
        public int maxHealth = 3;
        public int currentHealth;

        [Header("--- Physics & Invulnerability ---")]
        public float invulnerabilityTime = 1.2f; // Bất tử 1.2 giây
        private bool isInvincible = false;

        [Header("--- Visual Feedback ---")]
        [Tooltip("Kéo Model 3D của nhân vật (MeshRenderer hoặc SkinnedMeshRenderer) vào đây")]
        public Renderer playerModel; 
        public float blinkInterval = 0.15f; // Tốc độ chớp nháy (giây)

        [Header("--- Events ---")]
        public UnityEvent OnTakeDamage;
        public UnityEvent OnDie;

        private Rigidbody rb;

        private void Awake()
        {
            currentHealth = maxHealth;
            rb = GetComponent<Rigidbody>(); 
        }

        public void TakeDamage()
        {
            if (currentHealth <= 0 || isInvincible) return;

            currentHealth--;
            Debug.Log($"[{gameObject.name}] Ouch! Health: {currentHealth}/{maxHealth}");

            if (AnimalParty.Audio.MiniGameAudioManager.Instance != null)
            {
                AnimalParty.Audio.MiniGameAudioManager.Instance.PlayHitLaserSound();
            }

            OnTakeDamage?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        // CƠ CHẾ NHẤP NHÁY VÀ MIỄN NHIỄM SÁT THƯƠNG
        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;

            // Nếu đã gán Model 3D vào Inspector thì chạy hiệu ứng chớp nháy
            if (playerModel != null)
            {
                float elapsedTime = 0f;
                while (elapsedTime < invulnerabilityTime)
                {
                    // Đảo ngược trạng thái hiển thị của Model
                    playerModel.enabled = !playerModel.enabled;
                    
                    yield return new WaitForSeconds(blinkInterval);
                    elapsedTime += blinkInterval;
                }
                // Đảm bảo khi hết bất tử, nhân vật phải luôn hiện rõ ràng
                playerModel.enabled = true;
            }
            else
            {
                // Nếu bạn quên gán Model thì nó chỉ chờ thời gian thôi, không văng lỗi
                yield return new WaitForSeconds(invulnerabilityTime);
            }

            isInvincible = false;
        }

        private void Die()
        {
            Debug.Log($"[{gameObject.name}] Eliminated!");
            OnDie?.Invoke();

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.AddTorque(new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)), ForceMode.VelocityChange);
            }

            // Đảm bảo model hiển thị lúc chết
            if (playerModel != null) playerModel.enabled = true;

            Destroy(gameObject, 3f);
        }
    }
}