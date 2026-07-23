using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown qualityGraphicDropDown;

    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    [Header("Current Setting Values")]
    [Range(0f, 1f)]
    public float musicValue = 1f;

    [Range(0f, 1f)]
    public float sfxValue = 1f;

    [Header("Main Menu")]
    public Button backToMainMenuButton;

    [Header("Buttons")]
    public Button openSettingButton;
    public int countClick = 0;

    [Header("Guide")]
    public Button openGuideButton;
    public int guideClick = 0;

    public int indexScene;

    // Danh sách Resolution thực tế đang có trong Dropdown
    private readonly List<Vector2Int> availableResolutions =
        new List<Vector2Int>();

    // Danh sách Resolution muốn hiển thị
    // Thứ tự từ thấp lên cao
    private readonly Vector2Int[] resolutionPresets =
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1920, 1200),
        new Vector2Int(2560, 1440),
        new Vector2Int(2560, 1600)
    };

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
        var cursor = CursorManager.Instance;

        if (cursor != null)
        {
            cursor.ShowGameCursor();
        }

        SetupDefaultValue();

        // Tạo danh sách và tự chọn Resolution
        SetupResolutionDropdown();

        AddListeners();
        ApplySettings();
        ResetSetting();

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(
                OnClickBackToMainMenu
            );
        }
    }

    // ==================================================
    // DEFAULT VALUES
    // ==================================================

    private void SetupDefaultValue()
    {
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

        // Mặc định Graphics Quality là High
        if (qualityGraphicDropDown != null)
        {
            qualityGraphicDropDown.value = 2;
            qualityGraphicDropDown.RefreshShownValue();
        }
    }

    // ==================================================
    // RESOLUTION SETUP
    // ==================================================

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogWarning(
                "Chưa gắn Resolution Dropdown vào SettingManager."
            );

            return;
        }

        resolutionDropdown.ClearOptions();
        availableResolutions.Clear();

        Resolution[] screenResolutions = Screen.resolutions;

        // Kiểm tra từng Resolution preset có được màn hình hỗ trợ không
        foreach (Vector2Int preset in resolutionPresets)
        {
            if (IsResolutionSupported(
                    preset.x,
                    preset.y,
                    screenResolutions
                ))
            {
                availableResolutions.Add(preset);
            }
        }

        /*
         * Luôn thêm 1920x1080 làm Resolution mặc định
         * nếu Screen.resolutions không trả về nó.
         */
        Vector2Int fullHD = new Vector2Int(1920, 1080);

        if (!availableResolutions.Contains(fullHD))
        {
            availableResolutions.Insert(0, fullHD);
        }

        List<string> resolutionOptions = new List<string>();

        foreach (Vector2Int resolution in availableResolutions)
        {
            resolutionOptions.Add(
                resolution.x + " × " + resolution.y
            );
        }

        resolutionDropdown.AddOptions(resolutionOptions);

        int selectedIndex = FindBestResolutionIndex();

        resolutionDropdown.SetValueWithoutNotify(selectedIndex);
        resolutionDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Kiểm tra Resolution có nằm trong danh sách
    /// màn hình hỗ trợ hay không.
    /// Refresh Rate khác nhau không tạo mục trùng.
    /// </summary>
    private bool IsResolutionSupported(
        int width,
        int height,
        Resolution[] screenResolutions
    )
    {
        foreach (Resolution resolution in screenResolutions)
        {
            if (resolution.width == width &&
                resolution.height == height)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tự chọn Resolution tốt nhất khi mở game.
    ///
    /// Ví dụ:
    /// Màn hình 2560x1600 -> chọn 2560x1600.
    /// Nếu không có -> chọn 2560x1440.
    /// Nếu không có -> chọn 1920x1200.
    /// Cuối cùng -> 1920x1080.
    /// </summary>
    private int FindBestResolutionIndex()
    {
        if (availableResolutions.Count == 0)
        {
            return 0;
        }

        // Độ phân giải thật của màn hình laptop/PC
        int monitorWidth = Display.main.systemWidth;
        int monitorHeight = Display.main.systemHeight;

        // 1. Ưu tiên trùng chính xác với màn hình
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x == monitorWidth &&
                resolution.y == monitorHeight)
            {
                return i;
            }
        }

        /*
         * 2. Nếu không trùng chính xác:
         * tìm Resolution cao nhất nhưng không vượt quá màn hình.
         *
         * Vì danh sách đang sắp từ thấp lên cao,
         * nên duyệt từ cuối về đầu.
         */
        for (int i = availableResolutions.Count - 1; i >= 0; i--)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x <= monitorWidth &&
                resolution.y <= monitorHeight)
            {
                return i;
            }
        }

        // 3. Cuối cùng mặc định 1920x1080
        return FindResolutionIndex(1920, 1080);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x == width &&
                resolution.y == height)
            {
                return i;
            }
        }

        return 0;
    }

    // ==================================================
    // LISTENERS
    // ==================================================

    private void AddListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(
                SetMusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(
                SetSFXVolume
            );
        }

        if (qualityGraphicDropDown != null)
        {
            qualityGraphicDropDown.onValueChanged.AddListener(
                SetGraphicQuality
            );
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(
                SetResolution
            );
        }

        if (openSettingButton != null)
        {
            openSettingButton.onClick.AddListener(
                ToggleSetting
            );
        }

        if (openGuideButton != null)
        {
            openGuideButton.onClick.AddListener(
                ToggleGuide
            );
        }
    }

    private void RemoveListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                SetMusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(
                SetSFXVolume
            );
        }

        if (qualityGraphicDropDown != null)
        {
            qualityGraphicDropDown.onValueChanged.RemoveListener(
                SetGraphicQuality
            );
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(
                SetResolution
            );
        }

        if (openSettingButton != null)
        {
            openSettingButton.onClick.RemoveListener(
                ToggleSetting
            );
        }

        if (openGuideButton != null)
        {
            openGuideButton.onClick.RemoveListener(
                ToggleGuide
            );
        }

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveListener(
                OnClickBackToMainMenu
            );
        }
    }

    // ==================================================
    // MUSIC SLIDER
    // ==================================================

    public void SetMusicVolume(float value)
    {
        musicValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMusicSetting(
                musicValue
            );
        }
    }

    // ==================================================
    // SFX SLIDER
    // ==================================================

    public void SetSFXVolume(float value)
    {
        sfxValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplySFXSetting(
                sfxValue
            );
        }
    }

    // ==================================================
    // GRAPHICS QUALITY
    // 0 = Low
    // 1 = Medium
    // 2 = High
    // ==================================================

    public void SetGraphicQuality(int value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        value = Mathf.Clamp(value, 0, 2);

        if (VolumeManager.Instance != null)
        {
            VolumeManager.Instance.SetGraphicsQuality(
                value
            );
        }
    }

    // ==================================================
    // SET RESOLUTION
    // ==================================================

    public void SetResolution(int index)
    {
        if (availableResolutions.Count == 0)
        {
            return;
        }

        if (index < 0 ||
            index >= availableResolutions.Count)
        {
            return;
        }

        Vector2Int selectedResolution =
            availableResolutions[index];

        /*
         * Giữ nguyên chế độ màn hình hiện tại:
         * Fullscreen, Borderless hoặc Windowed.
         *
         * Refresh Rate không cần chọn riêng.
         */
        Screen.SetResolution(
            selectedResolution.x,
            selectedResolution.y,
            Screen.fullScreenMode
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        Debug.Log(
            "Đã đổi Resolution thành: " +
            selectedResolution.x +
            "x" +
            selectedResolution.y
        );
    }

    // ==================================================
    // APPLY ALL SETTINGS
    // ==================================================

    private void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMusicSetting(
                musicValue
            );

            AudioManager.Instance.ApplySFXSetting(
                sfxValue
            );
        }

        if (VolumeManager.Instance != null)
        {
            int graphicValue = 2;

            if (qualityGraphicDropDown != null)
            {
                graphicValue =
                    qualityGraphicDropDown.value;
            }

            VolumeManager.Instance.SetGraphicsQuality(
                graphicValue
            );
        }

        // Áp dụng Resolution được tự động chọn khi vào game
        if (resolutionDropdown != null &&
            availableResolutions.Count > 0)
        {
            SetResolution(
                resolutionDropdown.value
            );
        }
    }

    // ==================================================
    // SETTING PANEL
    // ==================================================

    public void ToggleSetting()
    {
        countClick++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        if (countClick == 1)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ActiveSettingPanel(true);
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ActiveSettingPanel(false);
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            countClick = 0;
        }
    }

    public void ResetSetting()
    {
        countClick = 0;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActiveSettingPanel(false);
        }
    }

    // ==================================================
    // BACK TO MAIN MENU
    // ==================================================

    public void OnClickBackToMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        StartCoroutine(BackToMainMenuRoutine());
    }

    private IEnumerator BackToMainMenuRoutine()
    {
        var audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.StopAllAudio();
        }

        if (LoadingManager.Instance != null)
        {
            yield return LoadingManager.Instance.ShowLoading();
        }

        if (VolumeManager.Instance != null)
        {
            VolumeManager.Instance.SetGraphicsQuality(2);
        }

        SceneManager.LoadScene(indexScene);
    }

    // ==================================================
    // GUIDE PANEL
    // ==================================================

    public void ToggleGuide()
    {
        guideClick++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        if (guideClick == 1)
        {
            if (UIManager.Instance != null &&
                UIManager.Instance.intructInputBuyPanel != null)
            {
                UIManager.Instance.intructInputBuyPanel.SetActive(true);
            }
        }
        else
        {
            if (UIManager.Instance != null &&
                UIManager.Instance.intructInputBuyPanel != null)
            {
                UIManager.Instance.intructInputBuyPanel.SetActive(false);
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            guideClick = 0;
        }
    }

    public void ResetGuide()
    {
        guideClick = 0;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (UIManager.Instance != null &&
            UIManager.Instance.intructInputBuyPanel != null)
        {
            UIManager.Instance.intructInputBuyPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        RemoveListeners();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}