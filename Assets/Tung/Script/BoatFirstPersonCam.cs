using UnityEngine;

public class BoatFirstPersonCam : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform boatTarget;

    [Header("Position Adjustment")]
    public float heightOffset = 2.5f;   // Độ cao tầm mắt
    public float forwardOffset = -1.5f; // Nếu nhìn ngược, chỉnh số này để nhích cam ra trước mũi
    public float sideOffset = 0f;

    void LateUpdate()
    {
        if (!boatTarget) return;

        // Tính toán vị trí dựa theo trục của model
        Vector3 setupPosition = boatTarget.position
            + (boatTarget.up * heightOffset)
            + (boatTarget.right * forwardOffset)
            + (boatTarget.forward * sideOffset);

        transform.position = setupPosition;

        // SỬA LỖI NHÌN NGƯỢC: Ép Camera quay ngược hướng nhìn lại 180 độ bằng dấu trừ (-)
        transform.rotation = Quaternion.LookRotation(-boatTarget.right, boatTarget.up);
    }
}