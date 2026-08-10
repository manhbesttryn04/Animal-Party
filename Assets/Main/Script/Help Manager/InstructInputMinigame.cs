using System.Collections.Generic;
using UnityEngine;

public class InstructInputMinigame : MonoBehaviour
{
    // =========================================================
    // MINIGAME INPUT CONFIG
    // =========================================================

    [Header("Input Minigame 1")]
    public List<bool> instructInputMinigame1 = new List<bool>();

    [Header("Input Minigame 2")]
    public List<bool> instructInputMinigame2 = new List<bool>();

    [Header("Input Minigame 3")]
    public List<bool> instructInputMinigame3 = new List<bool>();

    [Header("Input Minigame 4")]
    public List<bool> instructInputMinigame4 = new List<bool>();

    [Header("Input Minigame 5")]
    public List<bool> instructInputMinigame5 = new List<bool>();

    [Header("Input Minigame 6")]
    public List<bool> instructInputMinigame6 = new List<bool>();

    [Header("Input Minigame 7")]
    public List<bool> instructInputMinigame7 = new List<bool>();

    [Header("Input Minigame 8")]
    public List<bool> instructInputMinigame8 = new List<bool>();

    private const int INPUT_COUNT = 5;

    // =========================================================
    // CURRENT STATE
    // =========================================================

    private int currentMinigame = -1;

    private bool isInstructionShowing;

    private bool lastP1Controller;
    private bool lastP2Controller;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        HideAllInput();

        CacheControllerState();
    }

    private void Update()
    {
        if (!isInstructionShowing)
            return;

        ControllerManager controllerManager =
            ControllerManager.Instance;

        bool p1Controller =
            controllerManager != null &&
            controllerManager.IsConsole1Connected();

        bool p2Controller =
            controllerManager != null &&
            controllerManager.IsConsole2Connected();

        // Chỉ refresh khi trạng thái tay cầm thay đổi.
        if (p1Controller != lastP1Controller ||
            p2Controller != lastP2Controller)
        {
            lastP1Controller = p1Controller;
            lastP2Controller = p2Controller;

            RefreshCurrentInput();
        }
    }

    // =========================================================
    // SHOW INPUT
    // =========================================================

    /// <summary>
    /// indexMinigame chạy từ 1 đến 8.
    /// </summary>
    public void ShowInputMinigame(int indexMinigame)
    {
        int listIndex = indexMinigame - 1;

        List<bool> selectedBoolList =
            GetBoolList(listIndex);

        if (selectedBoolList == null)
        {
            Debug.LogWarning(
                $"Index minigame không hợp lệ: {indexMinigame}. " +
                "Index phải từ 1 đến 8."
            );

            HideAllInput();
            return;
        }

        currentMinigame = indexMinigame;
        isInstructionShowing = true;

        CacheControllerState();

        ApplyBoolToInput(selectedBoolList);
    }

    // =========================================================
    // REFRESH CURRENT INPUT
    // =========================================================

    public void RefreshCurrentInput()
    {
        if (!isInstructionShowing)
            return;

        if (currentMinigame < 1 ||
            currentMinigame > 8)
        {
            return;
        }

        List<bool> boolList =
            GetBoolList(currentMinigame - 1);

        if (boolList == null)
            return;

        ApplyBoolToInput(boolList);
    }

    // =========================================================
    // GET MINIGAME BOOL LIST
    // =========================================================

    private List<bool> GetBoolList(int listIndex)
    {
        switch (listIndex)
        {
            case 0:
                return instructInputMinigame1;

            case 1:
                return instructInputMinigame2;

            case 2:
                return instructInputMinigame3;

            case 3:
                return instructInputMinigame4;

            case 4:
                return instructInputMinigame5;

            case 5:
                return instructInputMinigame6;

            case 6:
                return instructInputMinigame7;

            case 7:
                return instructInputMinigame8;

            default:
                return null;
        }
    }

    // =========================================================
    // APPLY INPUT UI
    // =========================================================

    private void ApplyBoolToInput(
        List<bool> boolList)
    {
        UIManager ui = UIManager.Instance;

        if (ui == null)
            return;

        ControllerManager controllerManager =
            ControllerManager.Instance;

        bool p1Controller =
            controllerManager != null &&
            controllerManager.IsConsole1Connected();

        bool p2Controller =
            controllerManager != null &&
            controllerManager.IsConsole2Connected();

        bool bothController =
            p1Controller &&
            p2Controller;

        // =====================================================
        // RESET TẤT CẢ INPUT ICON
        // =====================================================

        SetListState(
            ui.instructKeyBoardInputListP1,
            boolList,
            false
        );

        SetListState(
            ui.instructKeyBoardInputListP2,
            boolList,
            false
        );

        SetListState(
            ui.instructConsoleInputListP1,
            boolList,
            false
        );

        SetListState(
            ui.instructConsoleInputListP2,
            boolList,
            false
        );

        SetListState(
            ui.instructBothConsoleInputListP1,
            boolList,
            false
        );

        // =====================================================
        // TRƯỜNG HỢP 1:
        // CẢ 2 PLAYER DÙNG CONTROLLER
        // =====================================================

        if (bothController)
        {
            // Tắt toàn bộ Keyboard.
            SetActiveSafe(
                ui.instructKeyBoard,
                false
            );

            SetActiveSafe(
                ui.instructKeyBoardP1,
                false
            );

            SetActiveSafe(
                ui.instructKeyBoardP2,
                false
            );

            // Tắt Console Clone riêng P1/P2.
            SetActiveSafe(
                ui.instructConsoleClone,
                false
            );

            SetActiveSafe(
                ui.instructConsoleCloneP1,
                false
            );

            SetActiveSafe(
                ui.instructConsoleCloneP2,
                false
            );

            // Hiện UI dành cho cả 2 Controller.
            SetActiveSafe(
                ui.instructBothConsole,
                true
            );

            // Áp đúng List<bool> của minigame.
            SetListState(
                ui.instructBothConsoleInputListP1,
                boolList,
                true
            );

            return;
        }

        // =====================================================
        // KHÔNG PHẢI CẢ 2 CONTROLLER
        // =====================================================

        SetActiveSafe(
            ui.instructBothConsole,
            false
        );

        // =====================================================
        // KEYBOARD ROOT
        // =====================================================

        bool hasKeyboardPlayer =
            !p1Controller ||
            !p2Controller;

        SetActiveSafe(
            ui.instructKeyBoard,
            hasKeyboardPlayer
        );

        // =====================================================
        // PLAYER 1 KEYBOARD
        // =====================================================

        bool showKeyboardP1 =
            !p1Controller;

        SetActiveSafe(
            ui.instructKeyBoardP1,
            showKeyboardP1
        );

        SetListState(
            ui.instructKeyBoardInputListP1,
            boolList,
            showKeyboardP1
        );

        // =====================================================
        // PLAYER 2 KEYBOARD
        // =====================================================

        bool showKeyboardP2 =
            !p2Controller;

        SetActiveSafe(
            ui.instructKeyBoardP2,
            showKeyboardP2
        );

        SetListState(
            ui.instructKeyBoardInputListP2,
            boolList,
            showKeyboardP2
        );

        // =====================================================
        // CONSOLE CLONE ROOT
        // =====================================================

        bool hasControllerPlayer =
            p1Controller ||
            p2Controller;

        SetActiveSafe(
            ui.instructConsoleClone,
            hasControllerPlayer
        );

        // =====================================================
        // PLAYER 1 CONTROLLER
        // =====================================================

        SetActiveSafe(
            ui.instructConsoleCloneP1,
            p1Controller
        );

        SetListState(
            ui.instructConsoleInputListP1,
            boolList,
            p1Controller
        );

        // =====================================================
        // PLAYER 2 CONTROLLER
        // =====================================================

        SetActiveSafe(
            ui.instructConsoleCloneP2,
            p2Controller
        );

        SetListState(
            ui.instructConsoleInputListP2,
            boolList,
            p2Controller
        );
    }

    // =========================================================
    // APPLY BOOL LIST
    // =========================================================

    private void SetListState(
        List<GameObject> objectList,
        List<bool> boolList,
        bool allowShow)
    {
        if (objectList == null)
            return;

        for (int i = 0;
             i < objectList.Count;
             i++)
        {
            GameObject inputObject =
                objectList[i];

            if (inputObject == null)
                continue;

            bool state = false;

            if (allowShow &&
                boolList != null &&
                i < boolList.Count &&
                boolList[i])
            {
                state = true;
            }

            inputObject.SetActive(state);
        }
    }

    // =========================================================
    // SAFE ACTIVE
    // =========================================================

    private void SetActiveSafe(
        GameObject target,
        bool state)
    {
        if (target != null)
        {
            target.SetActive(state);
        }
    }

    // =========================================================
    // CACHE CONTROLLER
    // =========================================================

    private void CacheControllerState()
    {
        ControllerManager controllerManager =
            ControllerManager.Instance;

        lastP1Controller =
            controllerManager != null &&
            controllerManager.IsConsole1Connected();

        lastP2Controller =
            controllerManager != null &&
            controllerManager.IsConsole2Connected();
    }

    // =========================================================
    // HIDE ALL
    // =========================================================

    public void HideAllInput()
    {
        isInstructionShowing = false;
        currentMinigame = -1;

        UIManager ui = UIManager.Instance;

        if (ui == null)
            return;

        // ROOT
        SetActiveSafe(
            ui.instructKeyBoard,
            false
        );

        SetActiveSafe(
            ui.instructKeyBoardP1,
            false
        );

        SetActiveSafe(
            ui.instructKeyBoardP2,
            false
        );

        SetActiveSafe(
            ui.instructConsoleClone,
            false
        );

        SetActiveSafe(
            ui.instructConsoleCloneP1,
            false
        );

        SetActiveSafe(
            ui.instructConsoleCloneP2,
            false
        );

        SetActiveSafe(
            ui.instructBothConsole,
            false
        );

        // LIST KEYBOARD P1
        HideList(
            ui.instructKeyBoardInputListP1
        );

        // LIST KEYBOARD P2
        HideList(
            ui.instructKeyBoardInputListP2
        );

        // LIST CONTROLLER P1
        HideList(
            ui.instructConsoleInputListP1
        );

        // LIST CONTROLLER P2
        HideList(
            ui.instructConsoleInputListP2
        );

        // LIST BOTH CONTROLLER
        HideList(
            ui.instructBothConsoleInputListP1
        );
    }

    // =========================================================
    // HIDE LIST
    // =========================================================

    private void HideList(
        List<GameObject> objectList)
    {
        if (objectList == null)
            return;

        foreach (GameObject obj in objectList)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}