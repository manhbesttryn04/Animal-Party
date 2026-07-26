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
    [SerializeField]
    private string verticalP1AxisName =
        "VerticalP1";

    [SerializeField]
    private string verticalP2AxisName =
        "VerticalP2";

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

    // Trạng thái tay cầm ở lần kiểm tra trước
    private bool previousConsole1Connected;
    private bool previousConsole2Connected;

    // Tay cầm hiện đang điều khiển menu
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
    }

    private void Update()
    {
        if (isLoading)
            return;

        UpdateControllerState();

        if (activeMenuController == 0)
            return;

        // Khi bảng Setting đang mở, Main Menu phía sau bị khóa.
        // Button 1 sẽ đóng Setting và trả focus về nút Setting.
        if (IsSettingOpen())
        {
            HandleCloseSettingInput();
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
        CursorManager cursor =
            CursorManager.Instance;

        if (cursor == null)
            return;

        // CursorManager tự kiểm tra trạng thái controller.
        cursor.UpdateCursorByControllerState();
    }

    private void SetupAudio()
    {
        AudioManager audio =
            AudioManager.Instance;

        if (audio == null)
            return;

        audio.PlayMusic(
            audio.musicMainMenuClip
        );

        audio.PlayEnvironment(
            audio.theNightClip
        );

        audio.SetupMainGameAudio();
    }

    private void SetupSetting()
    {
        SettingManager setting =
            SettingManager.Instance;

        if (setting != null)
        {
            setting.isOpenExitButton = false;
        }
    }

    private void SetupVolume()
    {
        VolumeManager volume =
            VolumeManager.Instance;

        if (volume != null)
        {
            volume.ResetVignette();
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
            activeMenuController = 0;
            return;
        }

        previousConsole1Connected =
            controller.IsConsole1Connected();

        previousConsole2Connected =
            controller.IsConsole2Connected();

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

        bool stateChanged =
            console1Connected !=
            previousConsole1Connected ||
            console2Connected !=
            previousConsole2Connected;

        if (!stateChanged)
            return;

        bool hadAnyController =
            previousConsole1Connected ||
            previousConsole2Connected;

        bool hasAnyController =
            console1Connected ||
            console2Connected;

        previousConsole1Connected =
            console1Connected;

        previousConsole2Connected =
            console2Connected;

        SelectActiveMenuController();

        canMoveVertical = true;

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.UpdateCursorByControllerState();
        }

        // Không còn tay cầm nào
        if (!hasAnyController)
        {
            ClearControllerFocus();
            return;
        }

        // Chỉ cần còn ít nhất 1 tay cầm
        // thì luôn focus nút Start
        StartCoroutine(
            FocusButtonDelay(startButton)
        );

        // Nếu không còn tay cầm:
        // bỏ focus để chuột tự điều khiển.
        if (!hasAnyController)
        {
            ClearControllerFocus();
        }
        else
        {
            FocusCurrentButton();
        }
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

        // Ưu tiên Console 1.
        if (controller.IsConsole1Connected())
        {
            activeMenuController = 1;

            Debug.Log(
                "Main Menu sử dụng VerticalP1."
            );

            return;
        }

        // Console 1 bị rút nhưng Console 2 vẫn còn.
        if (controller.IsConsole2Connected())
        {
            activeMenuController = 2;

            Debug.Log(
                "Main Menu sử dụng VerticalP2."
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
        if (activeMenuController == 0)
            return;

        activeMenuController = 0;

        previousConsole1Connected = false;
        previousConsole2Connected = false;

        canMoveVertical = true;

        ClearControllerFocus();

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.UpdateCursorByControllerState();
        }
    }

    private bool HasAnyControllerConnected()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        return controller != null &&
               controller.HasAnyController();
    }

    // =========================================================
    // GET ACTIVE CONTROLLER INPUT
    // =========================================================

    private float GetActiveVerticalInput()
    {
        if (activeMenuController == 1)
        {
            return Input.GetAxisRaw(
                verticalP1AxisName
            );
        }

        if (activeMenuController == 2)
        {
            return Input.GetAxisRaw(
                verticalP2AxisName
            );
        }

        return 0f;
    }

    private bool GetActiveSubmitDown()
    {
        if (activeMenuController == 1)
        {
            return Input.GetKeyDown(
                KeyCode.Joystick1Button0
            );
        }

        if (activeMenuController == 2)
        {
            return Input.GetKeyDown(
                KeyCode.Joystick2Button0
            );
        }

        return false;
    }

    private bool GetActiveCancelDown()
    {
        if (activeMenuController == 1)
        {
            return Input.GetKeyDown(
                KeyCode.Joystick1Button1
            );
        }

        if (activeMenuController == 2)
        {
            return Input.GetKeyDown(
                KeyCode.Joystick2Button1
            );
        }

        return false;
    }

    private bool IsSettingOpen()
    {
        SettingManager setting =
            SettingManager.Instance;

        if (setting == null)
            return false;

        return setting.isSettingOpen ||
               setting.isEscSettingOpen;
    }

    private void HandleCloseSettingInput()
    {
        if (!GetActiveCancelDown())
            return;

        SettingManager setting =
            SettingManager.Instance;

        if (setting == null)
            return;

        setting.ResetSetting();

        // Phải thả cần Vertical về giữa trước khi
        // tiếp tục di chuyển trong Main Menu.
        canMoveVertical = false;

        StartCoroutine(
            FocusButtonDelay(settingButton)
        );
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

        int startIndex =
            currentButtonIndex;

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

        } while (
            currentButtonIndex != startIndex
        );
    }

    private void MoveToNextButton()
    {
        if (menuButtons == null ||
            menuButtons.Length == 0)
        {
            return;
        }

        int startIndex =
            currentButtonIndex;

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

        } while (
            currentButtonIndex != startIndex
        );
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
        bool submitPressed =
            GetActiveSubmitDown();

        if (!submitPressed)
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
            currentButtonIndex >= menuButtons.Length)
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

        isLoading = true;

        if (startButton != null)
        {
            startButton.interactable = false;
        }

        AudioManager audio =
            AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayUI(
                audio.clickButton
            );
        }

        StartCoroutine(
            StartLoadScene()
        );
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

        SettingManager setting =
            SettingManager.Instance;

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
    // SETTINGS
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