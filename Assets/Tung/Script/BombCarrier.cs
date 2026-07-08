using UnityEngine;

[RequireComponent(typeof(PlayerType))]
public class BombCarrier : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Transform bombAnchor; // Điểm rỗng trên đầu nhân vật

    private PlayerType playerType;
    private PlayerMove playerMove;

    private bool isGameActive = false;
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
        if (!isGameActive) return;
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    // Gọi từ BombGameManager.StartMiniGame() / StopMiniGame()
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        isHoldingBomb = false;
        isEliminated = false;
        cooldownTimer = 0f;
    }

    public bool IsGameActive() => isGameActive;

    // Va chạm - detect qua Tag "Player 1" / "Player 2"
    void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;
        if (isEliminated || !isHoldingBomb) return;
        if (IsOnCooldown()) return;

        if (!other.CompareTag("Player 1") && !other.CompareTag("Player 2")) return;

        BombCarrier otherCarrier = other.GetComponent<BombCarrier>();
        if (otherCarrier == null || otherCarrier == this) return;
        if (!otherCarrier.IsGameActive() || otherCarrier.IsEliminated()) return;

        BombGameManager.Instance?.TransferBomb(this, otherCarrier);
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
            playerMove.isMove = !eliminated;
            playerMove.isJump = !eliminated;
        }
    }

    public bool IsEliminated() => isEliminated;
}