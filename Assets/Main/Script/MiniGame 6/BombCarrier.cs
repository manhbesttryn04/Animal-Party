using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerType))]
public class BombCarrier : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Transform bombAnchor;

    // FIX: cooldown âm thanh bẫy dùng CHUNG cho toàn bộ game (static), không phải riêng
    // từng bẫy. Vì mỗi bẫy có cooldown riêng, khi player đi qua vùng nhiều bẫy đặt gần nhau,
    // mỗi bẫy khác nhau vẫn tự phát âm thanh của nó -> âm thanh dồn chồng liên tục không dứt.
    [Header("--- TRAP SFX THROTTLE (Global) ---")]
    [Tooltip("Khoảng thời gian tối thiểu giữa 2 lần phát âm thanh bẫy, tính chung cho MỌI bẫy/MỌI player.")]
    public static float trapSfxMinInterval = 0.35f;
    private static float lastTrapSfxTime = -999f;

    private static bool CanPlayTrapSfx()
    {
        if (Time.time - lastTrapSfxTime < trapSfxMinInterval) return false;
        lastTrapSfxTime = Time.time;
        return true;
    }

    private PlayerType playerType;
    private PlayerMove playerMove;

    private bool isGameActive = false;
    private bool isHoldingBomb = false;
    private bool isEliminated = false;
    private bool canMove = false;
    private float cooldownTimer = 0f;

    // --- FREEZE ---
    private bool isFrozen = false;
    private float defaultSpeed = 0f;
    private Coroutine freezeCoroutine;
    private GameObject activeFreezeLoopVFX; // FIX: theo dõi VFX loop đang chạy để destroy thủ công khi bị dính bẫy lại

    // --- MAGNET ---
    private Coroutine magnetCoroutine;
    private GameObject activeMagnetVFX; // FIX: theo dõi VFX magnet đang chạy để destroy thủ công khi bị dính bẫy lại
    private bool isBeingPulled = false; // FIX: cờ trạng thái để chặn phát lại âm thanh/VFX khi đang bị hút

    void Awake()
    {
        playerType = GetComponent<PlayerType>();
        playerMove = GetComponent<PlayerMove>();

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

        UpdatePlayerMovementState();
    }

    private void UpdatePlayerMovementState()
    {
        if (playerMove == null) return;

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

    public void SetCanMove(bool enable)
    {
        canMove = enable;
        UpdatePlayerMovementState();
    }

    public bool CanMove() => canMove;

    /// <summary>
    /// Bật/tắt trạng thái active của carrier trong game.
    /// KHÔNG reset isEliminated ở đây — dùng ResetForNewGame() khi bắt đầu 1 game/round mới
    /// để tránh việc gọi SetGameActive(false) sau khi loại player làm mất cờ isEliminated.
    /// </summary>
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        isHoldingBomb = false;
        canMove = false;

        if (isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            if (activeFreezeLoopVFX != null) { Destroy(activeFreezeLoopVFX); activeFreezeLoopVFX = null; }
            SetFrozen(false);
        }

        if (magnetCoroutine != null) StopCoroutine(magnetCoroutine);
        if (activeMagnetVFX != null) { Destroy(activeMagnetVFX); activeMagnetVFX = null; }
        isBeingPulled = false;

        cooldownTimer = 0f;
    }

    /// <summary>
    /// Gọi khi bắt đầu 1 trận mới (trước SetGameActive(true)) để reset toàn bộ trạng thái,
    /// bao gồm cả isEliminated.
    /// </summary>
    public void ResetForNewGame()
    {
        isEliminated = false;
        isHoldingBomb = false;
        canMove = false;
        cooldownTimer = 0f;
    }

    public bool IsGameActive() => isGameActive;

    void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;
        if (isEliminated || !isHoldingBomb) return;
        if (isFrozen) return;
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
            if (activeFreezeLoopVFX != null) { Destroy(activeFreezeLoopVFX); activeFreezeLoopVFX = null; }
            SetFrozen(false);
        }
    }

    public bool IsEliminated() => isEliminated;

    // ====== FREEZE TRAP SYSTEM ======
    public void ApplyFreeze(float duration, GameObject hitVfxPrefab, GameObject loopVfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        // FIX: chặn tại nguồn — nếu đang đóng băng rồi thì không phát lại âm thanh/VFX/coroutine.
        // Trước đây chỉ FreezeTrap tự check IsFrozen() trước khi gọi, nên nếu có nơi khác gọi
        // trực tiếp (hoặc logic trap thay đổi) thì âm thanh/VFX vẫn có thể bị chồng.
        if (isFrozen) return;

        // PHÁT ÂM THANH BẪY BĂNG TỪ AUDIOMANAGER
        // FIX: chỉ phát nếu chưa có bẫy nào khác vừa phát âm thanh gần đây (global throttle),
        // tránh dồn chồng âm thanh khi đi qua vùng nhiều bẫy đặt gần nhau.
        if (CanPlayTrapSfx() && AudioManager.Instance != null)
        {
            AudioClip clip = AudioManager.Instance.freezeTrapClip != null ? AudioManager.Instance.freezeTrapClip : AudioManager.Instance.iceMagicClip;
            AudioManager.Instance.PlaySFX(clip);
        }

        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        // FIX: StopCoroutine chỉ ngắt coroutine ngay tại điểm yield, KHÔNG chạy phần code
        // dọn dẹp (Destroy loopVFX) nằm sau yield return trong FreezeRoutine cũ.
        // Nếu không destroy thủ công ở đây, VFX loop cũ sẽ bị bỏ quên và chồng lên VFX mới.
        if (activeFreezeLoopVFX != null)
        {
            Destroy(activeFreezeLoopVFX);
            activeFreezeLoopVFX = null;
        }

        if (vfxScale == Vector3.zero) vfxScale = Vector3.one;

        freezeCoroutine = StartCoroutine(FreezeRoutine(duration, hitVfxPrefab, loopVfxPrefab, vfxOffset, vfxScale));
    }

    private IEnumerator FreezeRoutine(float duration, GameObject hitVfxPrefab, GameObject loopVfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        SetFrozen(true);

        if (hitVfxPrefab != null)
        {
            GameObject hitVFX = Instantiate(hitVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            hitVFX.transform.localScale = vfxScale;
            Destroy(hitVFX, 2f);
        }

        GameObject loopVFX = null;
        if (loopVfxPrefab != null)
        {
            loopVFX = Instantiate(loopVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            loopVFX.transform.localScale = vfxScale;
        }

        // FIX: lưu tham chiếu ra field để ApplyFreeze() có thể destroy thủ công
        // nếu coroutine này bị Stop giữa đường (không kịp chạy tới đoạn Destroy dưới đây).
        activeFreezeLoopVFX = loopVFX;

        yield return new WaitForSeconds(duration);

        if (loopVFX != null)
        {
            Destroy(loopVFX);
        }

        activeFreezeLoopVFX = null;
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

    // ====== MAGNET TRAP SYSTEM ======
    public void ApplyPulledByPlayer(Transform pullerTransform, float force, float duration, GameObject vfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        // FIX: chặn tại nguồn — nếu đang bị hút rồi thì không phát lại âm thanh/VFX/coroutine.
        // Trước đây MagnetTrap không hề check trạng thái này, nên nếu bị 2 bẫy nam châm
        // khác nhau hút gần nhau về thời gian, âm thanh + VFX sẽ bị chồng lên nhau.
        if (isBeingPulled) return;

        // PHÁT ÂM THANH BẪY NAM CHÂM TỪ AUDIOMANAGER
        // FIX: dùng chung global throttle với Freeze để tránh dồn chồng âm thanh.
        if (CanPlayTrapSfx() && AudioManager.Instance != null)
        {
            AudioClip clip = AudioManager.Instance.magnetTrapClip != null ? AudioManager.Instance.magnetTrapClip : AudioManager.Instance.laserMoveClip;
            AudioManager.Instance.PlaySFX(clip);
        }

        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
        }

        // FIX: tương tự Freeze — destroy thủ công VFX magnet cũ vì StopCoroutine
        // không chạy phần dọn dẹp nằm sau while loop trong PulledRoutine cũ.
        if (activeMagnetVFX != null)
        {
            Destroy(activeMagnetVFX);
            activeMagnetVFX = null;
        }

        if (vfxScale == Vector3.zero) vfxScale = Vector3.one;

        isBeingPulled = true;
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

        // FIX: lưu tham chiếu ra field để ApplyPulledByPlayer() có thể destroy thủ công
        // nếu coroutine này bị Stop giữa đường.
        activeMagnetVFX = spawnedVFX;

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

        activeMagnetVFX = null;
        isBeingPulled = false;
        magnetCoroutine = null;
    }
}