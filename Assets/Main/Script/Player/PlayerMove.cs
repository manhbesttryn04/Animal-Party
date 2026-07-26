using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public PlayerManager manager;
    public bool isPlayer1 = true;

    [Header("Move Settings")]
    public float speed = 5f;
    public float turnSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Move State")]
    public bool isGround;
    public bool isJumpAndMove;
    public bool isMove;
    public bool isJump = true;
    public bool isWalk = false;

    public bool IsMoving { get; private set; }

    [Header("Lie Settings")]
    public bool hasLie = false;
    private bool canLie = true;

    public float lieHeight = 0.5f;
    public Vector3 lieCenter = new Vector3(0f, 0.25f, 0f);

    private float normalHeight;
    private Vector3 normalCenter;

    [Header("References")]
    public CharacterController controller;

    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        manager = GetComponent<PlayerManager>();

        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    private void Update()
    {
        CheckLieInput();

        if (isJumpAndMove)
        {
            if (isMove)
            {
                Move();
            }
            else
            {
                StopMoveAnimation();
            }

            if (isJump)
            {
                JumpInput();
            }
        }
        else
        {
            StopMoveAnimation();
        }

        ApplyGravity();
    }

    private void Move()
    {
        float horizontal;
        float vertical;

        if (!manager.playerType.isPlayer2)
        {
            horizontal = Input.GetAxisRaw("HorizontalP1");
            vertical = Input.GetAxisRaw("VerticalP1");
        }
        else
        {
            horizontal = Input.GetAxisRaw("HorizontalP2");
            vertical = Input.GetAxisRaw("VerticalP2");
        }

        Vector3 move = new Vector3(horizontal, 0f, vertical);

        // Tránh đi chéo nhanh hơn.
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        IsMoving = move.sqrMagnitude > 0.01f;

        if (IsMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            controller.Move(move * speed * Time.deltaTime);
        }

        UpdateMoveAnimation(move.magnitude);
    }

    private void UpdateMoveAnimation(float moveAmount)
    {
        if (manager == null ||
            manager.playerAnimator == null ||
            manager.playerAnimator.playerAnimator == null)
        {
            return;
        }

        if (!isWalk)
        {
            manager.playerAnimator.playerAnimator.SetFloat("Run", moveAmount);
            manager.playerAnimator.playerAnimator.SetFloat("Walk", 0f);
        }
        else
        {
            manager.playerAnimator.playerAnimator.SetFloat("Walk", moveAmount);
            manager.playerAnimator.playerAnimator.SetFloat("Run", 0f);
        }
    }

    private void StopMoveAnimation()
    {
        IsMoving = false;
        UpdateMoveAnimation(0f);
    }

    private void JumpInput()
    {
        bool jumpPressed;

        if (!manager.playerType.isPlayer2)
        {
            jumpPressed =
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Joystick1Button0);
        }
        else
        {
            jumpPressed =
                Input.GetKeyDown(KeyCode.Keypad0) ||
                Input.GetKeyDown(KeyCode.Joystick2Button0);
        }

        if (isGround && jumpPressed)
        {
            manager.playerAnimator.playerAnimator.SetTrigger("Jump");

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGround = false;
        }
    }

    private void ApplyGravity()
    {
        if (isGround && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckLieInput()
    {
        if (!hasLie)
            return;

        if (!canLie)
            return;

        if (!isGround || velocity.y > 0.1f)
            return;

        bool liePressed;

        if (!manager.playerType.isPlayer2)
        {
            liePressed =
                Input.GetKeyDown(KeyCode.L) ||
                Input.GetKeyDown(KeyCode.Joystick1Button2);
        }
        else
        {
            liePressed =
                Input.GetKeyDown(KeyCode.Keypad2) ||
                Input.GetKeyDown(KeyCode.Joystick2Button2);
        }

        if (liePressed)
        {
            canLie = false;
            manager.playerAnimator.playerAnimator.SetTrigger("Lie");
        }
    }

    // Animation Event: frame bắt đầu nằm
    public void StartLie()
    {
        if (!isGround)
            return;

        isMove = false;
        isJump = false;

        if (manager.playerAttack != null)
        {
            manager.playerAttack.hasAttack = false;
        }

        controller.height = lieHeight;
        controller.center = lieCenter;

        StopMoveAnimation();
    }

    // Animation Event: frame đứng dậy / hết nằm
    public void StopLie()
    {
        isMove = true;
        isJump = true;

        if (manager.playerAttack != null)
        {
            manager.playerAttack.hasAttack = true;
        }

        controller.height = normalHeight;
        controller.center = normalCenter;

        canLie = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }
}