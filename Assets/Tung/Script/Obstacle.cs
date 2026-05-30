using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Sát Thương Bẫy")]
    [Tooltip("Số máu sẽ trừ của thuyền khi đâm vào quả cầu gai này")]
    public float damageToApply = 15f;

    void Start()
    {
        // TỰ ĐỘNG LỌC BUG: Đảm bảo quả cầu gai luôn có Collider để bắt va chạm
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }

        // Giữ nguyên false để va chạm vật lý cục súc
        col.isTrigger = false;

        // XÓA BỎ HOÀN TOÀN CÁI DÒNG ÉP TAG "MAP" ĐỂ ĐÉO BỊ ĐÈ TAG LÚC PLAY GAME!
    }
}