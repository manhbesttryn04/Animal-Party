using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown qualityGraphicDropDown;

    [Header("Current Setting Values")]
    [Range(0f, 1f)]
    public float musicValue = 1f;

    [Range(0f, 1f)]
    public float sfxValue = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SetupDefaultValue();
        AddListeners();
        ApplySettings();
    }

    private void SetupDefaultValue()
    {
        // Hai Slider mặc định bằng 1
        musicValue = 1f;
        sfxValue = 1f;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = musicValue;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = sfxValue;
        }

        // Dropdown mặc định High
        if (qualityGraphicDropDown != null)
        {
            qualityGraphicDropDown.value = 2;
            qualityGraphicDropDown.RefreshShownValue();
        }
    }

    private void AddListeners()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (qualityGraphicDropDown != null)
            qualityGraphicDropDown.onValueChanged.AddListener(SetGraphicQuality);
    }

    private void RemoveListeners()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

        if (qualityGraphicDropDown != null)
            qualityGraphicDropDown.onValueChanged.RemoveListener(SetGraphicQuality);
    }

    //==================================================
    // MUSIC SLIDER
    //==================================================

    public void SetMusicVolume(float value)
    {
        musicValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMusicSetting(musicValue);
        }
    }

    //==================================================
    // SFX SLIDER
    //==================================================

    public void SetSFXVolume(float value)
    {
        sfxValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplySFXSetting(sfxValue);
        }
    }

    //==================================================
    // GRAPHIC QUALITY
    // 0 = Low
    // 1 = Medium
    // 2 = High
    //==================================================

    public void SetGraphicQuality(int value)
    {
        value = Mathf.Clamp(value, 0, 2);

        if (VolumeManager.Instance != null)
        {
            VolumeManager.Instance.SetGraphicsQuality(value);
        }
    }

    //==================================================
    // APPLY ALL
    //==================================================

    private void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMusicSetting(musicValue);
            AudioManager.Instance.ApplySFXSetting(sfxValue);
        }

        if (VolumeManager.Instance != null)
        {
            int graphicValue = 2;

            if (qualityGraphicDropDown != null)
                graphicValue = qualityGraphicDropDown.value;

            VolumeManager.Instance.SetGraphicsQuality(graphicValue);
        }
    }

    private void OnDestroy()
    {
        RemoveListeners();

        if (Instance == this)
            Instance = null;
    }
}