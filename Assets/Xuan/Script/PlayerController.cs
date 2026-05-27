using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerId { Player1, Player2 }

    [Header("Identify Player")]
    public PlayerId playerId = PlayerId.Player1; // Chọn ID cho từng Player trong Inspector

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private float moveHorizontal;
    private float moveVertical;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Phân biệt nút bấm dựa trên Player ID
        if (playerId == PlayerId.Player1)
        {
            // Player 1: Dùng A/D và W/S
            if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f;
            else if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f;
            else moveHorizontal = 0f;

            if (Input.GetKey(KeyCode.S)) moveVertical = -1f;
            else if (Input.GetKey(KeyCode.W)) moveVertical = 1f;
            else moveVertical = 0f;

            // Player 1 Nhảy bằng phím K
            if (Input.GetKeyDown(KeyCode.K) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else if (playerId == PlayerId.Player2)
        {
            // Player 2: Dùng 4 phím Mũi tên
            if (Input.GetKey(KeyCode.LeftArrow)) moveHorizontal = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) moveHorizontal = 1f;
            else moveHorizontal = 0f;

            if (Input.GetKey(KeyCode.DownArrow)) moveVertical = -1f;
            else if (Input.GetKey(KeyCode.UpArrow)) moveVertical = 1f;
            else moveVertical = 0f;

            // Player 2 Nhảy bằng phím L
            if (Input.GetKeyDown(KeyCode.L) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        // Kiểm tra chạm đất
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized;
        rb.linearVelocity = new Vector3(movement.x * moveSpeed, rb.linearVelocity.y, movement.z * moveSpeed);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}