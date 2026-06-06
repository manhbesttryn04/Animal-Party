using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PummelPlayerController : MonoBehaviour
{
    [Header("Thông số Di chuyển")]
    public float moveSpeed = 5f; // Tốc độ đã được chỉnh chậm lại cho map nhỏ
    public float turnSpeed = 900f;

    [Header("Thông số Nhảy (Arcade Jump)")]
    public float jumpHeight = 2.5f;
    public float gravity = 30f; 

    [Header("Cấu hình Hoạt họa")]
    public Transform visualModel;
    public float BobSpeed = 10f;
    public float BobAmount = 0.1f;
    public float leanAmount = 15f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 moveDirection;

    private Vector3 originalScale;
    private float bobTimer = 0f;
    private bool wasGroundedLastFrame;

    // Biến lưu trữ Camera chính của game
    private Transform mainCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Tự động tìm Main Camera trong màn chơi
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        if (visualModel == null && transform.childCount > 0)
        {
            visualModel = transform.GetChild(0); 
        }

        if (visualModel != null)
        {
            originalScale = visualModel.localScale;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // 1. Hiệu ứng lún xuống khi tiếp đất
        if (isGrounded && !wasGroundedLastFrame)
        {
            if (visualModel != null)
            {
                visualModel.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z * 1.3f);
            }
        }

        // 2. Lấy Input từ bàn phím
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // ---------------------------------------------------------
        // LOGIC CHỐNG LOẠN PHÍM (Camera-Relative Movement)
        // ---------------------------------------------------------
        if (mainCamera != null)
        {
            // Lấy hướng đằng trước và hướng ngang của Camera
            Vector3 camForward = mainCamera.forward;
            Vector3 camRight = mainCamera.right;

            // Xóa bỏ trục Y để nhân vật không bị bay lên trời khi Camera nhìn chúc xuống
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Tính toán lại hướng đi thực tế ráp với góc nhìn màn hình
            moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        }
        else
        {
            // Nếu không tìm thấy Camera, dùng cách cũ
            moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        }
        // ---------------------------------------------------------

        // 3. Xử lý lệnh nhảy
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            if (visualModel != null)
            {
                visualModel.localScale = new Vector3(originalScale.x * 0.7f, originalScale.y * 1.4f, originalScale.z * 0.7f);
            }
        }

        // 4. Áp dụng trọng lực rơi tự do
        velocity.y -= gravity * Time.deltaTime;

        // ---------------------------------------------------------
        // BỘ KHUNG ĐÃ ĐƯỢC FIX LỖI LƠ LỬNG
        // Gộp chung di chuyển ngang (MoveX/Z) và rơi dọc (Y) vào 1 Vector3
        // ---------------------------------------------------------
        Vector3 finalMovement = moveDirection * moveSpeed; 
        finalMovement.y = velocity.y; 

        // Gọi lệnh Move 1 lần duy nhất cho toàn bộ hệ thống
        controller.Move(finalMovement * Time.deltaTime);
        // ---------------------------------------------------------

        // 5. Xoay nhân vật theo hướng di chuyển
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // 6. Chạy hoạt họa giả lập (Procedural Animation)
        HandleProceduralAnimation();

        // Ghi nhớ trạng thái để dùng cho khung hình tiếp theo
        wasGroundedLastFrame = isGrounded;
    }

    private void HandleProceduralAnimation()
    {
        if (visualModel == null) return;

        visualModel.localScale = Vector3.Lerp(visualModel.localScale, originalScale, 10f * Time.deltaTime);

        if (isGrounded)
        {
            if (moveDirection.magnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * BobSpeed;
                float newY = originalScale.y + Mathf.Sin(bobTimer) * BobAmount;
                visualModel.localScale = new Vector3(originalScale.x, newY, originalScale.z);
                visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, Quaternion.Euler(leanAmount, 0f, 0f), 10f * Time.deltaTime);
            }
            else
            {
                bobTimer = 0f;
                visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, Quaternion.identity, 10f * Time.deltaTime);
                float idleBob = originalScale.y + Mathf.Sin(Time.time * 3f) * 0.02f;
                visualModel.localScale = new Vector3(originalScale.x, idleBob, originalScale.z);
            }
        }
        else
        {
            visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, Quaternion.identity, 5f * Time.deltaTime);
        }
    }
}