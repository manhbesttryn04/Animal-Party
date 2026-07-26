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

    [Header("Controller Setting Navigation")]
    [Tooltip("Thứ tự: Master, Music, SFX, Quality, Resolution, Display Mode")]
    [SerializeField] private List<Image> settingItemBackgrounds = new List<Image>();

    [SerializeField] private Color normalItemColor = Color.white;
    [SerializeField] private Color focusedItemColor = Color.cyan;

    [Tooltip("Màu của lựa chọn đang lia trong Dropdown, chưa xác nhận.")]
    [SerializeField] private Color dropdownPreviewColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    [Range(0.01f, 0.5f)]
    [SerializeField] private float sliderControllerStep = 0.05f;

    [Range(0.1f, 1f)]
    [SerializeField] private float controllerInputThreshold = 0.5f;

    [Range(0f, 0.5f)]
    [SerializeField] private float controllerResetThreshold = 0.2f;

    [SerializeField] private string verticalP1Axis = "VerticalP1";
    [SerializeField] private string horizontalP1Axis = "HorizontalP1";
    [SerializeField] private string verticalP2Axis = "VerticalP2";
    [SerializeField] private string horizontalP2Axis = "HorizontalP2";

    private Selectable[] settingItems;
    private int currentSettingIndex;

    private bool canMoveSettingVertical = true;
    private bool canMoveSettingHorizontal = true;

    private bool isControllerDropdownOpen;
    private TMP_Dropdown currentControllerDropdown;

    // Giá trị đang lia thử trong Dropdown.
    // Chỉ áp dụng thật khi nhấn Button 0.
    private int controllerDropdownPreviewValue;

    private bool hadControllerLastFrame;


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
        if (canOpenSettingByEsc &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingByEsc();
        }

        HandleControllerConnectionState();

        if (!IsAnySettingPanelOpen())
            return;

        if (!HasControllerForSetting())
            return;

        HandleControllerSettingNavigation();
    }


    private void Start()
    {

        SetupDefaultValue();

        // Tạo danh sách và tự chọn Resolution
        SetupResolutionDropdown();
        SetupDisplayModeDropdown();

        // Tạo danh sách 6 mục Setting cho tay cầm
        SetupControllerSettingItems();

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
            if (qualityGraphicDropDown != null &&
      VolumeManager.Instance != null)
            {
                qualityGraphicDropDown.SetValueWithoutNotify(
                    VolumeManager.Instance.GetCurrentQuality()
                );

                qualityGraphicDropDown.RefreshShownValue();
            }
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
    // CONTROLLER SETTING NAVIGATION
    // ==================================================

    private void SetupControllerSettingItems()
    {
        settingItems = new Selectable[]
        {
        musicSlider,
        sfxSlider,
        masterSlider,
        qualityGraphicDropDown,
        resolutionDropdown,
        displayModeDropdown
        };

        currentSettingIndex = 0;

        UpdateSettingControllerFocus();
    }

    private bool IsAnySettingPanelOpen()
    {
        return isSettingOpen || isEscSettingOpen;
    }

    private bool HasControllerForSetting()
    {
        if (ControllerManager.Instance == null)
            return false;

        return ControllerManager.Instance.HasAnyController();
    }

    private bool IsConsole1Available()
    {
        if (ControllerManager.Instance == null)
            return false;

        return ControllerManager.Instance.IsConsole1Connected();
    }

    private bool IsConsole2Available()
    {
        if (ControllerManager.Instance == null)
            return false;

        return ControllerManager.Instance.IsConsole2Connected();
    }

    private void HandleControllerConnectionState()
    {
        bool hasController = HasControllerForSetting();

        // Vừa cắm tay cầm khi bảng Setting đang mở:
        // tự focus vào mục đầu tiên.
        if (hasController &&
            !hadControllerLastFrame &&
            IsAnySettingPanelOpen())
        {
            FocusFirstSettingItem();
        }
        // Không còn tay cầm:
        // bỏ focus và đóng Dropdown nếu đang mở.
        else if (!hasController &&
                 hadControllerLastFrame)
        {
            ResetSettingControllerFocus();
            ClearSelectedUI();
        }

        hadControllerLastFrame = hasController;
    }

    private float GetSettingVerticalInput()
    {
        // Console 1 luôn được ưu tiên.
        if (IsConsole1Available())
        {
            return Input.GetAxisRaw(verticalP1Axis);
        }

        // Console 1 bị rút thì Console 2 được điều khiển.
        if (IsConsole2Available())
        {
            return Input.GetAxisRaw(verticalP2Axis);
        }

        return 0f;
    }

    private float GetSettingHorizontalInput()
    {
        if (IsConsole1Available())
        {
            return Input.GetAxisRaw(horizontalP1Axis);
        }

        if (IsConsole2Available())
        {
            return Input.GetAxisRaw(horizontalP2Axis);
        }

        return 0f;
    }

    private bool GetSettingSubmitDown()
    {
        // Console 1 có mặt thì chỉ Console 1 được quyền nhấn.
        if (IsConsole1Available())
        {
            return Input.GetKeyDown(KeyCode.Joystick1Button0);
        }

        // Chỉ khi Console 1 không còn thì Console 2 mới được quyền nhấn.
        if (IsConsole2Available())
        {
            return Input.GetKeyDown(KeyCode.Joystick2Button0);
        }

        return false;
    }

    private void HandleControllerSettingNavigation()
    {
        if (settingItems == null ||
            settingItems.Length == 0)
        {
            return;
        }

        float vertical = GetSettingVerticalInput();
        float horizontal = GetSettingHorizontalInput();

        // Khi Dropdown đang mở, Vertical chỉ được dùng trong Dropdown.
        if (isControllerDropdownOpen)
        {
            HandleOpenedDropdown(vertical);

            if (GetSettingSubmitDown())
            {
                ConfirmControllerDropdown();
            }

            return;
        }

        HandleSettingVertical(vertical);
        HandleSettingHorizontal(horizontal);

        if (GetSettingSubmitDown())
        {
            HandleSettingSubmit();
        }
    }

    private void HandleSettingVertical(float vertical)
    {
        if (Mathf.Abs(vertical) <= controllerResetThreshold)
        {
            canMoveSettingVertical = true;
            return;
        }

        if (!canMoveSettingVertical)
            return;

        if (vertical > controllerInputThreshold)
        {
            MoveToPreviousSettingItem();
            canMoveSettingVertical = false;
        }
        else if (vertical < -controllerInputThreshold)
        {
            MoveToNextSettingItem();
            canMoveSettingVertical = false;
        }
    }

    private void MoveToPreviousSettingItem()
    {
        int startIndex = currentSettingIndex;

        do
        {
            currentSettingIndex--;

            if (currentSettingIndex < 0)
            {
                currentSettingIndex = settingItems.Length - 1;
            }

            if (CanUseSettingItem(settingItems[currentSettingIndex]))
            {
                UpdateSettingControllerFocus();
                PlayControllerMoveSound();
                return;
            }

        } while (currentSettingIndex != startIndex);
    }

    private void MoveToNextSettingItem()
    {
        int startIndex = currentSettingIndex;

        do
        {
            currentSettingIndex++;

            if (currentSettingIndex >= settingItems.Length)
            {
                currentSettingIndex = 0;
            }

            if (CanUseSettingItem(settingItems[currentSettingIndex]))
            {
                UpdateSettingControllerFocus();
                PlayControllerMoveSound();
                return;
            }

        } while (currentSettingIndex != startIndex);
    }

    private bool CanUseSettingItem(Selectable item)
    {
        return item != null &&
               item.gameObject.activeInHierarchy &&
               item.interactable;
    }

    private void HandleSettingHorizontal(float horizontal)
    {
        // Chỉ ba phần tử đầu là Slider.
        if (currentSettingIndex < 0 ||
            currentSettingIndex > 2)
        {
            canMoveSettingHorizontal = true;
            return;
        }

        if (Mathf.Abs(horizontal) <= controllerResetThreshold)
        {
            canMoveSettingHorizontal = true;
            return;
        }

        if (!canMoveSettingHorizontal)
            return;

        Slider selectedSlider =
            settingItems[currentSettingIndex] as Slider;

        if (selectedSlider == null)
            return;

        float direction = horizontal > 0f ? 1f : -1f;

        float newValue =
            selectedSlider.value +
            direction * sliderControllerStep;

        newValue = Mathf.Clamp(
            newValue,
            selectedSlider.minValue,
            selectedSlider.maxValue
        );

        selectedSlider.value = newValue;

        canMoveSettingHorizontal = false;

        PlayControllerMoveSound();
    }

    private void HandleSettingSubmit()
    {
        // Ba mục đầu là Slider nên không cần Button 0.
        if (currentSettingIndex <= 2)
            return;

        TMP_Dropdown dropdown =
            settingItems[currentSettingIndex] as TMP_Dropdown;

        if (dropdown == null)
            return;

        OpenControllerDropdown(dropdown);
    }

    private void OpenControllerDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null ||
            dropdown.options == null ||
            dropdown.options.Count == 0)
        {
            return;
        }

        currentControllerDropdown = dropdown;
        isControllerDropdownOpen = true;

        // Lưu giá trị hiện tại làm giá trị xem trước.
        // Chưa áp dụng Graphics/Resolution/Display Mode ở bước này.
        controllerDropdownPreviewValue = dropdown.value;

        // Phải trả Vertical về giữa trước khi di chuyển lựa chọn.
        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;

        dropdown.Show();

        // Làm tối lựa chọn hiện đang được lia tới.
        UpdateDropdownPreviewHighlight();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                dropdown.gameObject
            );
        }

        PlayControllerClickSound();
    }

    private void HandleOpenedDropdown(float vertical)
    {
        if (currentControllerDropdown == null)
        {
            CloseControllerDropdownState();
            return;
        }

        if (Mathf.Abs(vertical) <= controllerResetThreshold)
        {
            canMoveSettingVertical = true;
            return;
        }

        if (!canMoveSettingVertical)
            return;

        int optionCount =
            currentControllerDropdown.options.Count;

        if (optionCount <= 0)
            return;

        int newValue = controllerDropdownPreviewValue;

        if (vertical > controllerInputThreshold)
        {
            newValue--;

            if (newValue < 0)
            {
                newValue = optionCount - 1;
            }
        }
        else if (vertical < -controllerInputThreshold)
        {
            newValue++;

            if (newValue >= optionCount)
            {
                newValue = 0;
            }
        }
        else
        {
            return;
        }

        // Chỉ đổi phần hiển thị, không gọi onValueChanged.
        // Vì vậy Graphics/Resolution/Display Mode chưa được áp dụng.
        controllerDropdownPreviewValue = newValue;
        currentControllerDropdown.SetValueWithoutNotify(
            controllerDropdownPreviewValue
        );
        currentControllerDropdown.RefreshShownValue();

        // Chỉ đổi màu lựa chọn đang lia, chưa áp dụng Setting.
        UpdateDropdownPreviewHighlight();

        canMoveSettingVertical = false;

        PlayControllerMoveSound();
    }


    private void UpdateDropdownPreviewHighlight()
    {
        if (currentControllerDropdown == null)
            return;

        // TMP_Dropdown tạo một object tên "Dropdown List" khi mở.
        // Tìm các Toggle thuộc đúng danh sách đó.
        Toggle[] allToggles =
            currentControllerDropdown.transform.root
                .GetComponentsInChildren<Toggle>(true);

        List<Toggle> optionToggles = new List<Toggle>();

        foreach (Toggle toggle in allToggles)
        {
            if (toggle == null || !toggle.gameObject.activeInHierarchy)
                continue;

            Transform current = toggle.transform;
            bool belongsToDropdownList = false;

            while (current != null)
            {
                if (current.name == "Dropdown List")
                {
                    belongsToDropdownList = true;
                    break;
                }

                current = current.parent;
            }

            if (belongsToDropdownList)
            {
                optionToggles.Add(toggle);
            }
        }

        for (int i = 0; i < optionToggles.Count; i++)
        {
            Toggle toggle = optionToggles[i];
            ColorBlock colors = toggle.colors;

            bool isPreview = i == controllerDropdownPreviewValue;

            Color normalColor = isPreview
                ? dropdownPreviewColor
                : Color.white;

            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;

            toggle.colors = colors;

            if (toggle.targetGraphic != null)
            {
                toggle.targetGraphic.color = normalColor;
            }
        }
    }

    private void ConfirmControllerDropdown()
    {
        if (currentControllerDropdown == null)
        {
            CloseControllerDropdownState();
            return;
        }

        // Nhấn Button 0 mới xác nhận và áp dụng giá trị.
        currentControllerDropdown.SetValueWithoutNotify(
            controllerDropdownPreviewValue
        );
        currentControllerDropdown.RefreshShownValue();

        // Gọi listener đúng 1 lần sau khi đã xác nhận.
        currentControllerDropdown.onValueChanged.Invoke(
            controllerDropdownPreviewValue
        );

        currentControllerDropdown.Hide();

        CloseControllerDropdownState();
        UpdateSettingControllerFocus();

        PlayControllerClickSound();
    }

    private void CloseControllerDropdownState()
    {
        isControllerDropdownOpen = false;
        currentControllerDropdown = null;
        controllerDropdownPreviewValue = 0;

        // Phải thả cần Vertical rồi mới di chuyển sang mục khác.
        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;
    }

    private void FocusFirstSettingItem()
    {
        if (!HasControllerForSetting())
            return;

        if (settingItems == null ||
            settingItems.Length == 0)
        {
            SetupControllerSettingItems();
        }

        currentSettingIndex = 0;

        isControllerDropdownOpen = false;
        currentControllerDropdown = null;

        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;

        UpdateSettingControllerFocus();
    }

    private void UpdateSettingControllerFocus()
    {
        if (settingItems == null ||
            settingItems.Length == 0)
        {
            return;
        }

        currentSettingIndex = Mathf.Clamp(
            currentSettingIndex,
            0,
            settingItems.Length - 1
        );

        // Đổi màu Image cha của 6 mục Setting.
        for (int i = 0;
             i < settingItemBackgrounds.Count;
             i++)
        {
            Image background =
                settingItemBackgrounds[i];

            if (background == null)
                continue;

            background.color =
                i == currentSettingIndex
                    ? focusedItemColor
                    : normalItemColor;
        }

        Selectable selectedItem =
            settingItems[currentSettingIndex];

        if (!CanUseSettingItem(selectedItem))
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(
                selectedItem.gameObject
            );
        }
    }

    private void ResetSettingControllerFocus()
    {
        if (currentControllerDropdown != null)
        {
            currentControllerDropdown.Hide();
        }

        isControllerDropdownOpen = false;
        currentControllerDropdown = null;

        canMoveSettingVertical = true;
        canMoveSettingHorizontal = true;

        for (int i = 0;
             i < settingItemBackgrounds.Count;
             i++)
        {
            if (settingItemBackgrounds[i] != null)
            {
                settingItemBackgrounds[i].color =
                    normalItemColor;
            }
        }
    }

    private void PlayControllerMoveSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.movechooseItemClip
            );
        }
    }

    private void PlayControllerClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
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

        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);

        if (HasControllerForSetting())
        {
            FocusFirstSettingItem();
        }
        else
        {
            ClearSelectedUI();
        }
    }

    private void CloseSettingPanel()
    {
        isSettingOpen = false;
        isEscSettingOpen = false;
        countClick = 0;

        ResetSettingControllerFocus();

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();
    }

    private void OpenEscSetting()
    {
        isEscSettingOpen = true;
        isSettingOpen = false;
        countClick = 0;

        ShowCursorForSetting();
        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);

        if (HasControllerForSetting())
        {
            FocusFirstSettingItem();
        }
        else
        {
            ClearSelectedUI();
        }
    }

    // Gắn hàm này vào nút đóng nếu bảng được mở bằng ESC
    public void ResetEscSetting()
    {
        isEscSettingOpen = false;
        isSettingOpen = false;
        countClick = 0;

        ResetSettingControllerFocus();

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

        ResetSettingControllerFocus();

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
            VolumeManager.Instance.SetGraphicsQuality(VolumeManager.Instance.currentQuality);
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