using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public PlayerManager manager;
    public bool isPlayer1 = true;

    public float speed = 5f;
    public float turnSpeed = 10f;

    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public bool isGround;
    public bool isJumpAndMove, isMove, isJump = true;
    public bool IsMoving { get; private set; }

    [Header("Lie Settings")]
    public bool hasLie = false;
    private bool canLie = true;

    public float lieHeight = 0.5f;
    public Vector3 lieCenter = new Vector3(0f, 0.25f, 0f);

    private float normalHeight;
    private Vector3 normalCenter;

    public CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        manager = GetComponent<PlayerManager>();
        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    void Update()
    {
        CheckLieInput();

        if (isJumpAndMove)
        {
            if (isMove)
                Move();

            if (isJump)
                JumpInput();

            ApplyGravity();
        }
    }
    void Move()
    {
  
        Vector3 move = Vector3.zero;

        if (!manager.playerType.isPlayer2)
        {
            if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) move += Vector3.back;
            if (Input.GetKey(KeyCode.A)) move += Vector3.left;
            if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow)) move += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow)) move += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow)) move += Vector3.right;
        }

        IsMoving = move.magnitude > 0.1f;

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

            controller.Move(move.normalized * speed * Time.deltaTime);
        }

        manager.playerAnimator.playerAnimator.SetFloat("Run", move.magnitude);
    }

    void JumpInput()
    {
        bool jumpPressed =
            !manager.playerType.isPlayer2
            ? Input.GetKeyDown(KeyCode.Space)
            : Input.GetKeyDown(KeyCode.Keypad0);

        if (isGround && jumpPressed)
        {
            manager.playerAnimator.playerAnimator.SetTrigger("Jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGround = false;
        }
    }
    void ApplyGravity()
    {
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void CheckLieInput()
    {
        if (!hasLie) return;
        if (!canLie) return;
        if (!isGround || velocity.y > 0.1f)
            return;

        bool liePressed = !manager.playerType.isPlayer2
            ? Input.GetKeyDown(KeyCode.K)
            : Input.GetKeyDown(KeyCode.Keypad3);

        if (liePressed)
        {
            canLie = false;
            manager.playerAnimator.playerAnimator.SetTrigger("Lie");
        }
    }

    // Animation Event: frame bắt đầu nằm
    public void StartLie()
    {
        if (!isGround) return;
        isMove = false;
        isJump = false;

        if (manager.playerAttack != null)
            manager.playerAttack.hasAttack = false;

        controller.height = lieHeight;
        controller.center = lieCenter;

        manager.playerAnimator.playerAnimator.SetFloat("Run", 0f);
    }

    // Animation Event: frame đứng dậy / hết nằm
    public void StopLie()
    {
        isMove = true;
        isJump = true;

        if (manager.playerAttack != null)
            manager.playerAttack.hasAttack = true;

        controller.height = normalHeight;
        controller.center = normalCenter;

        canLie = true;
    }
  
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ground"))
        {
            if (!isGround)
            {
                isGround = true;
            }
        }
    }
}