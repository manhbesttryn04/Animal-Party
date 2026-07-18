using UnityEngine;

namespace AnimalParty.Testing
{
    [RequireComponent(typeof(Rigidbody))]
    public class AdvancedTestMovement : MonoBehaviour
    {
        [Header("--- Di chuyển & Trọng lực ---")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("Lực bật nhảy. Hãy thử ở mức 10 - 15")]
        [SerializeField] private float jumpForce = 12f; 

        [Header("--- Chạm Đất (Ground Check) ---")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.3f;
        [SerializeField] private LayerMask groundMask;

        [Header("--- Hoạt ảnh (Animation) ---")]
        [SerializeField] private Animator animator;

        private Rigidbody _rb;
        private Vector3 _moveInput;
        private bool _isGrounded;
        private Transform _mainCamera;
        
        // CỜ HIỆU BÁO NHẢY: Giải pháp vàng để không bị Unity nuốt lực nhảy
        private bool _wantsToJump;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (Camera.main != null) _mainCamera = Camera.main.transform;
        }

        private void Update()
        {
            // 1. KIỂM TRA CHẠM ĐẤT
            if (groundCheck != null)
            {
                _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }

            // 2. NHẬN LỆNH DI CHUYỂN
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            if (_mainCamera != null)
            {
                Vector3 camForward = _mainCamera.forward;
                Vector3 camRight = _mainCamera.right;
                camForward.y = 0f;
                camRight.y = 0f;
                _moveInput = (camForward.normalized * moveZ + camRight.normalized * moveX).normalized;
            }
            else
            {
                _moveInput = new Vector3(moveX, 0f, moveZ).normalized;
            }

            // 3. XOAY MẶT
            if (_moveInput.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveInput);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }

            // 4. BẮT TÍN HIỆU BẤM PHÍM
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _wantsToJump = true;
                if (animator != null) animator.SetTrigger("Jump");
            }

            // 5. CẬP NHẬT ANIMATOR
            if (animator != null) animator.SetFloat("Run", _moveInput.magnitude);
        }

        private void FixedUpdate()
        {
            // DI CHUYỂN NGANG
            Vector3 targetVelocity = _moveInput * moveSpeed;
            Vector3 currentXZVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            Vector3 velocityChange = targetVelocity - currentXZVelocity;
            _rb.AddForce(velocityChange, ForceMode.VelocityChange);

            // THỰC THI LỆNH NHẢY
            if (_wantsToJump)
            {
                // Ép thẳng vận tốc Y để nhân vật bay lên
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
                _wantsToJump = false; 
            }
        }
    }
}