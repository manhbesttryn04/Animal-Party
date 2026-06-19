using UnityEngine;

public class AudioManager1 : MonoBehaviour
{
    public static AudioManager1 Instance;

    [Header("--- NHẠC NỀN (Background Music) ---")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.4f;

    [Header("--- ÂM THANH MÔI TRƯỜNG (Sóng biển + Hải âu) ---")]
    public AudioClip ambientSound;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;

    [Header("--- ÂM THANH CLICK BUTTON ---")]
    public AudioClip clickSound;
    [Range(0f, 1f)] public float clickVolume = 0.7f;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // Singleton - giữ AudioManager xuyên suốt các scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       // DontDestroyOnLoad(gameObject);

        // Tạo 3 AudioSource riêng cho 3 loại âm thanh
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = ambientVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = clickVolume;
    }

    void Start()
    {
        // Tự động phát nhạc nền + ambient khi vào Main Menu
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        if (ambientSound != null)
        {
            ambientSource.clip = ambientSound;
            ambientSource.Play();
        }
    }

    // ====== GỌI KHI CLICK BUTTON ======
    public void PlayClick()
    {
        if (clickSound != null)
            sfxSource.PlayOneShot(clickSound, clickVolume);
    }

    // ====== TẮT NHẠC NỀN (gọi khi chuyển sang GameScene) ======
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ====== TẮT ÂM THANH MÔI TRƯỜNG (nếu cần) ======
    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    // ====== ĐIỀU CHỈNH VOLUME (gắn vào Slider nếu cần) ======
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        musicSource.volume = value;
    }

    public void SetAmbientVolume(float value)
    {
        ambientVolume = value;
        ambientSource.volume = value;
    }

    public void SetSfxVolume(float value)
    {
        clickVolume = value;
        sfxSource.volume = value;
    }

    // ====== TẠM DỪNG / TIẾP TỤC (khi mở pause menu chẳng hạn) ======
    public void PauseAll()
    {
        musicSource.Pause();
        ambientSource.Pause();
    }

    public void ResumeAll()
    {
        musicSource.UnPause();
        ambientSource.UnPause();
    }
}