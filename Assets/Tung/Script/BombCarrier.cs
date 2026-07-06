using UnityEngine;

// Gắn script này lên MỖI Player, chung với PlayerType đã có
[RequireComponent(typeof(PlayerType))]
public class BombCarrier : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Transform bombAnchor;   // Điểm rỗng đặt trên đầu nhân vật - bomb prefab sẽ dính vào đây khi cầm

    private PlayerType playerType;
    private PlayerMove playerMove;

    private bool isHoldingBomb = false;
    private bool isEliminated = false;
    private float cooldownTimer = 0f;

    void Awake()
    {
        playerType = GetComponent<PlayerType>();
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    // Va chạm - chỉ nhận diện đối tượng có tag "Player 1" hoặc "Player 2"
    void OnTriggerEnter(Collider other)
    {
        if (isEliminated || !isHoldingBomb) return;

        // Vừa nhận bom xong thì phải giữ ít nhất passCooldown giây mới được truyền tiếp,
        // tránh trường hợp đứng sát nhau bị trả bom qua lại ngay lập tức
        if (IsOnCooldown()) return;

        if (!other.CompareTag("Player 1") && !other.CompareTag("Player 2"))
            return;

        BombCarrier otherCarrier = other.GetComponent<BombCarrier>();
        if (otherCarrier == null || otherCarrier == this || otherCarrier.isEliminated)
            return;

        BombGameManager.Instance.TransferBomb(this, otherCarrier);
    }

    public void SetHoldingBomb(bool holding)
    {
        isHoldingBomb = holding;
    }

    public bool IsHoldingBomb() => isHoldingBomb;

    public void StartCooldown(float duration)
    {
        cooldownTimer = duration;
    }

    public bool IsOnCooldown() => cooldownTimer > 0f;

    public void SetEliminated(bool eliminated)
    {
        isEliminated = eliminated;

        if (playerMove != null)
        {
            // Khóa di chuyển/nhảy nhưng KHÔNG tắt object, để animation thua/ngã (nếu có) vẫn chạy được
            playerMove.isMove = !eliminated;
            playerMove.isJump = !eliminated;
        }

        // TODO: nếu muốn chơi animation thua, gọi trigger animator ở đây
    }
}