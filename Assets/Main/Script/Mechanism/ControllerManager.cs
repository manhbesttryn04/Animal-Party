using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public enum ControllerChangeType
    {
        None,
        ConnectedNewController,
        Disconnected,
        ReconnectedSameController,
        ReplacedWithDifferentController
    }

    private struct ConnectedController
    {
        public string name;

        // Joystick index vật lý thật do Unity cấp.
        // Có thể là 1, 2, 3, 4...
        public int physicalJoystickIndex;

        public ConnectedController(
            string controllerName,
            int joystickIndex)
        {
            name = controllerName;
            physicalJoystickIndex = joystickIndex;
        }
    }

    public static ControllerManager Instance
    {
        get;
        private set;
    }

    // =========================================================
    // CONTROLLER STATE
    // =========================================================

    [Header("Controller Slots")]
    [SerializeField] private bool console1Connected;
    [SerializeField] private bool console2Connected;

    [Header("Physical Joystick Index")]
    [Tooltip(
        "Joystick vật lý thật mà Unity đang dùng cho Controller 1."
    )]
    [SerializeField] private int console1JoystickIndex;

    [Tooltip(
        "Joystick vật lý thật mà Unity đang dùng cho Controller 2."
    )]
    [SerializeField] private int console2JoystickIndex;

    [Header("Controller Names")]
    [SerializeField] private string console1Name = "";
    [SerializeField] private string console2Name = "";

    [Header("Last Change")]
    [SerializeField]
    private ControllerChangeType console1LastChange;

    [SerializeField]
    private ControllerChangeType console2LastChange;

    [Header("Update")]
    [Min(0.05f)]
    [SerializeField] private float checkInterval = 0.25f;

    private float checkTimer;

    private readonly List<ConnectedController>
        previousConnectedControllers =
            new List<ConnectedController>();

    private Coroutine notificationCoroutine;
    private bool allowControllerSounds;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        InitializeControllerState();

        float waitTimer = 0f;
        const float maxWaitTime = 2f;

        while ((UIManager.Instance == null ||
                AudioManager.Instance == null) &&
               waitTimer < maxWaitTime)
        {
            waitTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        allowControllerSounds = true;

        ShowInitialConnectedControllers();
    }

    private void Update()
    {
        checkTimer += Time.unscaledDeltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        RefreshControllerState();
        UpdateConsoleInstructionUI();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeControllerState()
    {
        List<ConnectedController> currentControllers =
            GetConnectedControllers();

        // Lúc mở game:
        // tay đầu tiên là Controller 1,
        // tay thứ hai là Controller 2.
        if (currentControllers.Count >= 1)
        {
            AssignConsole1(currentControllers[0]);
        }

        if (currentControllers.Count >= 2)
        {
            AssignConsole2(currentControllers[1]);
        }

        previousConnectedControllers.Clear();
        previousConnectedControllers.AddRange(
            currentControllers
        );

        UpdateConsoleInstructionUI();
        UpdateCursor();
        PrintControllerState();
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void RefreshControllerState()
    {
        List<ConnectedController> currentControllers =
            GetConnectedControllers();

        if (AreControllerListsEqual(
            previousConnectedControllers,
            currentControllers))
        {
            return;
        }

        UpdateControllerSlots(currentControllers);

        previousConnectedControllers.Clear();
        previousConnectedControllers.AddRange(
            currentControllers
        );

        UpdateConsoleInstructionUI();
        UpdateCursor();
        PrintControllerState();
    }

    // =========================================================
    // CONTROLLER SLOT LOGIC
    // =========================================================

    private void UpdateControllerSlots(
        List<ConnectedController> currentControllers)
    {
        console1LastChange =
            ControllerChangeType.None;

        console2LastChange =
            ControllerChangeType.None;

        bool console1WasConnected =
            console1Connected;

        bool console2WasConnected =
            console2Connected;

        string oldConsole1Name =
            console1Name;

        string oldConsole2Name =
            console2Name;

        int oldConsole1JoystickIndex =
            console1JoystickIndex;

        int oldConsole2JoystickIndex =
            console2JoystickIndex;

        // Danh sách tay cầm chưa được gán.
        List<ConnectedController> unassigned =
            new List<ConnectedController>(
                currentControllers
            );

        // =====================================================
        // GIỮ CONTROLLER 1 NẾU NÓ VẪN CÒN
        // =====================================================

        if (console1WasConnected)
        {
            int matchIndex =
                FindExistingController(
                    unassigned,
                    oldConsole1Name,
                    oldConsole1JoystickIndex
                );

            if (matchIndex >= 0)
            {
                ConnectedController controller =
                    unassigned[matchIndex];

                AssignConsole1(controller);

                unassigned.RemoveAt(matchIndex);
            }
            else
            {
                DisconnectConsole1();
            }
        }

        // =====================================================
        // GIỮ CONTROLLER 2 NẾU NÓ VẪN CÒN
        // =====================================================

        if (console2WasConnected)
        {
            int matchIndex =
                FindExistingController(
                    unassigned,
                    oldConsole2Name,
                    oldConsole2JoystickIndex
                );

            if (matchIndex >= 0)
            {
                ConnectedController controller =
                    unassigned[matchIndex];

                AssignConsole2(controller);

                unassigned.RemoveAt(matchIndex);
            }
            else
            {
                DisconnectConsole2();
            }
        }

        // =====================================================
        // LẤP CONTROLLER 1 NẾU ĐANG TRỐNG
        // =====================================================

        if (!console1Connected &&
            unassigned.Count > 0)
        {
            ConnectedController controller =
                unassigned[0];

            unassigned.RemoveAt(0);

            AssignConsole1(controller);

            console1LastChange =
                ControllerChangeType
                    .ConnectedNewController;

            ShowConsole1Connected();
            PlayConnectSound();
        }

        // =====================================================
        // LẤP CONTROLLER 2 NẾU ĐANG TRỐNG
        // =====================================================

        if (!console2Connected &&
            unassigned.Count > 0)
        {
            ConnectedController controller =
                unassigned[0];

            unassigned.RemoveAt(0);

            AssignConsole2(controller);

            console2LastChange =
                ControllerChangeType
                    .ConnectedNewController;

            ShowConsole2Connected();
            PlayConnectSound();
        }

        /*
         * Nếu vẫn còn phần tử trong unassigned:
         * đó là tay cầm thứ ba trở lên.
         *
         * Game sẽ bỏ qua hoàn toàn.
         */
    }

    // =========================================================
    // ASSIGN
    // =========================================================

    private void AssignConsole1(
        ConnectedController controller)
    {
        console1Connected = true;
        console1Name = controller.name;
        console1JoystickIndex =
            controller.physicalJoystickIndex;
    }

    private void AssignConsole2(
        ConnectedController controller)
    {
        console2Connected = true;
        console2Name = controller.name;
        console2JoystickIndex =
            controller.physicalJoystickIndex;
    }

    // =========================================================
    // DISCONNECT
    // =========================================================

    private void DisconnectConsole1()
    {
        if (!console1Connected)
            return;

        console1Connected = false;
        console1JoystickIndex = 0;

        // Rút tay cầm thì xóa tên đã lưu.
        console1Name = "";

        console1LastChange =
            ControllerChangeType.Disconnected;

        ShowConsole1Disconnected();
        PlayDisconnectSound();
    }

    private void DisconnectConsole2()
    {
        if (!console2Connected)
            return;

        console2Connected = false;
        console2JoystickIndex = 0;

        // Rút tay cầm thì xóa tên đã lưu.
        console2Name = "";

        console2LastChange =
            ControllerChangeType.Disconnected;

        ShowConsole2Disconnected();
        PlayDisconnectSound();
    }

    // =========================================================
    // GET CONNECTED CONTROLLERS
    // =========================================================

    private List<ConnectedController>
        GetConnectedControllers()
    {
        string[] joystickNames =
            Input.GetJoystickNames();

        List<ConnectedController> controllers =
            new List<ConnectedController>();

        if (joystickNames == null)
            return controllers;

        for (int i = 0;
             i < joystickNames.Length;
             i++)
        {
            string controllerName =
                joystickNames[i];

            if (string.IsNullOrWhiteSpace(
                controllerName))
            {
                continue;
            }

            int physicalJoystickIndex = i + 1;

            controllers.Add(
                new ConnectedController(
                    controllerName.Trim(),
                    physicalJoystickIndex
                )
            );
        }

        return controllers;
    }

    // =========================================================
    // FIND EXISTING CONTROLLER
    // =========================================================

    private int FindExistingController(
        List<ConnectedController> controllers,
        string oldName,
        int oldJoystickIndex)
    {
        if (controllers == null ||
            controllers.Count == 0)
        {
            return -1;
        }

        /*
         * Ưu tiên joystick index vật lý cũ.
         *
         * Việc này giúp Controller 2 không bị tự chuyển
         * thành Controller 1 khi Controller 1 bị rút.
         */
        for (int i = 0;
             i < controllers.Count;
             i++)
        {
            if (controllers[i]
                    .physicalJoystickIndex ==
                oldJoystickIndex)
            {
                return i;
            }
        }

        /*
         * Nếu Unity đổi joystick index thì thử tìm bằng tên.
         *
         * Chỉ sử dụng tên nếu tên đó xuất hiện đúng một lần,
         * tránh hai tay cầm cùng tên bị nhận nhầm.
         */
        int foundIndex = -1;
        int matchCount = 0;

        for (int i = 0;
             i < controllers.Count;
             i++)
        {
            if (!ControllerNamesEqual(
                controllers[i].name,
                oldName))
            {
                continue;
            }

            foundIndex = i;
            matchCount++;
        }

        if (matchCount == 1)
            return foundIndex;

        return -1;
    }

    private bool ControllerNamesEqual(
        string firstName,
        string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(secondName))
        {
            return false;
        }

        return string.Equals(
            firstName,
            secondName,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private bool AreControllerListsEqual(
        List<ConnectedController> first,
        List<ConnectedController> second)
    {
        if (first == null ||
            second == null)
        {
            return false;
        }

        if (first.Count != second.Count)
            return false;

        for (int i = 0;
             i < first.Count;
             i++)
        {
            bool sameName =
                ControllerNamesEqual(
                    first[i].name,
                    second[i].name
                );

            bool sameJoystickIndex =
                first[i].physicalJoystickIndex ==
                second[i].physicalJoystickIndex;

            if (!sameName ||
                !sameJoystickIndex)
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // AXIS INPUT
    // =========================================================

    public float GetConsoleAxisRaw(
        int consoleNumber,
        string joystick1Axis,
        string joystick2Axis)
    {
        int physicalJoystickIndex =
            GetConsoleJoystickIndex(
                consoleNumber
            );

        if (physicalJoystickIndex == 1)
        {
            return Input.GetAxisRaw(
                joystick1Axis
            );
        }

        if (physicalJoystickIndex == 2)
        {
            return Input.GetAxisRaw(
                joystick2Axis
            );
        }

        /*
         * Legacy Input Manager hiện tại chỉ có axis
         * dành cho Joystick 1 và Joystick 2.
         *
         * Nếu Unity cấp index 3 trở lên thì phải tạo thêm
         * axis riêng hoặc chuyển sang Input System.
         */
        return 0f;
    }

    public float GetConsoleHorizontalRaw(
        int consoleNumber,
        string horizontalJoystick1,
        string horizontalJoystick2)
    {
        return GetConsoleAxisRaw(
            consoleNumber,
            horizontalJoystick1,
            horizontalJoystick2
        );
    }

    public float GetConsoleVerticalRaw(
        int consoleNumber,
        string verticalJoystick1,
        string verticalJoystick2)
    {
        return GetConsoleAxisRaw(
            consoleNumber,
            verticalJoystick1,
            verticalJoystick2
        );
    }

    // =========================================================
    // BUTTON INPUT
    // =========================================================

    public bool GetConsoleButtonDown(
        int consoleNumber,
        int buttonIndex)
    {
        KeyCode keyCode =
            GetConsoleButtonKeyCode(
                consoleNumber,
                buttonIndex
            );

        if (keyCode == KeyCode.None)
            return false;

        return Input.GetKeyDown(keyCode);
    }

    public bool GetConsoleButton(
        int consoleNumber,
        int buttonIndex)
    {
        KeyCode keyCode =
            GetConsoleButtonKeyCode(
                consoleNumber,
                buttonIndex
            );

        if (keyCode == KeyCode.None)
            return false;

        return Input.GetKey(keyCode);
    }

    public bool GetConsoleButtonUp(
        int consoleNumber,
        int buttonIndex)
    {
        KeyCode keyCode =
            GetConsoleButtonKeyCode(
                consoleNumber,
                buttonIndex
            );

        if (keyCode == KeyCode.None)
            return false;

        return Input.GetKeyUp(keyCode);
    }

    private KeyCode GetConsoleButtonKeyCode(
        int consoleNumber,
        int buttonIndex)
    {
        int physicalJoystickIndex =
            GetConsoleJoystickIndex(
                consoleNumber
            );

        return GetJoystickButtonKeyCode(
            physicalJoystickIndex,
            buttonIndex
        );
    }

    private KeyCode GetJoystickButtonKeyCode(
        int joystickIndex,
        int buttonIndex)
    {
        if (joystickIndex < 1 ||
            joystickIndex > 8 ||
            buttonIndex < 0 ||
            buttonIndex > 19)
        {
            return KeyCode.None;
        }

        string keyName =
            "Joystick" +
            joystickIndex +
            "Button" +
            buttonIndex;

        if (Enum.TryParse(
            keyName,
            out KeyCode keyCode))
        {
            return keyCode;
        }

        return KeyCode.None;
    }

    // =========================================================
    // UI
    // =========================================================

    public void RefreshInputInstructionUI()
    {
        UpdateConsoleInstructionUI();
    }

    private void UpdateConsoleInstructionUI()
    {
        UIManager ui = UIManager.Instance;

        if (ui == null)
            return;

        bool hasAnyController =
            console1Connected ||
            console2Connected;

        bool hasBothControllers =
            console1Connected &&
            console2Connected;

        if (ui.instructConsolePanel != null)
        {
            ui.instructConsolePanel.SetActive(
                hasAnyController
            );
        }

        if (ui.instructKeyBoardPanel != null &&
            ui.isShowKeyBoard)
        {
            ui.instructKeyBoardPanel.SetActive(
                !hasBothControllers
            );
        }
    }

    private void UpdateCursor()
    {
        if (CursorManager.Instance == null)
            return;

        CursorManager.Instance
            .UpdateCursorByControllerState();
    }

    // =========================================================
    // CONNECT UI
    // =========================================================

    private void ShowInitialConnectedControllers()
    {
        bool showedController = false;

        if (console1Connected)
        {
            ShowConsole1Connected();
            showedController = true;
        }

        if (console2Connected)
        {
            ShowConsole2Connected();
            showedController = true;
        }

        if (showedController)
        {
            PlayConnectSound();
        }
    }

    private void ShowConsole1Connected()
    {
        UIManager ui = UIManager.Instance;

        if (ui == null ||
            ui.consoleOpenImageP1 == null)
        {
            return;
        }

        StartCoroutine(
            ui.ShowConsoleConect(
                ui.consoleOpenImageP1
            )
        );
    }

    private void ShowConsole2Connected()
    {
        UIManager ui = UIManager.Instance;

        if (ui == null ||
            ui.consoleOpenImageP2 == null)
        {
            return;
        }

        StartCoroutine(
            ui.ShowConsoleConect(
                ui.consoleOpenImageP2
            )
        );
    }

    private void ShowConsole1Disconnected()
    {
        UIManager ui = UIManager.Instance;

        if (ui == null ||
            ui.consoleCloseImageP1 == null)
        {
            return;
        }

        StartCoroutine(
            ui.ShowConsoleFailConect(
                ui.consoleCloseImageP1
            )
        );
    }

    private void ShowConsole2Disconnected()
    {
        UIManager ui = UIManager.Instance;

        if (ui == null ||
            ui.consoleCloseImageP2 == null)
        {
            return;
        }

        StartCoroutine(
            ui.ShowConsoleFailConect(
                ui.consoleCloseImageP2
            )
        );
    }

    // =========================================================
    // SOUND
    // =========================================================

    private void PlayConnectSound()
    {
        if (!allowControllerSounds ||
            AudioManager.Instance == null ||
            AudioManager.Instance
                .consoleControllerConect == null)
        {
            return;
        }

        AudioManager.Instance.PlayUI(
            AudioManager.Instance
                .consoleControllerConect
        );
    }

    private void PlayDisconnectSound()
    {
        if (!allowControllerSounds ||
            AudioManager.Instance == null ||
            AudioManager.Instance
                .consoleControllerDisConect == null)
        {
            return;
        }

        AudioManager.Instance.PlayUI(
            AudioManager.Instance
                .consoleControllerDisConect
        );
    }

    // =========================================================
    // NOTIFICATION
    // =========================================================

    private void ShowControllerNotification()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(
                notificationCoroutine
            );
        }

        notificationCoroutine =
            StartCoroutine(
                NotificationRoutine()
            );
    }

    private IEnumerator NotificationRoutine()
    {
        yield return new WaitForSecondsRealtime(
            0.5f
        );

        notificationCoroutine = null;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void PrintControllerState()
    {
        Debug.Log(
            "Controller 1: " +
            (console1Connected
                ? "Connected | " +
                  console1Name +
                  " | Physical Joystick " +
                  console1JoystickIndex
                : "Disconnected")
        );

        Debug.Log(
            "Controller 2: " +
            (console2Connected
                ? "Connected | " +
                  console2Name +
                  " | Physical Joystick " +
                  console2JoystickIndex
                : "Disconnected")
        );
    }

    // =========================================================
    // PUBLIC STATE
    // =========================================================

    public bool IsConsole1Connected()
    {
        return console1Connected;
    }

    public bool IsConsole2Connected()
    {
        return console2Connected;
    }

    public bool IsConsoleConnected(
        int consoleNumber)
    {
        if (consoleNumber == 1)
            return console1Connected;

        if (consoleNumber == 2)
            return console2Connected;

        return false;
    }

    public bool HasAnyController()
    {
        return console1Connected ||
               console2Connected;
    }

    public int GetControllerCount()
    {
        int count = 0;

        if (console1Connected)
            count++;

        if (console2Connected)
            count++;

        return count;
    }

    public string GetConsole1Name()
    {
        return console1Name;
    }

    public string GetConsole2Name()
    {
        return console2Name;
    }

    public int GetConsole1JoystickIndex()
    {
        if (!console1Connected)
            return 0;

        return console1JoystickIndex;
    }

    public int GetConsole2JoystickIndex()
    {
        if (!console2Connected)
            return 0;

        return console2JoystickIndex;
    }

    public int GetConsoleJoystickIndex(
        int consoleNumber)
    {
        if (consoleNumber == 1)
        {
            return GetConsole1JoystickIndex();
        }

        if (consoleNumber == 2)
        {
            return GetConsole2JoystickIndex();
        }

        return 0;
    }

    public ControllerChangeType
        GetConsole1LastChange()
    {
        return console1LastChange;
    }

    public ControllerChangeType
        GetConsole2LastChange()
    {
        return console2LastChange;
    }

    public bool DidConsole1ControllerChange()
    {
        return console1LastChange ==
               ControllerChangeType
                   .ReplacedWithDifferentController;
    }

    public bool DidConsole2ControllerChange()
    {
        return console2LastChange ==
               ControllerChangeType
                   .ReplacedWithDifferentController;
    }
}