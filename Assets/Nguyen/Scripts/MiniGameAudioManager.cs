using UnityEngine;

namespace AnimalParty.Audio
{
    public class MiniGameAudioManager : MonoBehaviour
    {
        // Tạo một biến static (Singleton) để các script khác gọi mọi lúc mọi nơi
        public static MiniGameAudioManager Instance;

        [Header("--- Máy Phát Âm Thanh (Audio Sources) ---")]
        [Tooltip("Nguồn phát nhạc nền (Sẽ lặp lại liên tục)")]
        public AudioSource bgmSource;
        
        [Tooltip("Nguồn phát hiệu ứng (SFX - Bắn, Chạm, Nổ...)")]
        public AudioSource sfxSource;

        [Header("--- File Âm Thanh (Audio Clips) ---")]
        public AudioClip backgroundMusic;
        public AudioClip laserHitSound;
        public AudioClip laserAmbientSound;

        private void Awake()
        {
            // Thiết lập Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                // Nếu lỡ tay kéo 2 cục Manager vào Scene, hủy cục dư thừa đi
                Destroy(gameObject); 
            }
        }

        private void Start()
        {
            // Vừa vào game là tự động bật nhạc nền
            PlayBGM();
        }

        // --- CÁC HÀM PHÁT ÂM THANH ---

        public void PlayBGM()
        {
            if (bgmSource != null && backgroundMusic != null)
            {
                bgmSource.clip = backgroundMusic;
                bgmSource.loop = true; // Bật lặp đi lặp lại
                bgmSource.Play();
            }
        }

        // Gọi hàm này khi nhân vật dẫm trúng Lazer
        public void PlayHitLaserSound()
        {
            if (sfxSource != null && laserHitSound != null)
            {
                // PlayOneShot giúp âm thanh đè lên nhau được (2 người cùng chạm lazer thì phát 2 tiếng)
                sfxSource.PlayOneShot(laserHitSound);
            }
        }

        // Gọi hàm này khi Lazer bắt đầu bắn (hiệu ứng "Chíu chíu" hoặc xè xè)
        public void PlayLaserSound()
        {
            if (sfxSource != null && laserAmbientSound != null)
            {
                sfxSource.PlayOneShot(laserAmbientSound);
            }
        }
    }
}