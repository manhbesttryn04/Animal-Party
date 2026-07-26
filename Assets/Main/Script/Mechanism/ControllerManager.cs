using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance { get; private set; }

    [Header("Current Controller State")]
    [SerializeField] private bool console1Connected;
    [SerializeField] private bool console2Connected;

    [Header("Saved Controller Names")]
    [SerializeField] private string console1Name = "";
    [SerializeField] private string console2Name = "";

    [Header("Update")]
    [SerializeField] private float checkInterval = 0.25f;

    private float checkTimer;
    private int previousControllerCount;

    private readonly List<string> previousConnectedNames =
        new List<string>();

    private Coroutine notificationCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
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
        List<string> currentNames =
            GetConnectedControllerNames();

        UpdateControllerSlots(currentNames);

        previousConnectedNames.Clear();
        previousConnectedNames.AddRange(currentNames);

        previousControllerCount = currentNames.Count;

        UpdateCursor();

        PrintControllerState();

        // Không gọi UI tại đây.
        // Vì đây chỉ là trạng thái lúc game vừa mở,
        // không phải sự kiện vừa cắm tay cầm.
    }

    // =========================================================
    // UPDATE CONTROLLER STATE
    // =========================================================

    private void RefreshControllerState()
    {
        List<string> currentNames =
            GetConnectedControllerNames();

        if (AreListsEqual(
            previousConnectedNames,
            currentNames))
        {
            return;
        }

        int currentControllerCount =
            currentNames.Count;

        // Chỉ true khi số tay cầm tăng:
        // 0 -> 1 hoặc 1 -> 2
        bool hasNewControllerConnected =
            currentControllerCount >
            previousControllerCount;

        UpdateControllerSlots(currentNames);

        previousConnectedNames.Clear();
        previousConnectedNames.AddRange(currentNames);

        previousControllerCount =
            currentControllerCount;

        UpdateCursor();

        // Chỉ hiện UI khi vừa cắm thêm tay cầm.
        // Không hiện khi rút.
        if (hasNewControllerConnected)
        {
            ShowControllerNotification();
        }

        PrintControllerState();
    }

    // =========================================================
    // CONTROLLER SLOT LOGIC
    // =========================================================

    private void UpdateControllerSlots(
        List<string> currentNames)
    {
        // Rút hết tay cầm:
        // reset toàn bộ tên đã lưu.
        if (currentNames.Count == 0)
        {
            console1Connected = false;
            console2Connected = false;

            console1Name = "";
            console2Name = "";

            Debug.Log(
                "Đã rút hết tay cầm. " +
                "Reset Console 1 và Console 2."
            );

            return;
        }

        bool console1NameStillExists =
            ContainsControllerName(
                currentNames,
                console1Name
            );

        bool console2NameStillExists =
            ContainsControllerName(
                currentNames,
                console2Name
            );

        console1Connected =
            !string.IsNullOrWhiteSpace(console1Name) &&
            console1NameStillExists;

        console2Connected =
            !string.IsNullOrWhiteSpace(console2Name) &&
            console2NameStillExists;

        List<string> unassignedControllers =
            new List<string>(currentNames);

        // Loại những tay cầm đã được nhận diện khỏi danh sách chưa gán.
        RemoveOneName(
            unassignedControllers,
            console1Connected
                ? console1Name
                : ""
        );

        RemoveOneName(
            unassignedControllers,
            console2Connected
                ? console2Name
                : ""
        );

        // Chưa có Console 1:
        // tay cầm đầu tiên được gán Console 1.
        if (string.IsNullOrWhiteSpace(console1Name) &&
            unassignedControllers.Count > 0)
        {
            console1Name =
                unassignedControllers[0];

            console1Connected = true;

            unassignedControllers.RemoveAt(0);

            Debug.Log(
                "Gán tay cầm vào Console 1: " +
                console1Name
            );
        }

        // Chưa có Console 2:
        // tay cầm còn lại được gán Console 2.
        if (string.IsNullOrWhiteSpace(console2Name) &&
            unassignedControllers.Count > 0)
        {
            console2Name =
                unassignedControllers[0];

            console2Connected = true;

            unassignedControllers.RemoveAt(0);

            Debug.Log(
                "Gán tay cầm vào Console 2: " +
                console2Name
            );
        }

        // Console 1 đã có tên nhưng đang bị rút.
        // Nếu tay cầm cùng tên cắm lại thì trả về Console 1.
        if (!console1Connected &&
            !string.IsNullOrWhiteSpace(console1Name))
        {
            int matchingIndex =
                FindNameIndex(
                    unassignedControllers,
                    console1Name
                );

            if (matchingIndex >= 0)
            {
                console1Connected = true;

                unassignedControllers.RemoveAt(
                    matchingIndex
                );

                Debug.Log(
                    "Console 1 đã được cắm lại: " +
                    console1Name
                );
            }
        }

        // Console 2 đã có tên nhưng đang bị rút.
        // Nếu tay cầm cùng tên cắm lại thì trả về Console 2.
        if (!console2Connected &&
            !string.IsNullOrWhiteSpace(console2Name))
        {
            int matchingIndex =
                FindNameIndex(
                    unassignedControllers,
                    console2Name
                );

            if (matchingIndex >= 0)
            {
                console2Connected = true;

                unassignedControllers.RemoveAt(
                    matchingIndex
                );

                Debug.Log(
                    "Console 2 đã được cắm lại: " +
                    console2Name
                );
            }
        }

        // Console 1 đang trống, Console 2 vẫn còn
        // và có tay cầm mới được cắm vào.
        if (!console1Connected &&
            unassignedControllers.Count > 0)
        {
            console1Name =
                unassignedControllers[0];

            console1Connected = true;

            unassignedControllers.RemoveAt(0);

            Debug.Log(
                "Gán tay cầm mới vào Console 1: " +
                console1Name
            );
        }

        // Console 2 đang trống và còn tay cầm chưa gán.
        if (!console2Connected &&
            unassignedControllers.Count > 0)
        {
            console2Name =
                unassignedControllers[0];

            console2Connected = true;

            unassignedControllers.RemoveAt(0);

            Debug.Log(
                "Gán tay cầm mới vào Console 2: " +
                console2Name
            );
        }
    }

    // =========================================================
    // GET CONNECTED CONTROLLERS
    // =========================================================

    private List<string> GetConnectedControllerNames()
    {
        string[] joystickNames =
            Input.GetJoystickNames();

        List<string> connectedNames =
            new List<string>();

        if (joystickNames == null)
            return connectedNames;

        for (int i = 0; i < joystickNames.Length; i++)
        {
            string controllerName =
                joystickNames[i];

            if (string.IsNullOrWhiteSpace(
                controllerName))
            {
                continue;
            }

            connectedNames.Add(
                controllerName.Trim()
            );
        }

        return connectedNames;
    }

    // =========================================================
    // HELPER METHODS
    // =========================================================

    private bool ContainsControllerName(
        List<string> names,
        string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            return false;

        for (int i = 0; i < names.Count; i++)
        {
            if (string.Equals(
                names[i],
                targetName,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private int FindNameIndex(
        List<string> names,
        string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            return -1;

        for (int i = 0; i < names.Count; i++)
        {
            if (string.Equals(
                names[i],
                targetName,
                StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private void RemoveOneName(
        List<string> names,
        string targetName)
    {
        int index =
            FindNameIndex(
                names,
                targetName
            );

        if (index >= 0)
        {
            names.RemoveAt(index);
        }
    }

    private bool AreListsEqual(
        List<string> first,
        List<string> second)
    {
        if (first.Count != second.Count)
            return false;

        for (int i = 0; i < first.Count; i++)
        {
            if (!string.Equals(
                first[i],
                second[i],
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // UI / CURSOR
    // =========================================================

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

    private IEnumerator ShowNotificationRoutine()
    {
        yield return StartCoroutine(
            UIManager.Instance.ShowConsoleConect()
        );

        notificationCoroutine = null;
    }

    private void PrintControllerState()
    {
        Debug.Log(
            "Console 1: " +
            (console1Connected
                ? "Connected - " + console1Name
                : "Disconnected - Saved: " + console1Name)
        );

        Debug.Log(
            "Console 2: " +
            (console2Connected
                ? "Connected - " + console2Name
                : "Disconnected - Saved: " + console2Name)
        );
    }

    // =========================================================
    // PUBLIC METHODS
    // =========================================================

    public bool IsConsole1Connected()
    {
        return console1Connected;
    }

    public bool IsConsole2Connected()
    {
        return console2Connected;
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
}