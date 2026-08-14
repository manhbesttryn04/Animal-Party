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

    [Header("Menu Intro Lock")]
    [Tooltip(
        "Nếu có Menu Intro Animator thì biến này chỉ dùng làm thời gian dự phòng. " +
        "Nếu không gắn Animator, Main Menu sẽ khóa input đúng thời gian này."
    )]
    [SerializeField] private float menuInputDelay = 1f;

    [Header("Menu Intro Animation Lock")]
    [Tooltip(
        "Animator đang chạy animation xuất hiện của cụm Start / Setting / Exit. " +
        "Nếu gắn Animator, nút chỉ được bấm sau khi animation hiện tại chạy xong."
    )]
    [SerializeField] private Animator menuIntroAnimator;

    [Tooltip("Layer Animator chứa animation intro của menu.")]
    [Min(0)]
    [SerializeField] private int menuIntroAnimatorLayer = 0;

    [Header("Setting Button Selected Visual")]
    [Tooltip(
        "Khi click nút Setting bằng chuột, giữ trạng thái Selected trong thời gian này. " +
        "Sau đó bắt buộc bỏ Selected để trở về Highlighted hoặc Normal."
    )]
    [Min(0f)]
    [SerializeField] private float settingButtonSelectedTime = 1f;

    [Tooltip(
        "Player phải ngừng click ít nhất thời gian này thì lần click sau mới được phép " +
        "hiện Selected lại. Spam liên tục sẽ không làm Selected bị dính."
    )]
    [Min(0f)]
    [SerializeField] private float settingButtonVisualRearmDelay = 1f;

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
    private bool canUseMainMenu;

    // Lưu trạng thái interactable ban đầu để sau intro trả lại đúng như cũ.
    private bool startButtonInteractableOnOpen;
    private bool settingButtonInteractableOnOpen;
    private bool exitButtonInteractableOnOpen;

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

    // Visual Selected của nút Setting bằng chuột.
    private Coroutine settingButtonSelectedCoroutine;
    private float lastSettingButtonPressTime = -999f;

    // Sau khi Selected 1 giây kết thúc, nếu player vẫn spam click
    // và Unity tự select lại Button thì LateUpdate sẽ xóa ngay.
    private bool forceClearSettingButtonSelection;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupButtons();

        // Khóa Start / Settings / Exit ngay từ frame đầu.
        // Chỉ mở khóa khi animation intro thật sự chạy xong.
        LockMenuButtonsForIntro();

        SetupCursor();
        SetupAudio();
        SetupSetting();
        SetupVolume();

        InitializeControllerState();

        var ui = UIManager.Instance;
        if (ui != null)
        {
            ui.isShowKeyBoard = false;
        }

        StartCoroutine(UnlockMenuAfterIntro());
    }

    private void Update()
    {
        if (isLoading)
            return;

        UpdateControllerState();

        // Trong thời gian animation mở menu:
        // không cho tay cầm di chuyển hoặc Submit.
        if (!canUseMainMenu)
            return;

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

    private void LateUpdate()
    {
        // Logic này chỉ dành cho chuột.
        // Khi có tay cầm, Selected là focus điều khiển menu nên không được clear.
        if (activeMenuController != 0)
            return;

        if (!forceClearSettingButtonSelection)
            return;

        // Nếu player đã ngừng spam đủ lâu thì cho phép
        // lần click sau hiển thị Selected lại.
        if (Time.unscaledTime - lastSettingButtonPressTime >=
            Mathf.Max(0f, settingButtonVisualRearmDelay))
        {
            forceClearSettingButtonSelection = false;
            return;
        }

        if (settingButton == null ||
            EventSystem.current == null)
        {
            return;
        }

        // Nếu Unity tự Select lại nút do player vẫn spam click,
        // xóa ngay trong LateUpdate.
        if (EventSystem.current.currentSelectedGameObject ==
            settingButton.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

    private void LockMenuButtonsForIntro()
    {
        canUseMainMenu = false;

        if (startButton != null)
        {
            startButtonInteractableOnOpen =
                startButton.interactable;

            startButton.interactable = false;
        }

        if (settingButton != null)
        {
            settingButtonInteractableOnOpen =
                settingButton.interactable;

            settingButton.interactable = false;
        }

        if (exitButton != null)
        {
            exitButtonInteractableOnOpen =
                exitButton.interactable;

            exitButton.interactable = false;
        }

        // Không giữ focus cũ trong lúc 3 nút đang animation.
        ClearControllerFocus();
    }

    private IEnumerator UnlockMenuAfterIntro()
    {
        // =====================================================
        // LOGIC 1:
        // Nếu có Animator -> chờ ANIMATION THẬT SỰ chạy xong.
        // Nếu không có Animator -> dùng menuInputDelay dự phòng.
        // =====================================================

        if (menuIntroAnimator != null &&
            menuIntroAnimator.gameObject.activeInHierarchy &&
            menuIntroAnimator.enabled)
        {
            yield return StartCoroutine(
                WaitForMenuIntroAnimation()
            );
        }
        else if (menuInputDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                menuInputDelay
            );
        }

        if (isLoading)
            yield break;

        if (startButton != null)
        {
            startButton.interactable =
                startButtonInteractableOnOpen;
        }

        if (settingButton != null)
        {
            settingButton.interactable =
                settingButtonInteractableOnOpen;
        }

        if (exitButton != null)
        {
            exitButton.interactable =
                exitButtonInteractableOnOpen;
        }

        // Chỉ đến đây mới cho phép Main Menu nhận input.
        canUseMainMenu = true;

        // Nếu analog đang bị giữ từ lúc animation chạy,
        // bắt buộc thả về giữa trước khi được di chuyển menu.
        canMoveVertical = false;

        // Chỉ focus Start sau khi animation đã chạy xong.
        if (focusStartButtonOnOpen &&
            ControllerManager.Instance != null &&
            ControllerManager.Instance.HasAnyController())
        {
            StartCoroutine(
                FocusButtonDelay(startButton)
            );
        }
    }

    private IEnumerator WaitForMenuIntroAnimation()
    {
        // Cho Animator ít nhất 1 frame để vào state intro.
        yield return null;

        if (menuIntroAnimator == null)
            yield break;

        int layer = menuIntroAnimatorLayer;

        if (layer < 0 ||
            layer >= menuIntroAnimator.layerCount)
        {
            layer = 0;
        }

        AnimatorStateInfo firstState =
            menuIntroAnimator.GetCurrentAnimatorStateInfo(layer);

        int introStateHash = firstState.fullPathHash;

        while (menuIntroAnimator != null &&
               menuIntroAnimator.enabled &&
               menuIntroAnimator.gameObject.activeInHierarchy)
        {
            AnimatorStateInfo currentState =
                menuIntroAnimator.GetCurrentAnimatorStateInfo(layer);

            bool isTransitioning =
                menuIntroAnimator.IsInTransition(layer);

            // Trường hợp animation intro ở nguyên state:
            // normalizedTime >= 1 nghĩa là đã chạy hết 100%.
            if (currentState.fullPathHash == introStateHash &&
                currentState.normalizedTime >= 1f &&
                !isTransitioning)
            {
                break;
            }

            // Trường hợp intro chạy xong rồi Animator tự chuyển sang Idle:
            // Khi đã chuyển hẳn sang state khác thì intro cũng đã kết thúc.
            if (currentState.fullPathHash != introStateHash &&
                !isTransitioning)
            {
                break;
            }

            yield return null;
        }
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
            volume.ResetBloom();
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

        // Không focus trong lúc intro chưa chạy xong.
        if (!canUseMainMenu)
            return;

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

   

            return;
        }

        // Nếu Console 1 bị rút nhưng Console 2 vẫn còn,
        // Console 2 được phép điều khiển menu.
        if (controller.IsConsole2Connected())
        {
            activeMenuController = 2;

            

            return;
        }

        activeMenuController = 0;

      
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
    // SETTING BUTTON SELECTED VISUAL
    // =========================================================

    private void RegisterSettingButtonPressVisual()
    {
        // Chỉ áp dụng cho chuột.
        // Khi dùng tay cầm, Selected là focus nên không đụng vào.
        if (activeMenuController != 0)
            return;

        if (settingButton == null ||
            EventSystem.current == null ||
            !settingButton.gameObject.activeInHierarchy)
        {
            return;
        }

        float now = Time.unscaledTime;

        float rearmDelay = Mathf.Max(
            0f,
            settingButtonVisualRearmDelay
        );

        bool enoughTimeSinceLastPress =
            now - lastSettingButtonPressTime >= rearmDelay;

        // Luôn ghi nhận lần click mới nhất.
        // Spam liên tục sẽ liên tục đẩy mốc này lên.
        lastSettingButtonPressTime = now;

        // Nếu Selected 1 giây đang chạy thì không restart timer.
        if (settingButtonSelectedCoroutine != null)
        {
            return;
        }

        // Nếu chưa ngừng spam đủ lâu:
        // không cho Selected xuất hiện lại.
        if (!enoughTimeSinceLastPress)
        {
            forceClearSettingButtonSelection = true;

            if (EventSystem.current.currentSelectedGameObject ==
                settingButton.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            return;
        }

        forceClearSettingButtonSelection = false;

        settingButtonSelectedCoroutine =
            StartCoroutine(
                SettingButtonSelectedVisualRoutine()
            );
    }

    private IEnumerator SettingButtonSelectedVisualRoutine()
    {
        if (settingButton == null ||
            EventSystem.current == null)
        {
            settingButtonSelectedCoroutine = null;
            yield break;
        }

        // Ép Setting Button sang trạng thái Selected.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            settingButton.gameObject
        );

        // Dùng realtime vì lúc Setting mở game có thể Pause.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, settingButtonSelectedTime)
        );

        // Sau đúng thời gian trên bắt buộc bỏ Selected.
        // Unity tự quyết định trạng thái tiếp theo:
        // - Chuột còn hover -> Highlighted
        // - Chuột đã rời -> Normal
        if (EventSystem.current != null &&
            settingButton != null &&
            EventSystem.current.currentSelectedGameObject ==
                settingButton.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        settingButtonSelectedCoroutine = null;

        // Nếu player vẫn spam thì LateUpdate tiếp tục xóa Selected
        // cho tới khi player ngừng click đủ rearm delay.
        forceClearSettingButtonSelection = true;
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

        if (!canUseMainMenu)
            yield break;

        FocusButton(targetButton);
    }

    private void FocusButton(
        Button targetButton)
    {
        if (EventSystem.current == null)
        {
           

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

        if (!canUseMainMenu)
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
        // Animation intro chưa xong -> tuyệt đối không nhận.
        if (!canUseMainMenu || isLoading)
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
        // Animation intro chưa xong -> tuyệt đối không nhận.
        if (!canUseMainMenu || isLoading)
            return;

        // LOGIC 2:
        // Selected chỉ giữ 1 giây.
        // Spam liên tục không làm Selected bị dính.
        RegisterSettingButtonPressVisual();

        SettingManager setting =
            SettingManager.Instance;

        if (setting == null)
            return;

        // Không phát click ở MainMenuController.
        // SettingManager tự chống spam Open/Close bằng cooldown
        // và chỉ phát tiếng khi lệnh thực sự hợp lệ.
        setting.ToggleSetting();
    }

    public void OnCloseSettingsClicked()
    {
        if (!canUseMainMenu || isLoading)
            return;

        // Khi đóng cũng áp dụng visual Selected 1 giây
        // cho nút Setting của Main Menu.
        RegisterSettingButtonPressVisual();

        SettingManager setting =
            SettingManager.Instance;

        if (setting == null)
            return;

        // Đóng bình thường phải đi qua ToggleSetting để:
        // - dùng cooldown 1 giây
        // - không spam âm thanh
        // - không thể vừa đóng xong đã mở lại ngay
        setting.ToggleSetting();

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
        // Animation intro chưa xong -> tuyệt đối không nhận.
        if (!canUseMainMenu || isLoading)
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