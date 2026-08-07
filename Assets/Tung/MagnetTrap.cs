using UnityEngine;

public class MagnetTrap : TrapBase
{
    [Header("--- MAGNET TRAP ---")]
    [Tooltip("Bán kính vùng hút nam châm")]
    public float pullRadius = 5f;

    [Tooltip("Lực hút (càng cao hút càng mạnh)")]
    public float pullForce = 15f;

    [Tooltip("Thời gian hiệu ứng hút kéo dài")]
    public float magnetDuration = 2.5f;

    [Header("--- VFX SETTINGS ---")]
    [Tooltip("VFX vòng xoáy/nam châm đặt tại tâm bẫy khi kích hoạt")]
    public GameObject magnetVFX;

    public Vector3 vfxOffset = new Vector3(0, 0.5f, 0);
    public Vector3 vfxScale = new Vector3(1f, 1f, 1f);

    protected override void OnPlayerHit(BombCarrier carrier)
    {
        // Gọi hàm xử lý hút trên BombCarrier
        carrier.ApplyMagnet(transform.position, pullForce, magnetDuration, magnetVFX, vfxOffset, vfxScale);
    }

    // Vẽ vòng bán kính hút trong Scene View để dễ căn chỉnh trên Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}