using System.Collections;
using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public PlayerMove playerMove;
    public GameObject shield;

    [Header("Defense Settings")]
    public bool hasDefense = true;

    [Tooltip("Thời gian khiên được bật")]
    public float defenseDuration = 0.3f;

    [Tooltip("Thời gian chờ sau khi khiên tắt")]
    public float defenseCooldown = 2f;

    [Header("Defense State")]
    public bool isDefending;
    public bool canDefense = true;

    private Coroutine defenseCoroutine;

    private void Start()
    {
        if (playerMove == null)
        {
            playerMove = GetComponent<PlayerMove>();
        }

        if (shield != null)
        {
            shield.SetActive(false);
        }
    }

    private void Update()
    {
        if (!hasDefense)
            return;

        if (!canDefense || isDefending)
            return;

        if (playerManager == null ||
            playerManager.playerType == null ||
            playerMove == null)
            return;

        bool defensePressed;

        // Player 1
        if (!playerManager.playerType.isPlayer2)
        {
            defensePressed =
                Input.GetKeyDown(KeyCode.J) ||
                Input.GetKeyDown(KeyCode.Joystick1Button1);
        }
        // Player 2
        else
        {
            defensePressed =
                Input.GetKeyDown(KeyCode.Keypad1) ||
                Input.GetKeyDown(KeyCode.Joystick2Button1);
        }

        if (!defensePressed)
            return;

        // Đang ở trên không thì không được Defense
        if (!playerMove.isGround)
            return;

        StartDefense();
    }

    private void StartDefense()
    {
        if (!canDefense || isDefending)
            return;

        if (defenseCoroutine != null)
        {
            StopCoroutine(defenseCoroutine);
        }

        defenseCoroutine = StartCoroutine(DefenseRoutine());
    }

    private IEnumerator DefenseRoutine()
    {
        isDefending = true;
        canDefense = false;

        // Khóa di chuyển và nhảy
        if (playerMove != null)
        {
            playerMove.isJumpAndMove = false;
        }

        // Chạy animation Defense
        if (playerManager != null &&
            playerManager.playerAnimator != null &&
            playerManager.playerAnimator.playerAnimator != null)
        {
            Animator animator =
                playerManager.playerAnimator.playerAnimator;

            animator.SetFloat("Walk", 0f);
            animator.SetTrigger("Defense");
        }

        // Bật khiên ngay khi Defense bắt đầu
        if (shield != null)
        {
            shield.SetActive(true);
        }

        // Khiên tồn tại trong 2.14 giây
        yield return new WaitForSeconds(defenseDuration);

        // Tắt khiên
        if (shield != null)
        {
            shield.SetActive(false);
        }

        isDefending = false;

        // Mở lại chạy và nhảy
        if (playerMove != null)
        {
            playerMove.isJumpAndMove = true;
        }

        // Chờ thêm 2 giây cooldown
        yield return new WaitForSeconds(defenseCooldown);

        canDefense = true;
        defenseCoroutine = null;
    }

    private void OnDisable()
    {
        if (defenseCoroutine != null)
        {
            StopCoroutine(defenseCoroutine);
            defenseCoroutine = null;
        }

        isDefending = false;
        canDefense = true;

        if (playerMove != null)
        {
            playerMove.isJumpAndMove = true;
        }

        if (shield != null)
        {
            shield.SetActive(false);
        }
    }
}