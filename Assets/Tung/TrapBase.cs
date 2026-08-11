using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public abstract class TrapBase : MonoBehaviour
{
    [Header("--- TRAP CHUNG ---")]
    [Tooltip("Thời gian hồi chung của bẫy. Kích hoạt xong thì trong khoảng thời gian này KHÔNG AI bị dẫm nữa.")]
    public float cooldown = 3f;

    protected bool isActive = true;

    // Cooldown chung cho toàn bộ bẫy (không phân biệt P1/P2)
    private float lastTriggerTime = -999f;

    // FIX: theo dõi player nào đang ở trong trigger để không bị trigger liên tục
    // qua OnTriggerStay nếu player đứng yên tại chỗ (ví dụ do bị đóng băng ngay trên bẫy).
    // Chỉ cho phép trigger lại sau khi player thực sự rời khỏi rồi vào lại (và hết cooldown).
    private readonly HashSet<BombCarrier> insideTrap = new HashSet<BombCarrier>();

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

    // FIX: bỏ OnTriggerStay để tránh trigger liên tục mỗi frame khi player đứng yên
    // trong vùng bẫy (ví dụ bị đóng băng ngay trên bẫy -> hết cooldown -> bị trigger lại
    // ngay lập tức mà không cần rời khỏi trigger).

    private void OnTriggerExit(Collider other)
    {
        BombCarrier carrier = GetCarrier(other);
        if (carrier != null)
        {
            insideTrap.Remove(carrier);
        }
    }

    private BombCarrier GetCarrier(Collider other)
    {
        BombCarrier carrier = other.GetComponent<BombCarrier>();
        if (carrier == null) carrier = other.GetComponentInParent<BombCarrier>();
        return carrier;
    }

    private void CheckAndTrigger(Collider other)
    {
        if (!isActive) return;

        // 1. Kiểm tra Cooldown chung (Nếu bẫy vừa bị dẫm cách đây chưa đủ 'cooldown' giây -> Bỏ qua mọi Player khác)
        if (Time.time - lastTriggerTime < cooldown) return;

        // 2. Tìm BombCarrier
        BombCarrier carrier = GetCarrier(other);

        if (carrier == null) return;
        if (!carrier.IsGameActive() || carrier.IsEliminated()) return;

        // 3. Check Tag
        string targetTag = carrier.gameObject.tag;
        if (targetTag != "Player 1" && targetTag != "Player 2") return;

        // 4. Nếu player này đang được ghi nhận là "còn ở trong bẫy" (chưa OnTriggerExit)
        // thì không trigger lại, tránh spam khi player đứng yên tại chỗ.
        if (insideTrap.Contains(carrier)) return;

        // 5. Đánh dấu thời gian dẫm bẫy MỚI NHẤT + đánh dấu player đang ở trong bẫy
        lastTriggerTime = Time.time;
        insideTrap.Add(carrier);

        // 6. Thực thi logic bẫy
        OnPlayerHit(carrier);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // Khi tắt bẫy, xóa luôn trạng thái "đang ở trong bẫy" để tránh giữ state cũ
        // khi bẫy được bật lại ở round sau.
        if (!active)
        {
            insideTrap.Clear();
        }
    }

    public bool IsActive() => isActive;

    protected abstract void OnPlayerHit(BombCarrier carrier);
}