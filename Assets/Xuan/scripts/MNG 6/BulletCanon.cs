using UnityEngine;

public class BulletCanon : MonoBehaviour
{
    [Header("Cấu hình Đạn")]
    public float speed = 15f;
    public float lifeTime = 4f; // Tự hủy sau 4 giây nếu không trúng gì

    [Header("Hiệu ứng Nổ")]
    public GameObject explosionPrefab; // Kéo Prefab hiệu ứng nổ vào đây
    public float explosionDestroyTime = 2f; // Thời gian hiệu ứng nổ tồn tại trước khi tự xóa

    [Header("Âm thanh Nổ")]
    public AudioClip explosionSound; // Kéo file âm thanh (.mp3, .wav) vào đây
    [Range(0f, 1f)] public float volume = 1f; // Âm lượng tiếng nổ (từ 0 đến 1)

    private Vector3 moveDirection;

    public void SetupDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerType>() != null || other.CompareTag("Player"))
        {
            TriggerExplosion();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerType>() != null || collision.gameObject.CompareTag("Player"))
        {
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        // 1. Kích hoạt âm thanh nổ 3D tại vị trí va chạm
        if (explosionSound != null)
        {
            // Hàm này tự sinh ra một AudioSource tạm thời tại vị trí nổ, phát xong tự xóa
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);
        }

        // 2. Kích hoạt hiệu ứng hình ảnh
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDestroyTime);
        }

        // 3. Hủy viên đạn
        Destroy(gameObject);
    }
}