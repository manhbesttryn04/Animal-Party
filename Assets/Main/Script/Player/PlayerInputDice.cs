using UnityEngine;

public class PlayerInputDice : MonoBehaviour
{
    [Header("References")]
    public PlayerManager manager;

    [Header("Dice State")]
    public bool isClick = true;

    // Sau khi đóng Setting phải thả nút rồi mới nhận input lại.
    private bool waitReleaseAfterSetting;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        HandleDiceInput();
    }

    // =========================================================
    // PLAYER
    // =========================================================

    private bool IsPlayer2()
    {
        return manager != null &&
               manager.playerType != null &&
               manager.playerType.isPlayer2;
    }

    // =========================================================
    // CONTROLLER
    // =========================================================

    private bool IsUsingController()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
            return false;

        return IsPlayer2()
            ? controller.IsConsole2Connected()
            : controller.IsConsole1Connected();
    }

    private int GetConsoleNumber()
    {
        return IsPlayer2() ? 2 : 1;
    }

    // =========================================================
    // DICE INPUT
    // =========================================================

    private bool IsDicePressed()
    {
        /*
         * Có tay cầm:
         * chỉ nhận Button 0 của đúng Console.
         */
        if (IsUsingController())
        {
            return ControllerManager.Instance
                .GetConsoleButtonDown(
                    GetConsoleNumber(),
                    0
                );
        }

        /*
         * Không có tay cầm:
         * mới nhận bàn phím.
         */
        if (IsPlayer2())
        {
            return Input.GetKeyDown(
                KeyCode.Keypad1
            );
        }

        return Input.GetKeyDown(
            KeyCode.J
        );
    }

    private bool IsDiceHeld()
    {
        /*
         * Dùng để kiểm tra người chơi đã thả nút
         * sau khi đóng Setting chưa.
         */
        if (IsUsingController())
        {
            return ControllerManager.Instance
                .GetConsoleButton(
                    GetConsoleNumber(),
                    0
                );
        }

        if (IsPlayer2())
        {
            return Input.GetKey(
                KeyCode.Keypad1
            );
        }

        return Input.GetKey(
            KeyCode.J
        );
    }

    // =========================================================
    // SETTING
    // =========================================================

    private bool IsSettingOpen()
    {
        return SettingManager.Instance != null &&
               SettingManager.Instance
                   .IsSettingBlockingInput;
    }

    // =========================================================
    // HANDLE INPUT
    // =========================================================

    private void HandleDiceInput()
    {
        /*
         * Setting đang mở:
         * khóa hoàn toàn input xúc xắc.
         */
        if (IsSettingOpen())
        {
            waitReleaseAfterSetting = true;
            return;
        }

        /*
         * Sau khi đóng Setting:
         * phải thả Button 0 / J / Keypad1.
         *
         * Điều này ngăn nút dùng để đóng Setting
         * kích hoạt luôn xúc xắc.
         */
        if (waitReleaseAfterSetting)
        {
            if (!IsDiceHeld())
            {
                waitReleaseAfterSetting = false;
            }

            return;
        }

        ExitsInput();
    }

    // =========================================================
    // DICE
    // =========================================================

    public void ExitsInput()
    {
        if (isClick)
            return;

        if (!IsDicePressed())
            return;

        if (manager == null ||
            manager.playerAnimator == null ||
            manager.playerAnimator.playerAnimator == null)
        {
            return;
        }

        manager.playerAnimator
            .playerAnimator
            .SetTrigger("Dice");

        isClick = true;
    }
}