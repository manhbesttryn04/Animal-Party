using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class TrapBase : MonoBehaviour
{
    [Header("--- TRAP CHUNG ---")]
    [Tooltip("Thời gian hồi chung của bẫy. Kích hoạt xong thì trong khoảng thời gian này KHÔNG AI bị dẫm nữa.")]
    public float cooldown = 3f;

    protected bool isActive = true;

    // Đổi 2 biến P1, P2 thành 1 biến cooldown chung cho toàn bộ bẫy
    private float lastTriggerTime = -999f;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    protected virtual void Awake()
    {
        TrapManager.Instance?.Register(this);
    }

    protected virtual void OnDestroy()
    {
        TrapManager.Instance?.Unregister(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckAndTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckAndTrigger(other);
    }

    private void CheckAndTrigger(Collider other)
    {
        if (!isActive) return;

        // 1. Kiểm tra Cooldown chung (Nếu bẫy vừa bị dẫm cách đây chưa đủ 'cooldown' giây -> Bỏ qua mọi Player khác)
        if (Time.time - lastTriggerTime < cooldown) return;

        // 2. Tìm BombCarrier
        BombCarrier carrier = other.GetComponent<BombCarrier>();
        if (carrier == null) carrier = other.GetComponentInParent<BombCarrier>();

        if (carrier == null) return;
        if (!carrier.IsGameActive() || carrier.IsEliminated()) return;

        // 3. Check Tag
        string targetTag = carrier.gameObject.tag;
        if (targetTag != "Player 1" && targetTag != "Player 2") return;

        // 4. Đánh dấu thời gian dẫm bẫy MỚI NHẤT
        lastTriggerTime = Time.time;

        // 5. Thực thi logic bẫy
        OnPlayerHit(carrier);
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public bool IsActive() => isActive;

    protected abstract void OnPlayerHit(BombCarrier carrier);
}