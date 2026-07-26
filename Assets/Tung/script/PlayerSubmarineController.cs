/*
PlayerSubmarineController.cs
Thay thế PlayerCarController.cs - giữ nguyên khung sườn gameplay (playerId, SetGameActive,
2 người chơi 2 bộ phím riêng, húc nhau khi va chạm) nhưng đổi hoàn toàn phần vật lý di chuyển:
- Không dùng WheelCollider/steering kiểu xe.
- Dùng Rigidbody + AddForce/AddTorque để tàu ngầm tiến/lùi/xoay/nổi/lặn tự do trong nước.

CÁC HÀM PUBLIC GIỮ NGUYÊN TÊN so với PlayerCarController để RaceMiniGame/manager cũ
đỡ phải sửa nhiều: playerId, SetGameActive(bool), carSpeed (đổi tên currentSpeed nhưng
giữ lại 1 property carSpeed trỏ qua cho tương thích ngược nếu cần).
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerSubmarineController : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }

    [Header("--- PLAYER ---")]
    public PlayerID playerId = PlayerID.Player1;

    [Header("--- SUBMARINE SETUP ---")]
    [Tooltip("Lực đẩy tiến/lùi")]
    public float thrustForce = 25f;
    [Tooltip("Tốc độ tối đa (m/s)")]
    public float maxSpeed = 12f;
    [Tooltip("Tốc độ lùi tối đa (m/s)")]
    public float maxReverseSpeed = 6f;
    [Tooltip("Tốc độ xoay trái/phải (độ/giây)")]
    public float turnSpeed = 60f;
    [Tooltip("Lực nổi lên/lặn xuống")]
    public float verticalThrustForce = 15f;
    [Tooltip("Tốc độ nổi/lặn tối đa (m/s)")]
    public float maxVerticalSpeed = 5f;
    [Tooltip("Lực cản nước - càng cao càng mau dừng khi buông ga")]
    public float waterDrag = 1.5f;
    [Tooltip("Giới hạn độ sâu tối đa được lặn xuống (world Y)")]
    public float minDepthY = -20f;
    [Tooltip("Giới hạn độ cao tối đa được nổi lên (world Y)")]
    public float maxDepthY = 0f;

    [Header("--- VA CHẠM / HÚC NHAU ---")]
    [Tooltip("Lực húc tàu kia khi đâm vào")]
    public float pushForce = 6f;

    [Header("--- ANIMATION ---")]
    [Tooltip("Animator của model tàu ngầm - có sẵn bool isWalking/isRunning từ asset")]
    public Animator animator;
    [Tooltip("Tốc độ (m/s) bắt đầu tính là 'Walk' thay vì 'Idle'")]
    public float walkSpeedThreshold = 1f;
    [Tooltip("Tốc độ (m/s) bắt đầu tính là 'Run' thay vì 'Walk'")]
    public float runSpeedThreshold = 7f;

    [Header("--- NGƯ LÔI ---")]
    [Tooltip("Điểm bắn ngư lôi bên trái, kéo object Torpedo_L trong Hierarchy vào đây")]
    public Transform torpedoPointLeft;
    [Tooltip("Điểm bắn ngư lôi bên phải, kéo object Torpedo_R trong Hierarchy vào đây")]
    public Transform torpedoPointRight;
    public GameObject torpedoPrefab;
    public float fireCooldown = 1.5f;
    private float lastFireTime = -10f;
    private bool fireFromLeft = true; // luân phiên trái/phải mỗi lần bắn cho đẹp

    [Header("--- STUN (khi trúng ngư lôi) ---")]
    [HideInInspector] public bool isStunned = false;
    private float stunTimer = 0f;

    [HideInInspector] public float currentSpeed;
    // Giữ tương thích ngược với code cũ từng đọc carSpeed
    public float carSpeed => currentSpeed;

    // ====== INTERNAL ======
    Rigidbody subRigidbody;
    bool gameActive = false;
    float verticalVelocitySmoothRef = 0f;

    // Input đọc ở Update, áp lực ở FixedUpdate
    bool inputForward, inputBackward, inputLeft, inputRight, inputUp, inputDown, inputFire;

    void Start()
    {
        subRigidbody = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        subRigidbody.useGravity = false;      // tàu ngầm nổi trong nước, không rơi theo gravity thường
        subRigidbody.linearDamping = waterDrag;
        subRigidbody.angularDamping = waterDrag * 2f;

        // Mặc định tắt điều khiển, chờ RaceMiniGame gọi SetGameActive(true)
        SetGameActive(false);
    }

    // ====== ĐƯỢC GỌI TỪ RaceMiniGame ======
    public void SetGameActive(bool active)
    {
        gameActive = active;

        if (!active)
        {
            if (subRigidbody != null && !subRigidbody.isKinematic)
            {
                subRigidbody.linearVelocity = Vector3.zero;
                subRigidbody.angularVelocity = Vector3.zero;
            }
            inputForward = inputBackward = inputLeft = inputRight = inputUp = inputDown = false;
        }
    }

    void Update()
    {
        currentSpeed = Vector3.Dot(subRigidbody.linearVelocity, transform.forward);

        UpdateAnimation();
        UpdateStunTimer();

        if (!gameActive) return;

        if (isStunned)
        {
            // Đang bị stun: không đọc input di chuyển/bắn, tàu trôi tự do theo quán tính + waterDrag
            inputForward = inputBackward = inputLeft = inputRight = inputUp = inputDown = inputFire = false;
            return;
        }

        ReadInput();
        HandleFireInput();
    }

    void UpdateStunTimer()
    {
        if (!isStunned) return;

        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            isStunned = false;
        }
    }

    // Gọi từ TorpedoProjectile khi bị bắn trúng
    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = Mathf.Max(stunTimer, duration); // không cộng dồn nếu bị bắn liên tiếp, chỉ lấy thời gian dài hơn
    }

    // ====== ANIMATION ======
    void UpdateAnimation()
    {
        if (animator == null) return;

        float speedAbs = Mathf.Abs(currentSpeed);

        bool shouldRun = speedAbs >= runSpeedThreshold;
        bool shouldWalk = !shouldRun && speedAbs >= walkSpeedThreshold;

        animator.SetBool("isRunning", shouldRun);
        animator.SetBool("isWalking", shouldWalk);
        // isBurrowed không dùng cho tàu ngầm - model chỉ có Idle/Walk/Run
    }

    void FixedUpdate()
    {
        if (!gameActive) return;
        if (isStunned) return; // đang bị stun - không nhận lực điều khiển, chỉ trôi theo waterDrag
        ApplyPhysics();
    }

    // ====== ĐỌC INPUT ======
    void ReadInput()
    {
        if (playerId == PlayerID.Player1)
        {
            inputForward = Input.GetKey(KeyCode.W);
            inputBackward = Input.GetKey(KeyCode.S);
            inputLeft = Input.GetKey(KeyCode.A);
            inputRight = Input.GetKey(KeyCode.D);
            inputUp = Input.GetKey(KeyCode.Space);       // nổi lên
            inputDown = Input.GetKey(KeyCode.LeftShift); // lặn xuống
            inputFire = Input.GetKeyDown(KeyCode.J);
        }
        else // Player2
        {
            inputForward = Input.GetKey(KeyCode.UpArrow);
            inputBackward = Input.GetKey(KeyCode.DownArrow);
            inputLeft = Input.GetKey(KeyCode.LeftArrow);
            inputRight = Input.GetKey(KeyCode.RightArrow);
            inputUp = Input.GetKey(KeyCode.RightShift);      // nổi lên
            inputDown = Input.GetKey(KeyCode.RightControl);  // lặn xuống
            inputFire = Input.GetKeyDown(KeyCode.Keypad1);
        }
    }

    // ====== BẮN NGƯ LÔI ======
    void HandleFireInput()
    {
        if (!inputFire) return;
        if (Time.time - lastFireTime < fireCooldown) return;
        if (torpedoPrefab == null) return;

        Transform firePoint = fireFromLeft ? torpedoPointLeft : torpedoPointRight;
        if (firePoint == null) firePoint = torpedoPointLeft != null ? torpedoPointLeft : torpedoPointRight;
        if (firePoint == null) return; // chưa gán điểm bắn nào cả

        fireFromLeft = !fireFromLeft; // luân phiên bên cho lần bắn sau
        lastFireTime = Time.time;

        GameObject torpedo = Instantiate(torpedoPrefab, firePoint.position, transform.rotation);
        TorpedoProjectile projectile = torpedo.GetComponent<TorpedoProjectile>();
        if (projectile != null)
            projectile.Launch(transform.forward, this);

        // Đã bỏ animator.Play("Shoot") - animation này tự mô phỏng sẵn 1 quả ngư lôi
        // gắn trên model (Torpedo_L/R), bắn ra sẽ bị trùng với viên đạn code tự spawn ở trên.
    }

    // ====== ÁP DỤNG VẬT LÝ ======
    void ApplyPhysics()
    {
        // Tiến / lùi - đẩy theo hướng forward của tàu
        if (inputForward && currentSpeed < maxSpeed)
        {
            subRigidbody.AddForce(transform.forward * thrustForce, ForceMode.Acceleration);
        }
        if (inputBackward && currentSpeed > -maxReverseSpeed)
        {
            subRigidbody.AddForce(-transform.forward * thrustForce * 0.6f, ForceMode.Acceleration);
        }

        // Xoay trái / phải quanh trục Y - chỉ xoay được khi có chút tốc độ, giống tàu thật
        // Dùng MoveRotation (không phải transform.Rotate) để không bị giật khi Rigidbody có Interpolation
        float turnFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / 2f);
        float turnInput = 0f;
        if (inputLeft) turnInput -= 1f;
        if (inputRight) turnInput += 1f;

        if (turnInput != 0f)
        {
            float turnAngleThisStep = turnInput * turnSpeed * turnFactor * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, turnAngleThisStep, 0f);
            subRigidbody.MoveRotation(subRigidbody.rotation * deltaRotation);
        }

        // Nổi lên / lặn xuống - dùng SmoothDamp để tăng/giảm lực mượt dần, không bật/tắt đột ngột
        // như AddForce thô (nguyên nhân chính gây giật khi chạm biên minDepthY/maxDepthY)
        float targetVerticalSpeed = 0f;
        if (inputUp) targetVerticalSpeed = maxVerticalSpeed;
        else if (inputDown) targetVerticalSpeed = -maxVerticalSpeed;

        Vector3 currentVel = subRigidbody.linearVelocity;
        float smoothedVerticalSpeed = Mathf.SmoothDamp(
            currentVel.y,
            targetVerticalSpeed,
            ref verticalVelocitySmoothRef,
            0.2f // thời gian làm mượt (giây) - tăng lên nếu vẫn còn giật, giảm xuống nếu thấy lì/chậm phản hồi
        );

        // Không cho vượt biên ngay tại đây - làm mượt dần về 0 khi gần biên thay vì cắt cứng
        if (subRigidbody.position.y >= maxDepthY && smoothedVerticalSpeed > 0f)
            smoothedVerticalSpeed = 0f;
        if (subRigidbody.position.y <= minDepthY && smoothedVerticalSpeed < 0f)
            smoothedVerticalSpeed = 0f;

        currentVel.y = smoothedVerticalSpeed;
        subRigidbody.linearVelocity = currentVel;

        // Giới hạn độ sâu giờ đã xử lý ngay trong ApplyPhysics() (phần SmoothDamp trục dọc)
        // để tránh 2 hệ thống cùng chỉnh vận tốc Y ở 2 nơi khác nhau trong cùng 1 FixedUpdate.

        // ====== HÚC NHAU GIỮA 2 TÀU ======
        void OnCollisionEnter(Collision collision)
        {
            if (!gameActive) return;

            PlayerSubmarineController otherSub = collision.gameObject.GetComponent<PlayerSubmarineController>();
            if (otherSub == null) return;

            Vector3 pushDir = collision.transform.position - transform.position;
            pushDir.Normalize(); // giữ nguyên cả trục Y - va chạm tàu ngầm có thể đẩy lệch cả chiều sâu

            float impactRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            otherSub.GetComponent<Rigidbody>().AddForce(pushDir * pushForce * impactRatio, ForceMode.VelocityChange);
        }
    }
}