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
    public int countClick;

    [Header("Guide")]
    public Button openGuideButton;
    public int guideClick;

    public int indexScene;

    private readonly List<Vector2Int> availableResolutions =
        new List<Vector2Int>();

    private readonly Vector2Int[] resolutionPresets =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(1920, 1200),
        new Vector2Int(2560, 1440),
        new Vector2Int(2560, 1600),
        new Vector2Int(3840, 2160)
    };

    [Header("Display Mode")]
    public TMP_Dropdown displayModeDropdown;

    [Header("Windowed Size")]
    [SerializeField] private int windowedWidth = 1280;
    [SerializeField] private int windowedHeight = 720;

    public bool isOpenAudioClick;

    [Header("Open Setting")]
    public bool canOpenSettingByEsc = true;

    public bool isSettingOpen;
    public bool isEscSettingOpen;
    public bool canOpenSettingByController = true;
    public bool isOpenExitButton;

    [Header("Controller Setting Navigation")]
    [Tooltip("Không thêm Background của nút Exit vào danh sách này.")]
    [SerializeField]
    private List<Image> settingItemBackgrounds = new List<Image>();

    [SerializeField] private Color normalItemColor = Color.white;
    [SerializeField] private Color focusedItemColor = Color.cyan;

    [Tooltip("Màu của lựa chọn đang lia trong Dropdown, chưa xác nhận.")]
    [SerializeField]
    private Color dropdownPreviewColor =
        new Color(0.2f, 0.2f, 0.2f, 0.9f);

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
    private int controllerDropdownPreviewValue;

    [Header("Controller Open Setting")]
    [Tooltip("Button 7 thường là nút Menu/Start/3 gạch.")]
    [SerializeField] private int controllerSettingButtonIndex = 7;

    // 0 = không có console
    // 1 = Console 1
    // 2 = Console 2
    private int previousSettingConsole;

    private bool waitControllerSettingButtonRelease;

    public bool IsSettingBlockingInput
    {
        get
        {
            return isSettingOpen || isEscSettingOpen;
        }
    }

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

    private void Start()
    {
        SetupDefaultValue();

        SetupResolutionDropdown();
        SetupDisplayModeDropdown();
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

    private void Update()
    {
        HandleControllerConnectionState();

        int activeConsole = GetActiveSettingConsole();

        // Chỉ cho tay cầm mở Setting ở scene được phép.
        if (activeConsole != 0 &&
            canOpenSettingByController)
        {
            HandleControllerSettingButton();
        }

        // ESC là bàn phím, vẫn hoạt động dù có tay cầm đang cắm.
        if (canOpenSettingByEsc &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingByEsc();
        }

        if (!IsAnySettingPanelOpen())
            return;

        // Không có tay cầm thì sử dụng chuột.
        if (activeConsole == 0)
            return;

        HandleControllerSettingNavigation();
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

        if (qualityGraphicDropDown != null &&
            VolumeManager.Instance != null)
        {
            qualityGraphicDropDown.SetValueWithoutNotify(
                VolumeManager.Instance.GetCurrentQuality()
            );

            qualityGraphicDropDown.RefreshShownValue();
        }
    }

    // ==================================================
    // RESOLUTION
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

        foreach (Vector2Int preset in resolutionPresets)
        {
            if (IsResolutionSupported(
                    preset.x,
                    preset.y,
                    screenResolutions))
            {
                availableResolutions.Add(preset);
            }
        }

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

    private bool IsResolutionSupported(
        int width,
        int height,
        Resolution[] screenResolutions)
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

    private int FindBestResolutionIndex()
    {
        if (availableResolutions.Count == 0)
        {
            return 0;
        }

        int monitorWidth = Display.main.systemWidth;
        int monitorHeight = Display.main.systemHeight;

        for (int i = 0;
             i < availableResolutions.Count;
             i++)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x == monitorWidth &&
                resolution.y == monitorHeight)
            {
                return i;
            }
        }

        for (int i = availableResolutions.Count - 1;
             i >= 0;
             i--)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x <= monitorWidth &&
                resolution.y <= monitorHeight)
            {
                return i;
            }
        }

        return FindResolutionIndex(1920, 1080);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0;
             i < availableResolutions.Count;
             i++)
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

        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(
                SetDisplayMode
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

        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.RemoveListener(
                SetDisplayMode
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
    // DISPLAY MODE
    // ==================================================

    private void SetupDisplayModeDropdown()
    {
        if (displayModeDropdown == null)
        {
            return;
        }

        displayModeDropdown.ClearOptions();

        List<string> options = new List<string>
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
        if (isOpenAudioClick &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance.clickButton
            );
        }

        switch (index)
        {
            case 0:
                SetResolutionByMode(
                    FullScreenMode.ExclusiveFullScreen
                );
                break;

            case 1:
                Screen.SetResolution(
                    Display.main.systemWidth,
                    Display.main.systemHeight,
                    FullScreenMode.FullScreenWindow
                );
                break;

            case 2:
                SetResolutionByMode(
                    FullScreenMode.Windowed
                );
                break;
        }
    }

    private void SetResolutionByMode(FullScreenMode mode)
    {
        if (resolutionDropdown == null ||
            availableResolutions.Count == 0)
        {
            return;
        }

        int index = Mathf.Clamp(
            resolutionDropdown.value,
            0,
            availableResolutions.Count - 1
        );

        Vector2Int resolution =
            availableResolutions[index];

        Screen.SetResolution(
            resolution.x,
            resolution.y,
            mode
        );
    }

    // ==================================================
    // AUDIO
    // ==================================================

    public void SetMasterVolume(float value)
    {
        masterValue = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMasterSetting(
                masterValue
            );
        }
    }

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
    // GRAPHICS
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

        Screen.SetResolution(
            selectedResolution.x,
            selectedResolution.y,
            Screen.fullScreenMode
        );

        if (isOpenAudioClick &&
            AudioManager.Instance != null)
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

    private void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyMasterSetting(
                masterValue
            );

            AudioManager.Instance.ApplyMusicSetting(
                musicValue
            );

            AudioManager.Instance.ApplySFXSetting(
                sfxValue
            );
        }

        if (VolumeManager.Instance != null &&
            qualityGraphicDropDown != null)
        {
            VolumeManager.Instance.SetGraphicsQuality(
                qualityGraphicDropDown.value
            );
        }

        if (displayModeDropdown != null)
        {
            SetDisplayMode(
                displayModeDropdown.value
            );
        }

        if (resolutionDropdown != null &&
            availableResolutions.Count > 0)
        {
            SetResolution(
                resolutionDropdown.value
            );
        }
    }

    // ==================================================
    // CONTROLLER SETTING
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
            displayModeDropdown,
            backToMainMenuButton
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
        return GetActiveSettingConsole() != 0;
    }

    private int GetActiveSettingConsole()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
        {
            return 0;
        }

        // Console 1 luôn được ưu tiên.
        if (controller.IsConsole1Connected())
        {
            return 1;
        }

        // Console 2 chỉ điều khiển khi Console 1 mất kết nối.
        if (controller.IsConsole2Connected())
        {
            return 2;
        }

        return 0;
    }

    private void HandleControllerConnectionState()
    {
        int activeConsole = GetActiveSettingConsole();

        bool hasController = activeConsole != 0;
        bool hadController = previousSettingConsole != 0;

        // Vừa cắm tay cầm.
        if (!hadController && hasController)
        {
            if (IsAnySettingPanelOpen())
            {
                FocusFirstSettingItem();
            }
        }
        // Vừa rút hết tay cầm.
        else if (hadController && !hasController)
        {
            ResetSettingControllerFocus();
            ClearSelectedUI();

            waitControllerSettingButtonRelease = false;

            /*
             * Không gọi ShowGameCursor() ở đây.
             *
             * ControllerManager sẽ báo trạng thái tay cầm
             * cho CursorManager.
             *
             * CursorManager tự hiện chuột nếu:
             * - Setting đang mở.
             * - Không còn tay cầm.
             */
        }
        // Chuyển quyền từ Console 1 sang Console 2
        // hoặc từ Console 2 về Console 1.
        else if (hasController &&
                 activeConsole != previousSettingConsole)
        {
            waitControllerSettingButtonRelease = false;

            if (IsAnySettingPanelOpen())
            {
                FocusFirstSettingItem();
            }
        }

        previousSettingConsole = activeConsole;
    }

    private float GetSettingVerticalInput()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        int activeConsole = GetActiveSettingConsole();

        if (controller == null ||
            activeConsole == 0)
        {
            return 0f;
        }

        return controller.GetConsoleAxisRaw(
            activeConsole,
            verticalP1Axis,
            verticalP2Axis
        );
    }

    private float GetSettingHorizontalInput()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        int activeConsole = GetActiveSettingConsole();

        if (controller == null ||
            activeConsole == 0)
        {
            return 0f;
        }

        return controller.GetConsoleAxisRaw(
            activeConsole,
            horizontalP1Axis,
            horizontalP2Axis
        );
    }

    private bool GetSettingSubmitDown()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        int activeConsole = GetActiveSettingConsole();

        if (controller == null ||
            activeConsole == 0)
        {
            return false;
        }

        return controller.GetConsoleButtonDown(
            activeConsole,
            0
        );
    }

    private bool GetControllerSettingButtonDown()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        int activeConsole = GetActiveSettingConsole();

        if (controller == null ||
            activeConsole == 0)
        {
            return false;
        }

        return controller.GetConsoleButtonDown(
            activeConsole,
            controllerSettingButtonIndex
        );
    }

    private bool GetControllerSettingButtonHeld()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        int activeConsole = GetActiveSettingConsole();

        if (controller == null ||
            activeConsole == 0)
        {
            return false;
        }

        return controller.GetConsoleButton(
            activeConsole,
            controllerSettingButtonIndex
        );
    }

    private void HandleControllerSettingButton()
    {
        if (GetActiveSettingConsole() == 0)
        {
            waitControllerSettingButtonRelease = false;
            return;
        }

        if (waitControllerSettingButtonRelease)
        {
            if (!GetControllerSettingButtonHeld())
            {
                waitControllerSettingButtonRelease = false;
            }

            return;
        }

        if (!GetControllerSettingButtonDown())
        {
            return;
        }

        waitControllerSettingButtonRelease = true;

        if (IsAnySettingPanelOpen())
        {
            CloseControllerSetting();
        }
        else
        {
            OpenControllerSetting();
        }
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
        {
            return;
        }

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
                currentSettingIndex =
                    settingItems.Length - 1;
            }

            if (CanUseSettingItem(
                    settingItems[currentSettingIndex]))
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

            if (CanUseSettingItem(
                    settingItems[currentSettingIndex]))
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
        if (currentSettingIndex < 0 ||
            currentSettingIndex > 2)
        {
            canMoveSettingHorizontal = true;
            return;
        }

        if (Mathf.Abs(horizontal) <=
            controllerResetThreshold)
        {
            canMoveSettingHorizontal = true;
            return;
        }

        if (!canMoveSettingHorizontal)
        {
            return;
        }

        Slider selectedSlider =
            settingItems[currentSettingIndex] as Slider;

        if (selectedSlider == null)
        {
            return;
        }

        float direction =
            horizontal > 0f ? 1f : -1f;

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
        // Ba mục đầu là Slider.
        if (currentSettingIndex <= 2)
        {
            return;
        }

        Selectable selectedItem =
            settingItems[currentSettingIndex];

        Button button = selectedItem as Button;

        if (button != null)
        {
            button.onClick.Invoke();
            return;
        }

        TMP_Dropdown dropdown =
            selectedItem as TMP_Dropdown;

        if (dropdown == null)
        {
            return;
        }

        OpenControllerDropdown(dropdown);
    }

    private void OpenControllerDropdown(
        TMP_Dropdown dropdown)
    {
        if (dropdown == null ||
            dropdown.options == null ||
            dropdown.options.Count == 0)
        {
            return;
        }

        currentControllerDropdown = dropdown;
        isControllerDropdownOpen = true;

        controllerDropdownPreviewValue =
            dropdown.value;

        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;

        dropdown.Show();

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

        if (Mathf.Abs(vertical) <=
            controllerResetThreshold)
        {
            canMoveSettingVertical = true;
            return;
        }

        if (!canMoveSettingVertical)
        {
            return;
        }

        int optionCount =
            currentControllerDropdown.options.Count;

        if (optionCount <= 0)
        {
            return;
        }

        int newValue =
            controllerDropdownPreviewValue;

        if (vertical > controllerInputThreshold)
        {
            newValue--;

            if (newValue < 0)
            {
                newValue = optionCount - 1;
            }
        }
        else if (vertical <
                 -controllerInputThreshold)
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

        controllerDropdownPreviewValue = newValue;

        currentControllerDropdown.SetValueWithoutNotify(
            controllerDropdownPreviewValue
        );

        currentControllerDropdown.RefreshShownValue();

        UpdateDropdownPreviewHighlight();

        canMoveSettingVertical = false;

        PlayControllerMoveSound();
    }

    private void UpdateDropdownPreviewHighlight()
    {
        if (currentControllerDropdown == null)
        {
            return;
        }

        Toggle[] allToggles =
            currentControllerDropdown.transform.root
                .GetComponentsInChildren<Toggle>(true);

        List<Toggle> optionToggles =
            new List<Toggle>();

        foreach (Toggle toggle in allToggles)
        {
            if (toggle == null ||
                !toggle.gameObject.activeInHierarchy)
            {
                continue;
            }

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

        for (int i = 0;
             i < optionToggles.Count;
             i++)
        {
            Toggle toggle = optionToggles[i];
            ColorBlock colors = toggle.colors;

            bool isPreview =
                i == controllerDropdownPreviewValue;

            Color normalColor =
                isPreview
                    ? dropdownPreviewColor
                    : Color.white;

            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;

            toggle.colors = colors;

            if (toggle.targetGraphic != null)
            {
                toggle.targetGraphic.color =
                    normalColor;
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

        TMP_Dropdown dropdown =
            currentControllerDropdown;

        dropdown.SetValueWithoutNotify(
            controllerDropdownPreviewValue
        );

        dropdown.RefreshShownValue();

        dropdown.onValueChanged.Invoke(
            controllerDropdownPreviewValue
        );

        dropdown.Hide();

        isControllerDropdownOpen = false;
        currentControllerDropdown = null;
        controllerDropdownPreviewValue = 0;

        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;

        UpdateSettingControllerFocus();
        PlayControllerClickSound();
    }

    private void CloseControllerDropdownState()
    {
        isControllerDropdownOpen = false;
        currentControllerDropdown = null;
        controllerDropdownPreviewValue = 0;

        canMoveSettingVertical = false;
        canMoveSettingHorizontal = true;
    }

    private void FocusFirstSettingItem()
    {
        if (!HasControllerForSetting())
        {
            return;
        }

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

        for (int i = 0;
             i < settingItemBackgrounds.Count;
             i++)
        {
            Image background =
                settingItemBackgrounds[i];

            if (background == null)
            {
                continue;
            }

            background.color =
                i == currentSettingIndex
                    ? focusedItemColor
                    : normalItemColor;
        }

        Selectable selectedItem =
            settingItems[currentSettingIndex];

        if (!CanUseSettingItem(selectedItem))
        {
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                null
            );

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

    private void OpenControllerSetting()
    {
        PlaySettingClickSound();

        isSettingOpen = true;
        isEscSettingOpen = false;
        countClick = 1;

        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);

        // Báo Setting đang mở cho CursorManager.
        SetCursorSettingState(true);

        FocusFirstSettingItem();
    }

    private void CloseControllerSetting()
    {
        PlaySettingClickSound();

        // Không chạy nút Exit.
        CloseSettingPanel();
    }

    public void ToggleSetting()
    {
        PlaySettingClickSound();

        if (isSettingOpen || isEscSettingOpen)
        {
            CloseSettingPanel();
        }
        else
        {
            OpenSettingPanel();
        }
    }

    public void ToggleSettingByEsc()
    {
        PlaySettingClickSound();

        if (IsAnySettingPanelOpen())
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

        // Báo Setting đang mở.
        SetCursorSettingState(true);

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

        // Báo Setting đã đóng.
        SetCursorSettingState(false);
    }

    private void OpenEscSetting()
    {
        isEscSettingOpen = true;
        isSettingOpen = false;
        countClick = 0;

        SetSettingPanelActive(true);
        SetExitButtonActive(isOpenExitButton);

        // Báo Setting đang mở.
        SetCursorSettingState(true);

        if (GetActiveSettingConsole() != 0)
        {
            FocusFirstSettingItem();
        }
        else
        {
            ClearSelectedUI();
        }
    }

    public void ResetEscSetting()
    {
        isEscSettingOpen = false;
        isSettingOpen = false;
        countClick = 0;

        ResetSettingControllerFocus();

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();

        // Báo Setting đã đóng.
        SetCursorSettingState(false);
    }

    public void ResetSetting()
    {
        isSettingOpen = false;
        isEscSettingOpen = false;
        countClick = 0;

        waitControllerSettingButtonRelease = false;

        ResetSettingControllerFocus();

        SetSettingPanelActive(false);
        SetExitButtonActive(false);
        ClearSelectedUI();

        // Báo Setting đã đóng.
        SetCursorSettingState(false);
    }

    private void SetCursorSettingState(bool open)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetSettingCursorActive(open);
        }
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
            UIManager.Instance.exitMainMenuButton.SetActive(
                active
            );
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
            EventSystem.current.SetSelectedGameObject(
                null
            );
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
        /*
         * Đóng trạng thái Setting trước khi chuyển scene.
         * Không gọi HideGameCursor trực tiếp.
         */
        ResetSetting();

        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.ZeroAllAudio();
            audio.PauseAudio();
        }

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.openSettingPanelButton != null)
            {
                UIManager.Instance.openSettingPanelButton.SetActive(
                    false
                );
            }

            if (UIManager.Instance.canvasNotifi != null)
            {
                UIManager.Instance.canvasNotifi.SetActive(
                    false
                );
            }
        }

        if (LoadingManager.Instance != null)
        {
            yield return LoadingManager.Instance.ShowLoading();
        }

        if (VolumeManager.Instance != null)
        {
            VolumeManager.Instance.SetGraphicsQuality(
                VolumeManager.Instance.currentQuality
            );
        }

        SceneManager.LoadScene(indexScene);
    }

    // ==================================================
    // GUIDE
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
                UIManager.Instance.intructInputBuyPanel.SetActive(
                    true
                );
            }
        }
        else
        {
            if (UIManager.Instance != null &&
                UIManager.Instance.intructInputBuyPanel != null)
            {
                UIManager.Instance.intructInputBuyPanel.SetActive(
                    false
                );
            }

            ClearSelectedUI();

            guideClick = 0;
        }
    }

    public void ResetGuide()
    {
        guideClick = 0;

        ClearSelectedUI();

        if (UIManager.Instance != null &&
            UIManager.Instance.intructInputBuyPanel != null)
        {
            UIManager.Instance.intructInputBuyPanel.SetActive(
                false
            );
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