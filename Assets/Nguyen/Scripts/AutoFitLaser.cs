using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AutoFitLaser : MonoBehaviour 
{
    private LineRenderer line;

    [Header("Cài đặt Laser")]
    public float laserWidth = 0.5f; 
    public float maxLaserDistance = 20f; 
    public float fadeSpeed = 5f; // Tốc độ co giãn của tia laser (càng cao càng nhanh)

    private float currentWidthMultiplier = 1f;
    private float targetWidthMultiplier = 1f;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    void Update()
    {
        // Tự động chuyển đổi mượt mà giữa độ dày cũ và mục tiêu (0 hoặc 1)
        currentWidthMultiplier = Mathf.MoveTowards(currentWidthMultiplier, targetWidthMultiplier, fadeSpeed * Time.deltaTime);

        // Tính toán độ dày thực tế dựa trên tỉ lệ hiệu ứng
        float calculatedWidth = laserWidth * currentWidthMultiplier;

        // 1. VẼ HÌNH ẢNH TIA LASER
        line.startWidth = calculatedWidth;
        line.endWidth = calculatedWidth;
        line.SetPosition(0, transform.position);

        float currentLaserLength = maxLaserDistance;
        RaycastHit wallHit;

        if (Physics.Raycast(transform.position, transform.forward, out wallHit, maxLaserDistance))
        {
            line.SetPosition(1, wallHit.point);
            currentLaserLength = wallHit.distance;
        }
        else
        {
            line.SetPosition(1, transform.position + transform.forward * maxLaserDistance);
        }

        // 2. XỬ LÝ VA CHẠM (Chỉ gây sát thương khi tia laser đủ lớn)
        if (currentWidthMultiplier > 0.1f) 
        {
            float radius = calculatedWidth / 2f;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, transform.forward, currentLaserLength);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Chém trúng: " + hit.collider.name);
                }
            }
        }
    }

    // Hàm public để Hub bên ngoài gọi điều khiển bật tắt tàng hình
    public void SetLaserActive(bool isActive)
    {
        targetWidthMultiplier = isActive ? 1f : 0f;
    }
}