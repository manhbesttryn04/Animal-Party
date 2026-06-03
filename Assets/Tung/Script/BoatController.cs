using UnityEngine;
using TMPro; // Bắt buộc phải có để điều khiển UI TextMeshPro
using System.Collections;

public class BoatController : MonoBehaviour
{
    [Header("Boat Status (System 100 HP)")]
    public bool isDead = false;
    public float maxHP = 100f; // Máu tối đa 100
    public float currentHP;

    // Đổi biến này thành static để TẤT CẢ các thuyền đều dùng chung trạng thái kết thúc game
    public static bool isGameFinished = false;

    [Header("UI Text Component")]
    [Tooltip("Kéo Object chữ ngoài Hierarchy (Health_Text) vào đây")]
    public TextMeshProUGUI healthTextUI;

    [Header("Movement Settings")]
    public float tocDoTien = 10f;
    public float tocDoNe = 15f;
    public float gioiHanTrai = -15f;
    public float gioiHanPhai = 15f;

    public enum ControlType { Boat1_AD, Boat2_Arrows }

    [Header("Input Control")]
    public ControlType controlType = ControlType.Boat1_AD;

    [Header("Height Offset")]
    public float groundOffset = 0.5f;

    [Header("Map Collision Settings")]
    public float satThuongVaChamMap = 20f; // Va chạm vào bờ map trừ 20 máu
    public float lucNayBờ = 8f;

    [Header("Respawn Settings")]
    public float thoiGianChoRespawn = 2.0f;
    public float cooldownNhanSatThuong = 0.5f;

    private float thoiGianChoPhepSatThuongTiep = 0f;
    private bool dangDuocBaoVeAnToan = false;
    private bool dangBiPhatDungIm = false;

    [Header("Visual Animation")]
    public Transform modelThuyenCon;
    public bool daoNguocHuongNghieng = false;
    public float gocNghiengToiDa = 25f;
    public float tocDoNghieng = 12f;

    private Rigidbody rb;
    private float viTriZBanDau;
    private Vector3 viTriXuatPhatBanDau;
    private Quaternion gocXoayMacDinhModelCon;

    void Start()
    {
        Time.timeScale = 1f; // Khởi động lại thời gian chạy bình thường khi vào game
        isDead = false;
        isGameFinished = false; // Reset trạng thái kết thúc game khi chơi lại
        dangBiPhatDungIm = false;
        dangDuocBaoVeAnToan = false;

        currentHP = maxHP; // Khởi tạo đầy 100 máu

        viTriXuatPhatBanDau = transform.position;
        viTriZBanDau = transform.position.z;
        thoiGianChoPhepSatThuongTiep = 0f;

        if (modelThuyenCon != null)
        {
            gocXoayMacDinhModelCon = modelThuyenCon.localRotation;
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        CapNhatUI_SoMau();
    }

    void Update()
    {
        // CHỐT: Nếu đã kết thúc game hoặc chết thì đứng im toàn bộ
        if (isDead || isGameFinished) return;

        float tocDoTienThucTe = dangBiPhatDungIm ? 0f : tocDoTien;
        transform.Translate(Vector3.left * tocDoTienThucTe * Time.deltaTime, Space.World);

        float moveHorizontal = 0f;
        if (controlType == ControlType.Boat1_AD)
        {
            if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f;
            if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f;
        }
        else if (controlType == ControlType.Boat2_Arrows)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) moveHorizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) moveHorizontal = 1f;
        }

        transform.Translate(Vector3.forward * moveHorizontal * tocDoNe * Time.deltaTime, Space.World);

        if (modelThuyenCon != null)
        {
            float heSoDaoChieu = daoNguocHuongNghieng ? -1f : 1f;
            float tinhGocNghieng = moveHorizontal * gocNghiengToiDa * heSoDaoChieu;
            Vector3 trucDocNoiBo = modelThuyenCon.InverseTransformDirection(Vector3.left);
            Quaternion bienDoiNghieng = Quaternion.AngleAxis(tinhGocNghieng, trucDocNoiBo);
            Quaternion targetRotation = gocXoayMacDinhModelCon * bienDoiNghieng;
            modelThuyenCon.localRotation = Quaternion.Lerp(modelThuyenCon.localRotation, targetRotation, Time.deltaTime * tocDoNghieng);
        }

        Vector3 currentPos = transform.position;
        currentPos.z = Mathf.Clamp(currentPos.z, viTriZBanDau + gioiHanTrai, viTriZBanDau + gioiHanPhai);
        transform.position = currentPos;

        SnapToGroundWithTag();
    }

    void SnapToGroundWithTag()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 4f))
        {
            if (hit.collider.CompareTag("Ground") && Mathf.Abs(hit.normal.y) > 0.7f)
            {
                Vector3 newPos = transform.position;
                newPos.y = hit.point.y + groundOffset;
                transform.position = newPos;
            }
        }
    }

    // HÀM XỬ LÝ KHI CHẠM VÀO VẠCH ĐÍCH `FinishLine`
    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có đúng là chạm vào vật thể có tag FinishLine không
        if (other.CompareTag("FinishLine") && !isGameFinished)
        {
            isGameFinished = true; // Đóng băng trạng thái di chuyển của toàn bộ script

            // Dừng mọi lực vật lý lập tức
            if (rb != null) rb.linearVelocity = Vector3.zero;

            // IN RA THÔNG BÁO THẮNG CUỘC TRÊN CONSOLE
            if (controlType == ControlType.Boat1_AD)
            {
                Debug.Log("<color=cyan><b>[KẾT THÚC] PLAYER 1 ĐÃ VỀ ĐÍCH VÀ CHIẾN THẮNG!</b></color>");
            }
            else
            {
                Debug.Log("<color=yellow><b>[KẾT THÚC] PLAYER 2 ĐÃ VỀ ĐÍCH VÀ CHIẾN THẮNG!</b></color>");
            }

            // DỪNG TOÀN BỘ THỜI GIAN TRONG GAME (Đóng băng bẫy gai, thuyền kia, mọi chuyển động)
            Time.timeScale = 0f;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (isDead || dangDuocBaoVeAnToan || isGameFinished) return;

        Obstacle bẫyGai = collision.gameObject.GetComponent<Obstacle>();
        if (bẫyGai != null)
        {
            if (Time.time < thoiGianChoPhepSatThuongTiep) return;
            thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;

            XulyMatMau(20f);
            return;
        }

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Map"))
        {
            ContactPoint contact = collision.contacts[0];
            if (Mathf.Abs(contact.normal.y) < 0.5f)
            {
                if (Time.time < thoiGianChoPhepSatThuongTiep) return;
                thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;

                XulyMatMau(satThuongVaChamMap);

                if (rb != null && currentHP > 0)
                {
                    Vector3 huongNayBật = contact.normal * lucNayBờ;
                    huongNayBật.y = 0;
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(huongNayBật, ForceMode.VelocityChange);
                }
            }
        }
    }

    private void XulyMatMau(float soMauMat)
    {
        currentHP -= soMauMat;
        if (currentHP < 0) currentHP = 0;

        CapNhatUI_SoMau();

        if (currentHP <= 0)
        {
            StartCoroutine(HoiSinhGocCoroutine());
        }
    }

    void CapNhatUI_SoMau()
    {
        if (healthTextUI != null)
        {
            healthTextUI.text = currentHP.ToString();
        }
    }

    IEnumerator HoiSinhGocCoroutine()
    {
        isDead = true;
        dangBiPhatDungIm = true;
        dangDuocBaoVeAnToan = true;
        currentHP = 0;
        CapNhatUI_SoMau();
        ResetVanTocPhysics();

        yield return new WaitForSeconds(thoiGianChoRespawn);

        ResetVanTocPhysics();
        currentHP = maxHP;
        CapNhatUI_SoMau();
        isDead = false;
        dangBiPhatDungIm = false;

        yield return new WaitForSeconds(1.0f);
        dangDuocBaoVeAnToan = false;
        thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;
    }

    public void HoiSinhTaiViTriChiDinh(Vector3 viTriHoiSinh)
    {
        if (dangDuocBaoVeAnToan || isDead || isGameFinished) return;
        StartCoroutine(HoiSinhBiTutLaiCoroutine(viTriHoiSinh));
    }

    IEnumerator HoiSinhBiTutLaiCoroutine(Vector3 viTriMoi)
    {
        dangDuocBaoVeAnToan = true;
        currentHP = maxHP;
        CapNhatUI_SoMau();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = viTriMoi;

        if (modelThuyenCon != null)
        {
            modelThuyenCon.localRotation = gocXoayMacDinhModelCon;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(1.0f);

        dangDuocBaoVeAnToan = false;
        thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;
    }

    private void ResetVanTocPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}