using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip(
        "Tên scene chứa màn chọn nhân vật, " +
        "phải đúng tên trong Build Settings"
    )]
    public string gameSceneName = "GameScene";

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button settingButton;
    public Button exitButton;

    [Header("Controller Axis")]
    [Tooltip("Axis dọc của Joystick 1 trong Legacy Input Manager.")]
    [SerializeField] private string verticalP1AxisName = "VerticalP1";

    [Tooltip("Axis dọc của Joystick 2 trong Legacy Input Manager.")]
    [SerializeField] private string verticalP2AxisName = "VerticalP2";

    [Range(0.1f, 1f)]
    [SerializeField] private float inputThreshold = 0.5f;

    [Range(0f, 0.5f)]
    [SerializeField] private float resetThreshold = 0.2f;

    [Header("Focus")]
    [SerializeField] private bool focusStartButtonOnOpen = true;

    private Button[] menuButtons;
    private int currentButtonIndex;

    private bool canMoveVertical = true;
    private bool isLoading;

    // Dùng để phát hiện Settings vừa đóng.
    private bool wasSettingOpen;

    // Trạng thái tay cầm ở lần kiểm tra trước.
    private bool previousConsole1Connected;
    private bool previousConsole2Connected;

    // Lưu joystick slot trước đó để phát hiện Unity đổi slot
    // dù trạng thái Connected vẫn là true.
    private int previousConsole1JoystickIndex;
    private int previousConsole2JoystickIndex;

    // Tay cầm hiện đang điều khiển menu:
    // 0 = không có
    // 1 = Console 1
    // 2 = Console 2
    private int activeMenuController;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupButtons();
        SetupCursor();
        SetupAudio();
        SetupSetting();
        SetupVolume();

        InitializeControllerState();

        if (focusStartButtonOnOpen &&
            ControllerManager.Instance != null &&
            ControllerManager.Instance.HasAnyController())
        {
            StartCoroutine(
                FocusButtonDelay(startButton)
            );
        }
        var ui = UIManager.Instance;
        if (ui != null)
        {
            ui.isShowKeyBoard = false;
        }
    }

    private void Update()
    {
        if (isLoading)
            return;

        UpdateControllerState();

        if (activeMenuController == 0)
            return;

        // Khi Settings đang mở, khóa toàn bộ điều khiển Main Menu phía sau.
        // Nút B/Circle được SettingManager tự xử lý.
        if (IsSettingOpen())
        {
            wasSettingOpen = true;
            return;
        }

        // Settings vừa đóng:
        // trả focus về nút Settings và chờ analog thả về giữa
        // trước khi cho Main Menu di chuyển tiếp.
        if (wasSettingOpen)
        {
            wasSettingOpen = false;
            canMoveVertical = false;

            StartCoroutine(
                FocusButtonDelay(settingButton)
            );

            return;
        }

        HandleVerticalInput();
        HandleSubmitInput();
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupButtons()
    {
        menuButtons = new Button[]
        {
            startButton,
            settingButton,
            exitButton
        };

        currentButtonIndex = 0;
    }

    private void SetupCursor()
    {
        CursorManager cursor = CursorManager.Instance;

        if (cursor == null)
            return;

        cursor.SetSceneCursorVisible(true);
    }

    private void SetupAudio()
    {
        AudioManager audio = AudioManager.Instance;

        if (audio == null)
            return;

        audio.PlayMusic(audio.musicMainMenuClip);
        audio.PlayEnvironment(audio.theNightClip);
        audio.SetupMainGameAudio();
    }

    private void SetupSetting()
    {
        SettingManager setting = SettingManager.Instance;

        if (setting != null)
        {
            setting.isOpenExitButton = false;
            setting.canOpenSettingByController = false;
        }
    }

    private void SetupVolume()
    {
        VolumeManager volume = VolumeManager.Instance;

        if (volume != null)
        {
            volume.ResetVignette();
            volume.ResetDepthBlur();
        }
    }

    // =========================================================
    // CONTROLLER STATE
    // =========================================================

    private void InitializeControllerState()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
        {
            previousConsole1Connected = false;
            previousConsole2Connected = false;

            previousConsole1JoystickIndex = 0;
            previousConsole2JoystickIndex = 0;

            activeMenuController = 0;
            return;
        }

        previousConsole1Connected =
            controller.IsConsole1Connected();

        previousConsole2Connected =
            controller.IsConsole2Connected();

        previousConsole1JoystickIndex =
            controller.GetConsole1JoystickIndex();

        previousConsole2JoystickIndex =
            controller.GetConsole2JoystickIndex();

        SelectActiveMenuController();
    }

    private void UpdateControllerState()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
        {
            SetNoControllerState();
            return;
        }

        bool console1Connected =
            controller.IsConsole1Connected();

        bool console2Connected =
            controller.IsConsole2Connected();

        int console1JoystickIndex =
            controller.GetConsole1JoystickIndex();

        int console2JoystickIndex =
            controller.GetConsole2JoystickIndex();

        bool connectionChanged =
            console1Connected != previousConsole1Connected ||
            console2Connected != previousConsole2Connected;

        bool joystickIndexChanged =
            console1JoystickIndex != previousConsole1JoystickIndex ||
            console2JoystickIndex != previousConsole2JoystickIndex;

        if (!connectionChanged &&
            !joystickIndexChanged)
        {
            return;
        }

        previousConsole1Connected =
            console1Connected;

        previousConsole2Connected =
            console2Connected;

        previousConsole1JoystickIndex =
            console1JoystickIndex;

        previousConsole2JoystickIndex =
            console2JoystickIndex;

        SelectActiveMenuController();

        // Sau khi cắm/rút hoặc đổi joystick slot,
        // yêu cầu thả analog về giữa trước khi di chuyển tiếp.
        canMoveVertical = false;

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.UpdateCursorByControllerState();
        }

        bool hasAnyController =
            console1Connected ||
            console2Connected;

        if (!hasAnyController)
        {
            ClearControllerFocus();
            return;
        }

        // Khi vừa cắm tay cầm hoặc Unity đổi joystick slot,
        // focus lại nút Start.
        StartCoroutine(
            FocusButtonDelay(startButton)
        );
    }

    private void SelectActiveMenuController()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
        {
            activeMenuController = 0;
            return;
        }

        // Luôn ưu tiên Console 1.
        if (controller.IsConsole1Connected())
        {
            activeMenuController = 1;

            Debug.Log(
                "Main Menu dùng Console 1 | Joystick " +
                controller.GetConsole1JoystickIndex()
            );

            return;
        }

        // Nếu Console 1 bị rút nhưng Console 2 vẫn còn,
        // Console 2 được phép điều khiển menu.
        if (controller.IsConsole2Connected())
        {
            activeMenuController = 2;

            Debug.Log(
                "Main Menu dùng Console 2 | Joystick " +
                controller.GetConsole2JoystickIndex()
            );

            return;
        }

        activeMenuController = 0;

        Debug.Log(
            "Main Menu không có tay cầm."
        );
    }

    private void SetNoControllerState()
    {
        if (activeMenuController == 0 &&
            !previousConsole1Connected &&
            !previousConsole2Connected)
        {
            return;
        }

        activeMenuController = 0;

        previousConsole1Connected = false;
        previousConsole2Connected = false;

        previousConsole1JoystickIndex = 0;
        previousConsole2JoystickIndex = 0;

        canMoveVertical = true;

        ClearControllerFocus();

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.UpdateCursorByControllerState();
        }
    }

    // =========================================================
    // GET ACTIVE CONTROLLER INPUT
    // =========================================================

    private float GetActiveVerticalInput()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null ||
            activeMenuController == 0)
        {
            return 0f;
        }

        // ControllerManager lấy joystick index thật của Console
        // rồi chọn VerticalP1 hoặc VerticalP2 tương ứng.
        return controller.GetConsoleAxisRaw(
            activeMenuController,
            verticalP1AxisName,
            verticalP2AxisName
        );
    }

    private bool GetActiveSubmitDown()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null ||
            activeMenuController == 0)
        {
            return false;
        }

        // Button 0:
        // Xbox A / PlayStation Cross.
        return controller.GetConsoleButtonDown(
            activeMenuController,
            0
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private bool IsSettingOpen()
    {
        SettingManager setting =
            SettingManager.Instance;

        if (setting == null)
            return false;

        return setting.isSettingOpen ||
               setting.isEscSettingOpen;
    }

    // =========================================================
    // VERTICAL INPUT
    // =========================================================

    private void HandleVerticalInput()
    {
        if (menuButtons == null ||
            menuButtons.Length == 0)
        {
            return;
        }

        float vertical =
            GetActiveVerticalInput();

        if (Mathf.Abs(vertical) <= resetThreshold)
        {
            canMoveVertical = true;
            return;
        }

        if (!canMoveVertical)
            return;

        if (vertical > inputThreshold)
        {
            MoveToPreviousButton();
            canMoveVertical = false;
        }
        else if (vertical < -inputThreshold)
        {
            MoveToNextButton();
            canMoveVertical = false;
        }
    }

    private void MoveToPreviousButton()
    {
        if (menuButtons == null ||
            menuButtons.Length == 0)
        {
            return;
        }

        int startIndex = currentButtonIndex;

        do
        {
            currentButtonIndex--;

            if (currentButtonIndex < 0)
            {
                currentButtonIndex =
                    menuButtons.Length - 1;
            }

            Button button =
                menuButtons[currentButtonIndex];

            if (CanSelectButton(button))
            {
                FocusButton(button);
                PlayMoveSound();
                return;
            }
        }
        while (currentButtonIndex != startIndex);
    }

    private void MoveToNextButton()
    {
        if (menuButtons == null ||
            menuButtons.Length == 0)
        {
            return;
        }

        int startIndex = currentButtonIndex;

        do
        {
            currentButtonIndex++;

            if (currentButtonIndex >=
                menuButtons.Length)
            {
                currentButtonIndex = 0;
            }

            Button button =
                menuButtons[currentButtonIndex];

            if (CanSelectButton(button))
            {
                FocusButton(button);
                PlayMoveSound();
                return;
            }
        }
        while (currentButtonIndex != startIndex);
    }

    private bool CanSelectButton(
        Button button)
    {
        return button != null &&
               button.gameObject.activeInHierarchy &&
               button.interactable;
    }

    // =========================================================
    // SUBMIT BUTTON
    // =========================================================

    private void HandleSubmitInput()
    {
        if (!GetActiveSubmitDown())
            return;

        if (EventSystem.current == null)
            return;

        GameObject selectedObject =
            EventSystem.current
                .currentSelectedGameObject;

        if (selectedObject == null)
        {
            FocusButton(startButton);
            return;
        }

        Button selectedButton =
            selectedObject.GetComponent<Button>();

        if (selectedButton == null ||
            !selectedButton.interactable)
        {
            return;
        }

        selectedButton.onClick.Invoke();
    }

    // =========================================================
    // FOCUS
    // =========================================================

    private IEnumerator FocusButtonDelay(
        Button targetButton)
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (activeMenuController == 0)
            yield break;

        FocusButton(targetButton);
    }

    private void FocusButton(
        Button targetButton)
    {
        if (EventSystem.current == null)
        {
            Debug.LogError(
                "Không tìm thấy EventSystem trong scene."
            );

            return;
        }

        if (!CanSelectButton(targetButton))
            return;

        EventSystem.current
            .SetSelectedGameObject(null);

        EventSystem.current
            .SetSelectedGameObject(
                targetButton.gameObject
            );

        int index =
            System.Array.IndexOf(
                menuButtons,
                targetButton
            );

        if (index >= 0)
        {
            currentButtonIndex = index;
        }
    }

    private void FocusCurrentButton()
    {
        if (activeMenuController == 0)
            return;

        if (menuButtons == null ||
            menuButtons.Length == 0)
        {
            return;
        }

        if (currentButtonIndex < 0 ||
            currentButtonIndex >=
            menuButtons.Length)
        {
            currentButtonIndex = 0;
        }

        Button currentButton =
            menuButtons[currentButtonIndex];

        if (CanSelectButton(currentButton))
        {
            FocusButton(currentButton);
        }
        else
        {
            FocusButton(startButton);
        }
    }

    private void ClearControllerFocus()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current
            .SetSelectedGameObject(null);
    }

    public void FocusStartButton()
    {
        FocusButton(startButton);
    }

    public void FocusSettingButton()
    {
        FocusButton(settingButton);
    }

    public void FocusExitButton()
    {
        FocusButton(exitButton);
    }

    private void PlayMoveSound()
    {
        AudioManager audio =
            AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayUI(
                audio.movechooseItemClip
            );
        }
    }

    // =========================================================
    // START GAME
    // =========================================================

    public void OnStartClicked()
    {
        if (isLoading)
            return;

        // Reset Pause ngay khi nhấn Start.
        if (PauseGameManager.Instance != null)
        {
            PauseGameManager.Instance.ForceResume();
        }
        else
        {
            Time.timeScale = 1f;
        }

        isLoading = true;

        if (startButton != null)
        {
            startButton.interactable = false;
        }

        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayUI(audio.clickButton);
        }

        StartCoroutine(StartLoadScene());
    }

    private IEnumerator StartLoadScene()
    {
        AudioManager audio =
            AudioManager.Instance;

        if (audio != null)
        {
            audio.PauseAudio();
        }

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.HideGameCursor();
        }

        SettingManager setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.ResetSetting();
        }

        LoadingManager loading =
            LoadingManager.Instance;

        if (loading != null)
        {
            yield return StartCoroutine(
                loading.ShowLoading()
            );
        }

        SceneManager.LoadScene(
            gameSceneName
        );
    }

    // =========================================================
    // SETTINGS BUTTONS
    // =========================================================

    public void OnSettingsClicked()
    {
        if (isLoading)
            return;

        AudioManager audio =
            AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayUI(
                audio.clickButton
            );
        }

        SettingManager setting =
            SettingManager.Instance;

        if (setting != null)
        {
            setting.ToggleSetting();
        }
    }

    public void OnCloseSettingsClicked()
    {
        SettingManager setting =
            SettingManager.Instance;

        if (setting != null)
        {
            setting.ResetSetting();
        }

        // Tránh analog đang giữ làm menu nhảy ngay sau khi đóng.
        canMoveVertical = false;

        if (activeMenuController != 0)
        {
            StartCoroutine(
                FocusButtonDelay(settingButton)
            );
        }
    }

    // =========================================================
    // EXIT GAME
    // =========================================================

    public void OnExitClicked()
    {
        if (isLoading)
            return;

        AudioManager audio =
            AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayUI(
                audio.clickButton
            );
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}