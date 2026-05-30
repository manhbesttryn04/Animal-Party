using UnityEngine;
using TMPro;
using System.Collections;

public class CountDownManager : MonoBehaviour
{
    [Header("UI Component")]
    public TextMeshProUGUI textDemNguoc;

    [Header("Targets to Control")]
    public BoatController thuyen1;
    public BoatController thuyen2;

    private bool dangTrongThoiGianDemNguoc = false;

    // Biến lưu vị trí gốc để khóa chết tọa độ
    private Vector3 viTriGocThuyen1;
    private Vector3 viTriGocThuyen2;

    void Start()
    {
        if (textDemNguoc == null || thuyen1 == null || thuyen2 == null)
        {
            Debug.LogError("<color=red>[COUNTDOWN] Thiếu kéo thả kìa bạn ơi!</color>");
            return;
        }

        // 1. Lưu ngay vị trí chính xác của 2 thuyền tại frame đầu tiên vào game
        viTriGocThuyen1 = thuyen1.transform.position;
        viTriGocThuyen2 = thuyen2.transform.position;

        // 2. Chạy Coroutine đếm ngược thời gian
        StartCoroutine(ChayTrinhDemNguocCoroutine());
    }

    void Update()
    {
        // THUẬT TOÁN KHÓA CỨNG: Nếu đang đếm ngược, ép chết tọa độ về vị trí gốc ban đầu
        // Bất kể script khác hay hệ thống vật lý có đẩy thuyền đi, nó cũng bị kéo giật lại vị trí cũ ở mỗi khung hình
        if (dangTrongThoiGianDemNguoc)
        {
            if (thuyen1 != null) thuyen1.transform.position = viTriGocThuyen1;
            if (thuyen2 != null) thuyen2.transform.position = viTriGocThuyen2;
        }
    }

    IEnumerator ChayTrinhDemNguocCoroutine()
    {
        // BẬT KHÓA CỨNG TOÀN DIỆN
        dangTrongThoiGianDemNguoc = true;
        thuyen1.isDead = true;
        thuyen2.isDead = true;

        Rigidbody rb1 = thuyen1.GetComponent<Rigidbody>();
        Rigidbody rb2 = thuyen2.GetComponent<Rigidbody>();

        if (rb1 != null)
        {
            rb1.isKinematic = true;
            rb1.linearVelocity = Vector3.zero;
            rb1.angularVelocity = Vector3.zero;
        }
        if (rb2 != null)
        {
            rb2.isKinematic = true;
            rb2.linearVelocity = Vector3.zero;
            rb2.angularVelocity = Vector3.zero;
        }

        // TIẾN HÀNH ĐẾM NGƯỢC
        textDemNguoc.text = "3";
        yield return new WaitForSeconds(1f);

        textDemNguoc.text = "2";
        yield return new WaitForSeconds(1f);

        textDemNguoc.text = "1";
        yield return new WaitForSeconds(1f);

        // XUẤT PHÁT - MỞ KHÓA TOÀN BỘ
        textDemNguoc.text = "START!";

        // Tắt chế độ ép chết tọa độ trong Update(), cho phép thuyền tự do di chuyển
        dangTrongThoiGianDemNguoc = false;

        if (rb1 != null) rb1.isKinematic = false;
        if (rb2 != null) rb2.isKinematic = false;

        thuyen1.isDead = false;
        thuyen2.isDead = false;

        Debug.Log("<color=green>[COUNTDOWN] KHÓA VỊ TRÍ ĐÃ TẮT! XUẤT PHÁT!</color>");

        yield return new WaitForSeconds(1f);
        textDemNguoc.text = "";
    }
}