using UnityEngine;

namespace AnimalParty.Testing
{
    [RequireComponent(typeof(Rigidbody))]
    public class TestMovement : MonoBehaviour
    {
        [Header("--- Cài đặt di chuyển Test ---")]
        [SerializeField] private float moveSpeed = 7f;

        private Rigidbody _rb;
        private Vector3 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Lấy input từ phím WASD hoặc các phím mũi tên trên bàn phím
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            // Tạo vector hướng đi trên mặt phẳng XZ
            _moveInput = new Vector3(moveX, 0f, moveZ).normalized;

            // Xoay mặt nhân vật theo hướng di chuyển cho tự nhiên
            if (_moveInput != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveInput);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        private void FixedUpdate()
        {
            // Áp dụng vận tốc di chuyển nhưng vẫn giữ nguyên vận tốc trục Y (để trọng lực rơi tự do hoạt động)
            Vector3 targetVelocity = _moveInput * moveSpeed;
            
            // Unity 6 sử dụng linearVelocity thay cho velocity cũ
            _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
        }
    }
}