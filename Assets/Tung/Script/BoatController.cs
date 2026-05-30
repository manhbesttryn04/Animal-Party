using UnityEngine;
using System.Collections;

public class BoatController : MonoBehaviour
{
    [Header("Boat Status")]
    public bool isDead = false;
    public float maxHP = 100f;
    public float currentHP;

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
    public float satThuongVaChamMap = 10f;
    public float lucNayBờ = 8f;

    [Header("Respawn Settings")]
    [Tooltip("Thời gian chờ hồi sinh khi HẾT MÁU chết tại chỗ")]
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
        Time.timeScale = 1f;
        isDead = false;
        dangBiPhatDungIm = false;
        dangDuocBaoVeAnToan = false;
        currentHP = maxHP;
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
    }

    void Update()
    {
        if (isDead) return;

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

    // ================= KHU VỰC DEBUG LOG VA CHẠM =================
    void OnCollisionStay(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;

        // LOG TRƯỚC: Check xem Unity Physics có đang ghi nhận hai vật thể chạm vào nhau không
        // Nếu đâm vào bẫy mà không hiện dòng chữ màu xanh này -> Bạn chưa bật Collider/Rigidbody trên bẫy hoặc thuyền!
        Debug.Log("<color=cyan>[PHYSICS] Thuyền đang cọ xát với vật thể: </color>" + collision.gameObject.name);

        if (isDead || dangDuocBaoVeAnToan)
        {
            // Log thông báo thuyền đang bất tử do vừa hồi sinh/dịch chuyển nên bẫy không thể gây sát thương
            Debug.Log("<color=white>[IMMUNE] Bỏ qua va chạm vì thuyền đang trong trạng thái bất tử bảo vệ.</color>");
            return;
        }

        // Kiểm tra xem vật thể va chạm có chứa component Obstacle (bẫy gai) không
        Obstacle bẫyGai = collision.gameObject.GetComponent<Obstacle>();
        if (bẫyGai != null)
        {
            // LOG 1: Tìm thấy bẫy gai thành công
            Debug.Log("<color=yellow>[SPIKEBALL DETECTED] Đã nhận diện được bẫy gai: </color>" + collision.gameObject.name);

            // Kiểm tra thời gian hồi chiêu nhận sát thương
            if (Time.time < thoiGianChoPhepSatThuongTiep)
            {
                Debug.LogWarning("[COOLDOWN] Đang trong thời gian chớp đỏ chớp vàng chặn bẫy. Còn: " + (thoiGianChoPhepSatThuongTiep - Time.time) + " giây.");
                return;
            }

            thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;

            // LOG 2: Đủ điều kiện và trừ máu
            Debug.Log("<color=red>[SPIKEBALL HIT] Đâm trúng bẫy gai thành công! Trừ đi " + bẫyGai.damageToApply + " HP.</color>");
            XulyMatMau(bẫyGai.damageToApply);
            return;
        }

        // Va chạm bờ map
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Map"))
        {
            ContactPoint contact = collision.contacts[0];
            if (Mathf.Abs(contact.normal.y) < 0.5f)
            {
                if (Time.time < thoiGianChoPhepSatThuongTiep) return;
                thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;

                Debug.Log("<color=orange>[MAP HIT] Va chạm bờ sông! Trừ " + satThuongVaChamMap + " HP.</color>");
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
        // LOG 3: In ra lượng máu còn lại thực tế của con thuyền sau khi ăn đòn
        Debug.Log("<color=magenta>[HP STATUS] " + gameObject.name + " vừa mất máu! Máu hiện tại còn lại: </color>" + currentHP + " / " + maxHP);

        if (currentHP <= 0)
        {
            StartCoroutine(HoiSinhGocCoroutine());
        }
    }

    IEnumerator HoiSinhGocCoroutine()
    {
        isDead = true;
        dangBiPhatDungIm = true;
        dangDuocBaoVeAnToan = true;
        currentHP = 0;
        ResetVanTocPhysics();
        Debug.Log("<color=red>[DIED] Thuyền hết máu! Đứng im tại chỗ chờ hồi sinh...</color>");

        yield return new WaitForSeconds(thoiGianChoRespawn);

        ResetVanTocPhysics();
        currentHP = maxHP;
        isDead = false;
        dangBiPhatDungIm = false;

        yield return new WaitForSeconds(1.0f);
        dangDuocBaoVeAnToan = false;
        thoiGianChoPhepSatThuongTiep = Time.time + cooldownNhanSatThuong;
        Debug.Log("<color=green>[ALIVE] Thuyền hồi sinh đầy máu!</color>");
    }

    public void HoiSinhTaiViTriChiDinh(Vector3 viTriHoiSinh)
    {
        if (dangDuocBaoVeAnToan || isDead) return;
        StartCoroutine(HoiSinhBiTutLaiCoroutine(viTriHoiSinh));
    }

    IEnumerator HoiSinhBiTutLaiCoroutine(Vector3 viTriMoi)
    {
        dangDuocBaoVeAnToan = true;
        currentHP = maxHP;

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