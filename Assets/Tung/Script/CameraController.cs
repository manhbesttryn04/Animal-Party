using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Multiplayer Targets")]
    public BoatController boat1;
    public BoatController boat2;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 22f, -14f);
    public float smoothSpeed = 6f;
    public float cameraPitch = 55f;

    [Header("Gameplay Rules (Chế độ 2 người)")]
    public float khoangCachToiDa = 25f;
    public float cuLyHoiSinhPhiaSau = 6f;

    private Transform currentLeader;
    private Transform lastKnownLeader;

    void Start()
    {
        transform.rotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void LateUpdate()
    {
        if (boat1 == null || boat2 == null) return;

        DetermineLeader();

        Transform targetLook = currentLeader != null ? currentLeader : lastKnownLeader;

        if (targetLook != null)
        {
            if (currentLeader != null)
            {
                Vector3 targetPosition = currentLeader.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

                KiemTraKhoangCachVaPhat();
            }
        }
    }

    void DetermineLeader()
    {
        if (boat1 == null || boat2 == null) return;

        if (boat1.isDead && !boat2.isDead)
        {
            currentLeader = boat2.transform;
            lastKnownLeader = boat2.transform;
            return;
        }

        if (boat2.isDead && !boat1.isDead)
        {
            currentLeader = boat1.transform;
            lastKnownLeader = boat1.transform;
            return;
        }

        if (boat1.isDead && boat2.isDead)
        {
            currentLeader = null;
            return;
        }

        if (boat1.transform.position.x < boat2.transform.position.x)
        {
            currentLeader = boat1.transform;
            lastKnownLeader = boat1.transform;
        }
        else
        {
            currentLeader = boat2.transform;
            lastKnownLeader = boat2.transform;
        }
    }

    void KiemTraKhoangCachVaPhat()
    {
        if (boat1.isDead || boat2.isDead || currentLeader == null) return;

        float khoangCachTratBanh = Mathf.Abs(boat1.transform.position.x - boat2.transform.position.x);

        if (khoangCachTratBanh > khoangCachToiDa)
        {
            BoatController thuyenDanDau = currentLeader.GetComponent<BoatController>();
            BoatController thuyenTutLai = (thuyenDanDau == boat1) ? boat2 : boat1;

            if (thuyenTutLai != null)
            {
                Vector3 viTriSpawnMoi = thuyenDanDau.transform.position;

                viTriSpawnMoi.x += cuLyHoiSinhPhiaSau;
                viTriSpawnMoi.y = thuyenDanDau.transform.position.y;
                viTriSpawnMoi.z = thuyenDanDau.transform.position.z;

                thuyenTutLai.HoiSinhTaiViTriChiDinh(viTriSpawnMoi);
            }
        }
    }
}