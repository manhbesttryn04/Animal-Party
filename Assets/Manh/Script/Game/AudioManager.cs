using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // Nhạc nền
    public AudioSource sfxSource;     // Hiệu ứng

    [Header("SFX")]
    [Header("Player SFX")]
    public AudioClip walkPlayerClip;
    public AudioClip diceRollClip;

    [Header("Debuff SFX")]
    public AudioClip cannonClip;
    public AudioClip fallingBom;
    public AudioClip boomClip;
    public AudioClip bebuffRockMagicClip;

    [Header("Buff SFX")]
    public AudioClip buffDeffClip;
    public AudioClip buffMagicClip;
    public AudioClip bonusBuffClip;

    
    [Header("Shop SFX")]
    public AudioClip openShopClip;
    public AudioClip movechooseItemClip;
    public AudioClip buyItemClip;
    public AudioClip openCardRamdomClip;
    public AudioClip noCoinBuyItemClip;

    [Header("UI")]
    public AudioClip openResultPanel;


    

    [Header("MusicGame")]
    public AudioClip musicMainClip;
    public AudioClip musicMiniGame1;
    public AudioClip musicMiniGame2;
    public AudioClip musicMiniGame3;
    public AudioClip musicMiniGame4;
    public AudioClip musicMiniGame5;
    public AudioClip musicMiniGame6;
    public AudioClip musicMiniGame7;
    public AudioClip musicMiniGame8;
    public AudioClip musicMiniGame9;
    public AudioClip musicMiniGame10;

    

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
    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void OpenMusicminiGame(int indexMiniGame)
    {
        if (indexMiniGame == 1)
        {
            PlayMusic(AudioManager.Instance.musicMiniGame1);
        }
        else if (indexMiniGame == 2)
        {
          PlayMusic(AudioManager.Instance.musicMiniGame2);
        }
        else if (indexMiniGame == 3)
        {
            PlayMusic(AudioManager.Instance.musicMiniGame3);
        }
        else if (indexMiniGame == 4)
        {
           PlayMusic(AudioManager.Instance.musicMiniGame4);
        }
        else if (indexMiniGame == 5)
        {
            PlayMusic(AudioManager.Instance.musicMiniGame5);
        }
        else if (indexMiniGame == 6)
        {
            PlayMusic(AudioManager.Instance.musicMiniGame6);
        }
        else if (indexMiniGame == 7)
        {
           PlayMusic(AudioManager.Instance.musicMiniGame7);
        }
        else if (indexMiniGame == 8)
        {
           PlayMusic(AudioManager.Instance.musicMiniGame8);
        }
        else if (indexMiniGame == 9)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMiniGame9);
        }
        else if (indexMiniGame == 10)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMiniGame10);
        }
    }
    
}