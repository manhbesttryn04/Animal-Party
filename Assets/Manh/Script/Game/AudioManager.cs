using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // Nhạc nền
    public AudioSource sfxSource;     // Hiệu ứng

    [Header("SFX")]
    public AudioClip walkPlayerClip;
    public AudioClip cannonClip;
    public AudioClip diceRollClip;

    [Header("MusicGame")]
    public AudioClip musicMainClip;

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        musicSource.loop = true;
        sfxSource.loop = false;
    }

   

    /// <summary>
    /// Đổi nhạc nền
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    /// <summary>
    /// Phát hiệu ứng âm thanh
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }
}