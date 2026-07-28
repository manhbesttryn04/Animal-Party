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
        public int joystickIndex;

        public ConnectedController(
            string controllerName,
            int index)
        {
            name = controllerName;
            joystickIndex = index;
        }
    }

    public static ControllerManager Instance
    {
        get;
        private set;
    }

    [Header("Current Controller State")]
    [SerializeField] private bool console1Connected;
    [SerializeField] private bool console2Connected;

    [Header("Current Joystick Index")]
    [Tooltip("Joystick slot thật mà Unity đang gán cho Console 1.")]
    [SerializeField] private int console1JoystickIndex;

    [Tooltip("Joystick slot thật mà Unity đang gán cho Console 2.")]
    [SerializeField] private int console2JoystickIndex;

    [Header("Saved Controller Names")]
    [SerializeField] private string console1Name = "";
    [SerializeField] private string console2Name = "";

    [Header("Last Controller Change")]
    [SerializeField]
    private ControllerChangeType console1LastChange;

    [SerializeField]
    private ControllerChangeType console2LastChange;

    [Header("Update")]
    [Min(0.05f)]
    [SerializeField] private float checkInterval = 0.25f;

    private float checkTimer;
    private int previousControllerCount;

    private readonly List<ConnectedController>
        previousConnectedControllers =
            new List<ConnectedController>();

    private Coroutine notificationCoroutine;

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

    private void Start()
    {
        InitializeControllerState();
    }

    private void Update()
    {
        checkTimer += Time.unscaledDeltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        RefreshControllerState();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeControllerState()
    {
        List<ConnectedController> currentControllers =
            GetConnectedControllers();

        UpdateControllerSlots(currentControllers);
        UpdateConsoleInstructionUI();

        previousConnectedControllers.Clear();

        previousConnectedControllers.AddRange(
            currentControllers
        );

        previousControllerCount =
            currentControllers.Count;

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

        int currentControllerCount =
            currentControllers.Count;

        bool hasNewControllerConnected =
            currentControllerCount >
            previousControllerCount;

        bool hasControllerDisconnected =
            currentControllerCount <
            previousControllerCount;

        UpdateControllerSlots(currentControllers);
        UpdateConsoleInstructionUI();

        previousConnectedControllers.Clear();

        previousConnectedControllers.AddRange(
            currentControllers

        );

        previousControllerCount =
            currentControllerCount;

        UpdateCursor();

        if (hasNewControllerConnected)
        {
            ShowControllerNotification();
        }

        if (hasControllerDisconnected)
        {
            ShowControllerDisconnect();
        }

        PrintControllerState();
    }

    // =========================================================
    // CONTROLLER SLOT LOGIC
    // =========================================================

    private void UpdateControllerSlots(
        List<ConnectedController> currentControllers)
    {
        bool wasConsole1Connected =
            console1Connected;

        bool wasConsole2Connected =
            console2Connected;

        string oldConsole1Name =
            console1Name;

        string oldConsole2Name =
            console2Name;

        int oldConsole1JoystickIndex =
            console1JoystickIndex;

        int oldConsole2JoystickIndex =
            console2JoystickIndex;

        console1LastChange =
            ControllerChangeType.None;

        console2LastChange =
            ControllerChangeType.None;

        console1Connected = false;
        console2Connected = false;

        console1JoystickIndex = 0;
        console2JoystickIndex = 0;

        // Tạo danh sách tay cầm chưa được gán.
        List<ConnectedController>
            unassignedControllers =
                new List<ConnectedController>(
                    currentControllers
                );

        // Không có tay cầm nào.
        if (unassignedControllers.Count == 0)
        {
            if (wasConsole1Connected)
            {
                console1LastChange =
                    ControllerChangeType.Disconnected;
                if (UIManager.Instance != null &&
                    UIManager.Instance.consoleCloseImageP1 != null)
                {
                    StartCoroutine(
                        UIManager.Instance.ShowConsoleFailConect(
                            UIManager.Instance.consoleCloseImageP1
                        )
                    );
                }
            }

            if (wasConsole2Connected)
            {
                console2LastChange =
                    ControllerChangeType.Disconnected;
                if (UIManager.Instance != null &&
                    UIManager.Instance.consoleCloseImageP2 != null)
                {
                    StartCoroutine(
                        UIManager.Instance.ShowConsoleFailConect(
                            UIManager.Instance.consoleCloseImageP2
                        )
                    );
                }
            }

            Debug.Log(
                "Đã rút hết tay cầm. " +
                "Tên tay cầm cũ vẫn được giữ lại."
            );

            return;
        }

        // =====================================================
        // TÌM LẠI CONSOLE 1
        // =====================================================

        int console1MatchIndex =
            FindBestControllerMatch(
                unassignedControllers,
                oldConsole1Name,
                oldConsole1JoystickIndex
            );

        if (console1MatchIndex >= 0)
        {
            ConnectedController matchedController =
                unassignedControllers[console1MatchIndex];

            AssignConsole1(matchedController);

            unassignedControllers.RemoveAt(console1MatchIndex);

            if (!wasConsole1Connected)
            {
                console1LastChange =
                    ControllerChangeType.ReconnectedSameController;

                if (UIManager.Instance != null)
                {
                    StartCoroutine(
                        UIManager.Instance.ShowConsoleConect(
                            UIManager.Instance.consoleOpenImageP1
                        )
                    );
                }
            }
        }

        // =====================================================
        // TÌM LẠI CONSOLE 2
        // =====================================================

        int console2MatchIndex =
            FindBestControllerMatch(
                unassignedControllers,
                oldConsole2Name,
                oldConsole2JoystickIndex
            );

        if (console2MatchIndex >= 0)
        {
            ConnectedController matchedController =
                unassignedControllers[console2MatchIndex];

            AssignConsole2(matchedController);

            unassignedControllers.RemoveAt(console2MatchIndex);

            if (!wasConsole2Connected)
            {
                console2LastChange =
                    ControllerChangeType.ReconnectedSameController;

                if (UIManager.Instance != null)
                {
                    StartCoroutine(
                        UIManager.Instance.ShowConsoleConect(
                            UIManager.Instance.consoleOpenImageP2
                        )
                    );
                }
            }
        }

        // =====================================================
        // CONSOLE 1 ĐANG TRỐNG
        // =====================================================

        if (!console1Connected &&
            unassignedControllers.Count > 0)
        {
            ConnectedController newController =
                unassignedControllers[0];

            unassignedControllers.RemoveAt(0);

            bool hadSavedController =
                !string.IsNullOrWhiteSpace(
                    oldConsole1Name
                );

            bool isDifferentController =
                hadSavedController &&
                !ControllerNamesEqual(
                    oldConsole1Name,
                    newController.name
                );

            AssignConsole1(newController);

            if (UIManager.Instance != null &&
                UIManager.Instance.consoleOpenImageP1 != null)
            {
                StartCoroutine(
                    UIManager.Instance.ShowConsoleConect(
                        UIManager.Instance.consoleOpenImageP1
                    )
                );
            }
            console1LastChange =
                isDifferentController
                    ? ControllerChangeType
                        .ReplacedWithDifferentController
                    : ControllerChangeType
                        .ConnectedNewController;
        }

        // =====================================================
        // CONSOLE 2 ĐANG TRỐNG
        // =====================================================

        if (!console2Connected &&
            unassignedControllers.Count > 0)
        {
            ConnectedController newController =
                unassignedControllers[0];

            unassignedControllers.RemoveAt(0);

            bool hadSavedController =
                !string.IsNullOrWhiteSpace(
                    oldConsole2Name
                );

            bool isDifferentController =
                hadSavedController &&
                !ControllerNamesEqual(
                    oldConsole2Name,
                    newController.name
                );

            AssignConsole2(newController);
            if (UIManager.Instance != null &&
                UIManager.Instance.consoleOpenImageP2 != null)
            {
                StartCoroutine(
                    UIManager.Instance.ShowConsoleConect(
                        UIManager.Instance.consoleOpenImageP2
                    )
                );
            }

            console2LastChange =
                isDifferentController
                    ? ControllerChangeType
                        .ReplacedWithDifferentController
                    : ControllerChangeType
                        .ConnectedNewController;
        }

        // Nếu trước đó kết nối nhưng giờ không tìm thấy.
        if (wasConsole1Connected &&
            !console1Connected)
        {
            console1LastChange =
                ControllerChangeType.Disconnected;
        }

        if (wasConsole2Connected &&
            !console2Connected)
        {
            console2LastChange =
                ControllerChangeType.Disconnected;
        }
    }

    private void AssignConsole1(
        ConnectedController controller)
    {
        console1Connected = true;
        console1Name = controller.name;
        console1JoystickIndex =
            controller.joystickIndex;
    }

    private void AssignConsole2(
        ConnectedController controller)
    {
        console2Connected = true;
        console2Name = controller.name;
        console2JoystickIndex =
            controller.joystickIndex;
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

            // i bắt đầu từ 0 nhưng Joystick bắt đầu từ 1.
            int joystickIndex = i + 1;

            controllers.Add(
                new ConnectedController(
                    controllerName.Trim(),
                    joystickIndex
                )
            );
        }

        return controllers;
    }

    // =========================================================
    // MATCH CONTROLLER
    // =========================================================

    private int FindBestControllerMatch(
        List<ConnectedController> controllers,
        string savedName,
        int savedJoystickIndex)
    {
        if (string.IsNullOrWhiteSpace(savedName))
            return -1;

        // Ưu tiên cùng tên và cùng joystick slot cũ.
        for (int i = 0;
             i < controllers.Count;
             i++)
        {
            bool sameName =
                ControllerNamesEqual(
                    controllers[i].name,
                    savedName
                );

            bool sameIndex =
                controllers[i].joystickIndex ==
                savedJoystickIndex;

            if (sameName && sameIndex)
                return i;
        }

        // Nếu Unity đổi joystick slot,
        // tìm lại bằng tên tay cầm.
        for (int i = 0;
             i < controllers.Count;
             i++)
        {
            if (ControllerNamesEqual(
                controllers[i].name,
                savedName))
            {
                return i;
            }
        }

        return -1;
    }

    private bool ControllerNamesEqual(
        string firstName,
        string secondName)
    {
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

            bool sameIndex =
                first[i].joystickIndex ==
                second[i].joystickIndex;

            if (!sameName || !sameIndex)
                return false;
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
        int joystickIndex =
            GetConsoleJoystickIndex(
                consoleNumber
            );

        if (joystickIndex == 1)
        {
            return Input.GetAxisRaw(
                joystick1Axis
            );
        }

        if (joystickIndex == 2)
        {
            return Input.GetAxisRaw(
                joystick2Axis
            );
        }

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
        int joystickIndex =
            GetConsoleJoystickIndex(
                consoleNumber
            );

        return GetJoystickButtonKeyCode(
            joystickIndex,
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
    // UI / CURSOR
    // =========================================================

    private void UpdateConsoleInstructionUI()
    {
        if (UIManager.Instance == null ||
            UIManager.Instance.instructConsolePanel == null)
        {
            return;
        }

        bool hasController =
            console1Connected ||
            console2Connected;

        UIManager.Instance.instructConsolePanel.SetActive(
            hasController
        );
    }

    private void UpdateCursor()
    {
        if (CursorManager.Instance == null)
            return;

        CursorManager.Instance
            .UpdateCursorByControllerState();
    }

    private void ShowControllerNotification()
    {
        if (UIManager.Instance == null)
            return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(
                notificationCoroutine
            );
        }

        notificationCoroutine =
            StartCoroutine(
                ShowNotificationRoutine()
            );
    }

    private void ShowControllerDisconnect()
    {
        if (UIManager.Instance == null)
            return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(
                notificationCoroutine
            );
        }

        notificationCoroutine =
            StartCoroutine(
                ShowDisconnectRoutine()
            );
    }

    private IEnumerator ShowNotificationRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        notificationCoroutine = null;
    }

    private IEnumerator ShowDisconnectRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        notificationCoroutine = null;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void PrintControllerState()
    {
        Debug.Log(
            "Console 1: " +
            (console1Connected
                ? "Connected | " +
                  console1Name +
                  " | Joystick " +
                  console1JoystickIndex
                : "Disconnected | Saved: " +
                  console1Name)
        );

        Debug.Log(
            "Console 2: " +
            (console2Connected
                ? "Connected | " +
                  console2Name +
                  " | Joystick " +
                  console2JoystickIndex
                : "Disconnected | Saved: " +
                  console2Name)
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
        return console1Connected
            ? console1JoystickIndex
            : 0;
    }

    public int GetConsole2JoystickIndex()
    {
        return console2Connected
            ? console2JoystickIndex
            : 0;
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