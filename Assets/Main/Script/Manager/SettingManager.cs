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
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("UI")]
    public GameObject settingPanel;
    [Header("Graphics")]
    public TMP_Dropdown qualityGraphicDropDown;

    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    [Header("Current Setting Values")]
    [Range(0f, 1f)]
    public float masterValue = 1f;

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
         // Thấp
        new Vector2Int(1280, 720),   // HD

        new Vector2Int(1366, 768),   // HD Laptop

        new Vector2Int(1600, 900),   // HD+
        new Vector2Int(1920, 1080),
        new Vector2Int(1920, 1200),
        new Vector2Int(2560, 1440),
        new Vector2Int(2560, 1600),
        new Vector2Int(3840, 2160)
    };
    [Header("Display Mode")]
    public TMP_Dropdown displayModeDropdown; [Header("Windowed Size")]
    [SerializeField] private int windowedWidth = 1280;
    [SerializeField] private int windowedHeight = 720;

    public bool isOpenAudioClick = false;
    [Header("Open Setting")]
    public bool canOpenSettingByEsc = true;

    // Trạng thái bảng Setting khi mở bằng nút bình thường
    public bool isSettingOpen = false;

    // Trạng thái bảng Setting khi mở bằng ESC trong cutscene
    public bool isEscSettingOpen = false;

    public bool isOpenExitButton = false;


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
            return;
        }
    }
    private void Update()
    {
        if (!canOpenSettingByEsc)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingByEsc();
        }
    }


    private void Start()
    {

        SetupDefaultValue();

        // Tạo danh sách và tự chọn Resolution
        SetupResolutionDropdown();
        SetupDisplayModeDropdown();
        AddListeners();
        ApplySettings();
        ResetSetting();

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(
                OnClickBackToMainMenu

            );
        }
        isOpenAudioClick = true;
    }

    // ==================================================
    // DEFAULT VALUES
    // ==================================================

    private void SetupDefaultValue()
    {
        masterValue = 1f;
        musicValue = 1f;
        sfxValue = 1f;

        if (masterSlider != null)
        {
            masterSlider.minValue = 0f;
            masterSlider.maxValue = 1f;
            masterSlider.value = masterValue;
        }

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
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(
                SetMasterVolume
            );
        }

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
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        }
    }

    private void RemoveListeners()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(
                SetMasterVolume
            );
        }

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
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.RemoveListener(SetDisplayMode);
        }
    }
    private void SetupDisplayModeDropdown()
    {
        if (displayModeDropdown == null)
            return;

        displayModeDropdown.ClearOptions();

        List<string> options = new List<string>()
    {
        "Fullscreen",
        "Borderless",
        "Windowed"
    };

        displayModeDropdown.AddOptions(options);

        int selectedIndex = 0;

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                selectedIndex = 0;
                break;

            case FullScreenMode.FullScreenWindow:
                selectedIndex = 1;
                break;

            case FullScreenMode.Windowed:
                selectedIndex = 2;
                break;
        }

        displayModeDropdown.SetValueWithoutNotify(selectedIndex);
        displayModeDropdown.RefreshShownValue();
    }
    public void SetDisplayMode(int index)
    {
        if (isOpenAudioClick)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(
                    AudioManager.Instance.clickButton
                );
            }
        }

        switch (index)
        {

            // Fullscreen
            case 0:
                {
                    SetResolutionByMode(FullScreenMode.ExclusiveFullScreen);
                    break;
                }

            // Borderless
            case 1:
                {
                    Screen.SetResolution(
                        Display.main.systemWidth,
                        Display.main.systemHeight,
                        FullScreenMode.FullScreenWindow
                    );
                    break;
                }

            // Windowed
            case 2:
                {
                    SetResolutionByMode(FullScreenMode.Windowed);
                    break;
                }
        }
    }
    private void SetResolutionByMode(FullScreenMode mode)
    {
        if (resolutionDropdown == null || availableResolutions.Count == 0)
            return;

        int index = Mathf.Clamp(
            resolutionDropdown.value,
            0,
            availableResolutions.Count - 1
        );

        Vector2Int resolution = availableResolutions[index];

        Screen.SetResolution(
            resolution.x,
            resolution.y,
            mode
        );
    }

    private Vector2Int GetSelectedResolution()
    {
        if (resolutionDropdown != null &&
            availableResolutions.Count > 0)
        {
            int index = Mathf.Clamp(
                resolutionDropdown.value,
                0,
                availableResolutions.Count - 1
            );

            return availableResolutions[index];
        }

        return new Vector2Int(1920, 1080);
    }

    // ==================================================
    // MASTER VOLUME SLIDER
    // ==================================================

    public void SetMasterVolume(float value)
    {
        masterValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMasterSetting(masterValue);
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

        if (isOpenAudioClick)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(
                    AudioManager.Instance.clickButton
                );
            }
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
            AudioManager.Instance.ApplyMasterSetting(masterValue);
            AudioManager.Instance.ApplyMusicSetting(musicValue);
            AudioManager.Instance.ApplySFXSetting(sfxValue);
        }

        if (VolumeManager.Instance != null)
        {
            int graphicValue = qualityGraphicDropDown.value;
            VolumeManager.Instance.SetGraphicsQuality(graphicValue);
        }

        if (displayModeDropdown != null)
        {
            SetDisplayMode(displayModeDropdown.value);
        }

        if (resolutionDropdown != null &&
            availableResolutions.Count > 0)
        {
            SetResolution(resolutionDropdown.value);
        }
    }
    // ==================================================
    // SETTING PANEL
    // ==================================================

    // Dùng cho nút Setting bình thường
    public void ToggleSetting()
    {
        PlaySettingClickSound();

        if (isSettingOpen)
        {
            CloseSettingPanel();
        }
        else
        {
            OpenSettingPanel();
        }
    }

    // Dùng riêng cho phím ESC trong cutscene
    public void ToggleSettingByEsc()
    {
        PlaySettingClickSound();

        if (isEscSettingOpen)
        {
            ResetEscSetting();
        }
        else
        {
            OpenEscSetting();
        }
    }

    private void OpenSettingPanel()
    {
        isSettingOpen = true;
        isEscSettingOpen = false;
        countClick = 1;

       // ShowCursorForSetting();
        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);
        //ClearSelectedUI();
    }

    private void CloseSettingPanel()
    {
        isSettingOpen = false;
        isEscSettingOpen = false;
        countClick = 0;

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();
       // HideCursorAfterSetting();
    }

    private void OpenEscSetting()
    {
        isEscSettingOpen = true;
        isSettingOpen = false;
        countClick = 0;

        ShowCursorForSetting();
        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);
        ClearSelectedUI();
    }

    // Gắn hàm này vào nút đóng nếu bảng được mở bằng ESC
    public void ResetEscSetting()
    {
        isEscSettingOpen = false;
        isSettingOpen = false;
        countClick = 0;

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();
        HideCursorAfterSetting();
    }

    // Reset chung khi đổi scene hoặc muốn đóng toàn bộ Setting
    public void ResetSetting()
    {
        isSettingOpen = false;
        isEscSettingOpen = false;
        countClick = 0;

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();
        HideCursorAfterSetting();
    }

    private void SetSettingPanelActive(bool active)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActiveSettingPanel(active);
        }
        else if (settingPanel != null)
        {
            settingPanel.SetActive(active);
        }
    }

    private void SetExitButtonActive(bool active)
    {
        if (UIManager.Instance != null &&
            UIManager.Instance.exitMainMenuButton != null)
        {
            UIManager.Instance.exitMainMenuButton.SetActive(active);
        }
    }

    private void ShowCursorForSetting()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowGameCursor();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HideCursorAfterSetting()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.HideGameCursor();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void PlaySettingClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }
    }

    private void ClearSelectedUI()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
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
        var cursor = CursorManager.Instance;
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }
        var audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.ZeroAllAudio();
            audio.PauseAudio();
        }
        if (UIManager.Instance != null)
        {
            ResetSetting();
            UIManager.Instance.openSettingPanelButton.SetActive(false);

            UIManager.Instance.canvasNotifi.SetActive(false);
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