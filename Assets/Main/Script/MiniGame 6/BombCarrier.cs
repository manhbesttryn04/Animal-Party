using UnityEngine;
using System.Collections;

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
    private bool canMove = false; // Mặc định chưa đếm ngược xong -> ĐÉO CHO DI CHUYỂN
    private float cooldownTimer = 0f;

    // --- FREEZE ---
    private bool isFrozen = false;
    private float defaultSpeed = 0f; // Lưu tốc độ gốc cố định
    private Coroutine freezeCoroutine;

    // --- MAGNET ---
    private Coroutine magnetCoroutine;

    void Awake()
    {
        playerType = GetComponent<PlayerType>();
        playerMove = GetComponent<PlayerMove>();

        // Lưu tốc độ gốc ngay từ đầu game để tránh lỗi trùng speed = 0
        if (playerMove != null)
        {
            defaultSpeed = playerMove.speed;
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Cập nhật liên tục trạng thái di chuyển cho PlayerMove
        UpdatePlayerMovementState();
    }

    // Quyết định Player có được di chuyển/nhảy hay không
    private void UpdatePlayerMovementState()
    {
        if (playerMove == null) return;

        // Nếu đã bị loại, chưa cho phép di chuyển (đếm ngược), hoặc đang bị Freeze
        if (isEliminated || !canMove || isFrozen)
        {
            playerMove.isMove = false;
            playerMove.isJump = false;
        }
        else
        {
            playerMove.isMove = true;
            playerMove.isJump = true;
        }
    }

    // Gọi từ MiniGame6.cs để bật/tắt quyền di chuyển (Dùng lúc đếm ngược 3, 2, 1)
    public void SetCanMove(bool enable)
    {
        canMove = enable;
        UpdatePlayerMovementState();
    }

    public bool CanMove() => canMove;

    // Gọi từ BombGameManager.StartMiniGame() / StopMiniGame()
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        isHoldingBomb = false;
        isEliminated = false;
        canMove = false; // Mặc định khóa di chuyển khi mới kích hoạt game

        // Reset trạng thái đóng băng nếu ngắt game giữa chừng
        if (isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            SetFrozen(false);
        }

        if (magnetCoroutine != null) StopCoroutine(magnetCoroutine);

        cooldownTimer = 0f;
    }

    public bool IsGameActive() => isGameActive;

    // Va chạm - detect qua Tag "Player 1" / "Player 2"
    void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;
        if (isEliminated || !isHoldingBomb) return;
        if (isFrozen) return; // đang bị đóng băng thì không truyền được bom
        if (IsOnCooldown()) return;

        if (!other.CompareTag("Player 1") && !other.CompareTag("Player 2")) return;

        BombCarrier otherCarrier = other.GetComponent<BombCarrier>();
        if (otherCarrier == null || otherCarrier == this) return;
        if (!otherCarrier.IsGameActive() || otherCarrier.IsEliminated()) return;

        MiniGame6.Instance?.TransferBomb(this, otherCarrier);
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
        UpdatePlayerMovementState();

        if (eliminated && isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            SetFrozen(false);
        }
    }

    public bool IsEliminated() => isEliminated;

    // ====== FREEZE TRAP SYSTEM ======
    public void ApplyFreeze(float duration, GameObject hitVfxPrefab, GameObject loopVfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        if (vfxScale == Vector3.zero) vfxScale = Vector3.one;

        freezeCoroutine = StartCoroutine(FreezeRoutine(duration, hitVfxPrefab, loopVfxPrefab, vfxOffset, vfxScale));
    }

    private IEnumerator FreezeRoutine(float duration, GameObject hitVfxPrefab, GameObject loopVfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        SetFrozen(true);

        // 1. Spawn VFX Va chạm
        if (hitVfxPrefab != null)
        {
            GameObject hitVFX = Instantiate(hitVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            hitVFX.transform.localScale = vfxScale;
            Destroy(hitVFX, 2f);
        }

        // 2. Spawn VFX Duy trì
        GameObject loopVFX = null;
        if (loopVfxPrefab != null)
        {
            loopVFX = Instantiate(loopVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            loopVFX.transform.localScale = vfxScale;
        }

        yield return new WaitForSeconds(duration);

        if (loopVFX != null)
        {
            Destroy(loopVFX);
        }

        SetFrozen(false);
        freezeCoroutine = null;
    }

    public void SetFrozen(bool frozen)
    {
        if (isEliminated) return;

        isFrozen = frozen;
        if (playerMove == null) return;

        if (frozen)
        {
            playerMove.speed = 0f;
        }
        else
        {
            playerMove.speed = defaultSpeed;
        }

        UpdatePlayerMovementState();
    }

    public bool IsFrozen() => isFrozen;

    // ====== MAGNET TRAP SYSTEM (HÚT VỀ PHÍA PLAYER KHÁC) ======
    public void ApplyPulledByPlayer(Transform pullerTransform, float force, float duration, GameObject vfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
        }

        if (vfxScale == Vector3.zero) vfxScale = Vector3.one;

        magnetCoroutine = StartCoroutine(PulledRoutine(pullerTransform, force, duration, vfxPrefab, vfxOffset, vfxScale));
    }

    private IEnumerator PulledRoutine(Transform pullerTransform, float force, float duration, GameObject vfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        GameObject spawnedVFX = null;
        if (vfxPrefab != null && pullerTransform != null)
        {
            spawnedVFX = Instantiate(vfxPrefab, pullerTransform.position + vfxOffset, Quaternion.identity, pullerTransform);
            spawnedVFX.transform.localScale = vfxScale;
        }

        CharacterController cc = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        float timer = 0f;

        while (timer < duration)
        {
            if (isEliminated || pullerTransform == null) break;

            Vector3 directionToPuller = (pullerTransform.position - transform.position);
            directionToPuller.y = 0;

            if (directionToPuller.magnitude > 1.0f)
            {
                Vector3 pullVelocity = directionToPuller.normalized * force;

                if (cc != null && cc.enabled)
                {
                    cc.Move(pullVelocity * Time.deltaTime);
                }
                else if (rb != null && !rb.isKinematic)
                {
                    rb.AddForce(pullVelocity, ForceMode.Acceleration);
                }
                else
                {
                    transform.position += pullVelocity * Time.deltaTime;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (spawnedVFX != null)
        {
            Destroy(spawnedVFX);
        }

        magnetCoroutine = null;
    }
}