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
  

    public CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        manager = GetComponent<PlayerManager>();
    }

    void Update()
    {
        if (isJumpAndMove)
        {
            if (isMove)
            {
                Move();
            }

            if (isJump)
            {
                JumpAndGravity();
            }

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

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

            controller.Move(move.normalized * speed * Time.deltaTime);
        }

        manager.playerAnimator.playerAnimator.SetFloat("Run", move.magnitude);
    }

    void JumpAndGravity()
    {
        

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        bool jumpPressed =
            !manager.playerType.isPlayer2 ? Input.GetKeyDown(KeyCode.Space)
                      : Input.GetKeyDown(KeyCode.Keypad0); // hoặc Alpha0

        if (isGround && jumpPressed)
        {
            manager.playerAnimator.playerAnimator.SetTrigger("Jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGround = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
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