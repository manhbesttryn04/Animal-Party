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
    private float cooldownTimer = 0f;

    // --- FREEZE ---
    private bool isFrozen = false;
    private float defaultSpeed = 0f; // Lưu tốc độ gốc cố định
    private Coroutine freezeCoroutine;

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
    }

    // Gọi từ BombGameManager.StartMiniGame() / StopMiniGame()
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        isHoldingBomb = false;
        isEliminated = false;

        // Reset trạng thái đóng băng nếu ngắt game giữa chừng
        if (isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            SetFrozen(false);
        }

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

        if (playerMove != null)
        {
            playerMove.isMove = !eliminated;
            playerMove.isJump = !eliminated;
        }

        if (eliminated && isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            SetFrozen(false);
        }
    }

    public bool IsEliminated() => isEliminated;

    // ====== FREEZE TRAP SYSTEM ======

    // Bẫy sẽ gọi hàm này thay vì tự chạy Coroutine
    public void ApplyFreeze(float duration, GameObject vfxPrefab = null, Vector3 vfxOffset = default)
    {
        if (isEliminated) return;

        // Nếu đang đóng băng mà ăn tiếp bẫy thì reset lại đếm ngược
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        freezeCoroutine = StartCoroutine(FreezeRoutine(duration, vfxPrefab, vfxOffset));
    }

    private IEnumerator FreezeRoutine(float duration, GameObject vfxPrefab, Vector3 vfxOffset)
    {
        SetFrozen(true);

        // Spawn VFX làm con của Player
        GameObject spawnedVFX = null;
        if (vfxPrefab != null)
        {
            spawnedVFX = Instantiate(vfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(duration);

        // Hết đóng băng -> Xóa VFX & trả lại di chuyển
        if (spawnedVFX != null)
        {
            Destroy(spawnedVFX);
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
            playerMove.isMove = false;
            playerMove.isJump = false;
            playerMove.speed = 0f;
        }
        else
        {
            playerMove.isMove = true;
            playerMove.isJump = true;
            playerMove.speed = defaultSpeed; // Trả về tốc độ ban đầu
        }
    }

    public bool IsFrozen() => isFrozen;
    // ====== FREEZE TRAP SYSTEM ======
    public void ApplyFreeze(float duration, GameObject hitVfxPrefab, GameObject loopVfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        // Nếu đang bị đóng băng mà dẫm tiếp thì reset đếm ngược
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

        // 1. Spawn VFX Va chạm (Nổ 1 phát rồi tự hủy sau 2s)
        if (hitVfxPrefab != null)
        {
            GameObject hitVFX = Instantiate(hitVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            hitVFX.transform.localScale = vfxScale;
            Destroy(hitVFX, 2f); // Tự hủy VFX hit
        }

        // 2. Spawn VFX Duy trì (Bám theo người, hết đóng băng mới Destroy)
        GameObject loopVFX = null;
        if (loopVfxPrefab != null)
        {
            loopVFX = Instantiate(loopVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            loopVFX.transform.localScale = vfxScale;
        }

        // Chờ hết thời gian đóng băng
        yield return new WaitForSeconds(duration);

        // 3. Hết đóng băng -> Xóa VFX duy trì & thả Player ra
        if (loopVFX != null)
        {
            Destroy(loopVFX);
        }

        SetFrozen(false);
        freezeCoroutine = null;
    }
    // ====== MAGNET TRAP SYSTEM ======
    private Coroutine magnetCoroutine;

    public void ApplyMagnet(Vector3 trapPosition, float force, float duration, GameObject vfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        if (isEliminated) return;

        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
        }

        if (vfxScale == Vector3.zero) vfxScale = Vector3.one;

        magnetCoroutine = StartCoroutine(MagnetRoutine(trapPosition, force, duration, vfxPrefab, vfxOffset, vfxScale));
    }

    private IEnumerator MagnetRoutine(Vector3 trapPosition, float force, float duration, GameObject vfxPrefab, Vector3 vfxOffset, Vector3 vfxScale)
    {
        // Spawn VFX xoáy/nam châm tại vị trí bẫy
        GameObject spawnedVFX = null;
        if (vfxPrefab != null)
        {
            spawnedVFX = Instantiate(vfxPrefab, trapPosition + vfxOffset, Quaternion.identity);
            spawnedVFX.transform.localScale = vfxScale;
        }

        CharacterController cc = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        float timer = 0f;

        while (timer < duration)
        {
            if (isEliminated) break;

            // Tính hướng từ Player về tâm Bẫy
            Vector3 directionToTrap = (trapPosition - transform.position);
            directionToTrap.y = 0; // Chỉ hút theo mặt phẳng ngang, không hút chìm xuống đất

            // Nếu còn ở xa tâm bẫy thì tiếp tục hút
            if (directionToTrap.magnitude > 0.2f)
            {
                Vector3 pullVelocity = directionToTrap.normalized * force;

                // Nếu xài CharacterController
                if (cc != null && cc.enabled)
                {
                    cc.Move(pullVelocity * Time.deltaTime);
                }
                // Nếu xài Rigidbody
                else if (rb != null && !rb.isKinematic)
                {
                    rb.AddForce(pullVelocity, ForceMode.Acceleration);
                }
                // Nếu dời Transform thủ công
                else
                {
                    transform.position += pullVelocity * Time.deltaTime;
                }
            }

            timer += Time.deltaTime;
            yield return null; // Chờ frame tiếp theo
        }

        // Hết thời gian hút -> Xóa VFX
        if (spawnedVFX != null)
        {
            Destroy(spawnedVFX);
        }

        magnetCoroutine = null;
    }
}