using System.Collections;
using UnityEngine;

public class BombCarrier : MonoBehaviour
{
    [Header("--- BOM VISUAL ---")]
    [Tooltip("Điểm gắn bom trên đầu nhân vật")]
    public Transform bombAnchor;

    [Header("--- COOLDOWN ---")]
    private float cooldownTimer = 0f;
    private bool isHoldingBomb = false;
    private bool isEliminated = false;

    // ====== BẬT/TẮT CARRIER ======
    // Chỉ hoạt động khi miniGame đang chạy
    private bool isActive = false;

    public void Activate()
    {
        isActive = true;
        isEliminated = false;
        cooldownTimer = 0f;
    }

    public void Deactivate()
    {
        isActive = false;
        SetHoldingBomb(false);
    }

    public bool IsActive => isActive;

    // ====== BOM ======
    public void SetHoldingBomb(bool value)
    {
        isHoldingBomb = value;
    }

    public bool IsHoldingBomb => isHoldingBomb;

    // ====== COOLDOWN (miễn nhiễm sau khi vừa nhận bom) ======
    public void StartCooldown(float duration)
    {
        cooldownTimer = duration;
        StartCoroutine(CooldownRoutine());
    }

    public bool IsOnCooldown() => cooldownTimer > 0f;

    IEnumerator CooldownRoutine()
    {
        while (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        cooldownTimer = 0f;
    }

    // ====== LOẠI ======
    public void SetEliminated(bool value)
    {
        isEliminated = value;
        if (value) Deactivate();
    }

    public bool IsEliminated => isEliminated;

    // ====== TRIGGER - chạm vào player khác để truyền bom ======
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!isHoldingBomb) return;

        BombCarrier other_carrier = other.GetComponent<BombCarrier>();
        if (other_carrier == null) return;
        if (!other_carrier.IsActive) return;
        if (other_carrier.IsEliminated) return;
        if (other_carrier.IsOnCooldown()) return;
        if (other_carrier.IsHoldingBomb) return;

        BombGameManager.Instance?.TransferBomb(this, other_carrier);
    }
}