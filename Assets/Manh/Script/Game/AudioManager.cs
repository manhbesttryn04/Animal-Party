using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Setup List")]
    public List<AudioSetup> audioSetupList = new List<AudioSetup>();
    public AudioSetup mainGameAudioSetup;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // Nhạc nền
    public AudioSource sfxSource;     // Hiệu ứng
    public AudioSource environmentSource; // Môi trường
    public AudioSource specialSource; // Âm thanh đặc biệt


    [Header("SFX")]
    [Header("Player SFX")]
    public AudioClip walkPlayerClip;
    public AudioClip diceRollClip;
    public AudioClip playerTeleport;

    [Header("Debuff SFX")]
    public AudioClip cannonClip;
    public AudioClip fallingBom;
    public AudioClip boomClip;
    public AudioClip bebuffRockMagicClip;
    public AudioClip skipDiceClip;

    [Header("Buff SFX")]
    public AudioClip buffDeffClip;
    public AudioClip buffMagicClip;
    public AudioClip bonusBuffClip;
    public AudioClip coinClip;
    public AudioClip startLeteClip;
    public AudioClip endLeteClip;
  
    
    [Header("Shop SFX")]
    public AudioClip openShopClip;
    public AudioClip movechooseItemClip;
    public AudioClip buyItemClip;
    public AudioClip openCardRamdomClip;
    public AudioClip noCoinBuyItemClip;
    public AudioClip skipBuyClip;
    
    [Header("UI SFX")]
    public AudioClip openResultPanel;
    public AudioClip nextRound;
    [Header("Camera SFX")]
    public AudioClip moveCamera;

    [Header("Winner")]
    public AudioClip winnerClip;
    public AudioClip winnerMiniGameClip1;
    public AudioClip winnerMiniGameClip2;
    public AudioClip fourPowerCoinClip;
    public AudioClip threethirtyIndexClip;

    [Header("Teleport")]
    public AudioClip openTeleportClip;
    public AudioClip closeTeleportClip;


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

    [Header("Minigame 1")]
    public AudioClip loadBrickClip;
    public AudioClip sharkAttackClip;
    public AudioClip warningClip;
    [Header("Minigame 2")]
    public AudioClip brickFallClip;
    public AudioClip javaLoopClip;
    [Header("Minigam 3")]
    public AudioClip laserHitClip;
    public AudioClip laserMoveClip;
    [Header("Minigame 4")]
    public AudioClip scanPiratesClip;
    public AudioClip laughPiratesClip;
    public AudioClip gunShotPiratesClip;
    public AudioClip seaGullClip;
    public AudioClip[] piratesSingClipList;
    [Header("Minigame 5")]
    public AudioClip snowFallClip;
    public AudioClip buffBigClip;
    public AudioClip iceMagicClip;




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
        specialSource.loop = false;
        environmentSource.loop = true;
    }
    private void Start()
    {
        SetupMainGameAudio();
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
    public void PlayEnvironment(AudioClip clip)
    {

        if (clip == null) return;

        if (environmentSource.clip == clip)
            return;

        environmentSource.clip = clip;
        environmentSource.Play();
    }
    public void StopEnvironment()
    {
        if (environmentSource == null) return;

        environmentSource.Stop();
        environmentSource.clip = null;
    }
    public void PlaySpecial(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;

        if (specialSource.clip == clip &&
            specialSource.isPlaying &&
            specialSource.loop == loop)
            return;

        specialSource.Stop();

        specialSource.clip = clip;
        specialSource.loop = loop;
        specialSource.Play();
    }
    public void StopSpecial()
    {
        specialSource.Stop();
        specialSource.clip = null;
        specialSource.loop = false;
    }
    public void PlaySpecialOneShot(AudioClip clip)
    {
        if (clip == null) return;

        specialSource.PlayOneShot(clip);
    }

    public void SetupMusicMiniGame(int indexMiniGame)
    {

        SetupAudioByMiniGame(indexMiniGame);

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
    public void SetupMainGameAudio()
    {
        if (mainGameAudioSetup != null)
        {
            musicSource.volume = mainGameAudioSetup.musicVolume;
            sfxSource.volume = mainGameAudioSetup.sfxVolume;
            environmentSource.volume = mainGameAudioSetup.environmentVolume;
            specialSource.volume = mainGameAudioSetup.specialVolume;
        }
        else
        {
            Debug.LogWarning("Main game audio setup is not assigned.");
        }
    }

    public void SetupAudioByMiniGame(int indexMiniGame)
    {
        int index = indexMiniGame - 1;

        if (index < 0 || index >= audioSetupList.Count)
        {
            Debug.LogWarning("AudioSetup index out of range: " + indexMiniGame);
            return;
        }

        AudioSetup setup = audioSetupList[index];

        musicSource.volume = setup.musicVolume;
        sfxSource.volume = setup.sfxVolume;
        environmentSource.volume = setup.environmentVolume;
        specialSource.volume = setup.specialVolume;
    }
}