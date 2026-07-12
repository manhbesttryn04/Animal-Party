using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class IslandGameManager : MonoBehaviour
{
    [Header("Thời gian chơi (120 giây)")]
    public float gameDuration = 120f;
    private float timeLeft;
    private bool isPlaying = false;

    [Header("Cấu hình Đại Bác")]
    public GameObject bulletPrefab;
    public List<Transform> cannonPositions = new List<Transform>();
    public float bulletSpeed = 15f;

    [Header("Cấu hình Quay Nòng Pháo")]
    [Tooltip("Góc quay tối đa sang trái hoặc phải (Ví dụ: 25 độ)")]
    public float maxSpreadAngle = 25f;

    [Header("Cấu hình Âm thanh Bắn Pháo")]
    public AudioClip cannonShotSound;
    [Range(0f, 1f)] public float shotVolume = 0.8f;

    [Header("UI Giao diện")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI p1PercentText;
    public TextMeshProUGUI p2PercentText;

    [Header("Cấu hình Lực Đẩy Vật Lý (Cho CharacterController)")]
    public float baseKnockbackForce = 18f;     // Tăng nhẹ lực đẩy tối thiểu ban đầu lên để tạo độ văng rõ rệt
    public float knockbackScaling = 0.65f;     // Tăng tỷ lệ nhân % để càng nhiều % càng văng xa khủng khiếp hơn
    public float superKnockbackForce = 75f;    // Siêu lực đẩy khi đạt 100%
    public float knockbackDecay = 4.5f;        // Giảm tốc độ dừng lại một chút để player trượt dài mượt mà hơn

    private GameObject player1Obj;
    private GameObject player2Obj;
    private PlayerMove p1MoveScript;
    private PlayerMove p2MoveScript;

    private int p1Percent = 0;
    private int p2Percent = 0;

    private Vector3 p1KnockbackVelocity = Vector3.zero;
    private Vector3 p2KnockbackVelocity = Vector3.zero;

    private Dictionary<Transform, Vector3> cannonOriginalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> cannonOriginalRotations = new Dictionary<Transform, Quaternion>();

    private void Start()
    {
        SaveAndHideAllCannons();
        StartIslandGame();
    }

    void SaveAndHideAllCannons()
    {
        foreach (Transform cannon in cannonPositions)
        {
            if (cannon != null)
            {
                cannonOriginalPositions[cannon] = cannon.position;
                cannonOriginalRotations[cannon] = cannon.rotation;
                cannon.position = cannon.position + new Vector3(0f, -1.5f, 0f);
            }
        }
    }

    public void StartIslandGame()
    {
        timeLeft = gameDuration;
        p1Percent = 0;
        p2Percent = 0;
        p1KnockbackVelocity = Vector3.zero;
        p2KnockbackVelocity = Vector3.zero;
        isPlaying = true;

        if (gameStatusText != null) gameStatusText.text = "SỐNG SÓT TRÊN ĐẢO!";

        UpdatePercentUI();
        FindAndAssignPlayers();

        StartCoroutine(GameTimerRoutine());
        StartCoroutine(CannonAttackRoutine());
    }

    void FindAndAssignPlayers()
    {
        player1Obj = GameObject.FindWithTag("Player 1");
        player2Obj = GameObject.FindWithTag("Player 2");

        if (player1Obj == null || player2Obj == null)
        {
            PlayerType[] allTypes = FindObjectsByType<PlayerType>(FindObjectsSortMode.None);
            foreach (PlayerType pType in allTypes)
            {
                if (!pType.isPlayer2) player1Obj = pType.gameObject;
                else player2Obj = pType.gameObject;
            }
        }

        if (player1Obj != null) p1MoveScript = player1Obj.GetComponent<PlayerMove>();
        if (player2Obj != null) p2MoveScript = player2Obj.GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (!isPlaying) return;

        ApplyCharacterControllerKnockback();

        // KIỂM TRA RỚT ĐẢO
        if (player1Obj != null && player1Obj.transform.position.y < -6f)
        {
            EndGame("PLAYER 2 CHIẾN THẮNG!");
            player1Obj.SetActive(false);
        }
        else if (player2Obj != null && player2Obj.transform.position.y < -6f)
        {
            EndGame("PLAYER 1 CHIẾN THẮNG!");
            player2Obj.SetActive(false);
        }
    }

    void ApplyCharacterControllerKnockback()
    {
        if (p1KnockbackVelocity.magnitude > 0.1f && p1MoveScript != null && p1MoveScript.controller != null)
        {
            p1MoveScript.controller.Move(p1KnockbackVelocity * Time.deltaTime);
            p1KnockbackVelocity = Vector3.Lerp(p1KnockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        if (p2KnockbackVelocity.magnitude > 0.1f && p2MoveScript != null && p2MoveScript.controller != null)
        {
            p2MoveScript.controller.Move(p2KnockbackVelocity * Time.deltaTime);
            p2KnockbackVelocity = Vector3.Lerp(p2KnockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }
    }

    // Hàm xử lý khi đạn nổ trúng người chơi
    public void ProcessBulletHit(GameObject hitPlayer, Vector3 bulletPosition)
    {
        if (!isPlaying) return;

        bool isP1 = hitPlayer.CompareTag("Player 1") || hitPlayer == player1Obj;
        bool isP2 = hitPlayer.CompareTag("Player 2") || hitPlayer == player2Obj;

        if (!isP1 && !isP2) return;

        // Tính số % ngẫu nhiên cộng thêm dựa trên mốc thời gian
        int addedPercent = 2;
        if (timeLeft > 60) addedPercent = Random.Range(4, 9); // Tăng % tích lũy ở giai đoạn đầu lên một chút
        else addedPercent = Random.Range(9, 16); // Giai đoạn sau cộng nhiều hơn để tăng tính kịch tính

        int currentPercent = 0;

        if (isP1)
        {
            p1Percent = Mathf.Min(p1Percent + addedPercent, 100);
            currentPercent = p1Percent;
        }
        else
        {
            p2Percent = Mathf.Min(p2Percent + addedPercent, 100);
            currentPercent = p2Percent;
        }

        UpdatePercentUI();

        // Tính hướng đẩy lùi (Đẩy ngang trên trục XZ)
        Vector3 pushDirection = (hitPlayer.transform.position - bulletPosition).normalized;
        pushDirection.y = 0.05f; // Tạo một chút góc hướng lên trên nhẹ để tránh ma sát sàn nhà làm đứng máy

        float finalForce = baseKnockbackForce;

        if (currentPercent >= 100)
        {
            finalForce = superKnockbackForce;
            if (isP1) p1Percent = 0; else p2Percent = 0;
            Invoke("UpdatePercentUI", 0.6f);
        }
        else
        {
            // FIX LỰC VĂNG: Công thức nhân cộng dồn lũy tiến rõ rệt ngay từ phần trăm nhỏ
            finalForce = baseKnockbackForce + (currentPercent * knockbackScaling);
        }

        if (isP1) p1KnockbackVelocity = pushDirection * finalForce;
        else p2KnockbackVelocity = pushDirection * finalForce;
    }

    void UpdatePercentUI()
    {
        if (p1PercentText != null)
        {
            p1PercentText.text = p1Percent + "%";
            p1PercentText.color = Color.Lerp(Color.white, Color.red, p1Percent / 100f);
        }
        if (p2PercentText != null)
        {
            p2PercentText.text = p2Percent + "%";
            p2PercentText.color = Color.Lerp(Color.white, Color.red, p2Percent / 100f);
        }
    }

    IEnumerator GameTimerRoutine()
    {
        while (timeLeft > 0 && isPlaying)
        {
            if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString() + "s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }
        if (timeLeft <= 0) EndGame("HẾT GIỜ! HÒA NHAU!");
    }

    IEnumerator CannonAttackRoutine()
    {
        while (isPlaying)
        {
            float spawnDelay = 2.5f;
            int cannonsToFire = 1;

            if (timeLeft <= 120 && timeLeft > 90) { spawnDelay = 2.5f; cannonsToFire = 1; }
            else if (timeLeft <= 90 && timeLeft > 60) { spawnDelay = 1.8f; cannonsToFire = 2; }
            else if (timeLeft <= 60 && timeLeft > 30) { spawnDelay = 1.2f; cannonsToFire = 3; }
            else if (timeLeft <= 30) { spawnDelay = 0.7f; cannonsToFire = 4; }

            yield return new WaitForSeconds(spawnDelay);

            List<Transform> selectedCannons = GetRandomCannons(cannonsToFire);
            foreach (Transform cannon in selectedCannons)
            {
                StartCoroutine(AnimateAndShoot(cannon));
            }
        }
    }

    IEnumerator AnimateAndShoot(Transform cannonTransform)
    {
        Vector3 upPos = cannonTransform.position;
        Quaternion originalRot = transform.rotation;

        if (cannonOriginalPositions.ContainsKey(cannonTransform)) upPos = cannonOriginalPositions[cannonTransform];
        if (cannonOriginalRotations.ContainsKey(cannonTransform)) originalRot = cannonOriginalRotations[cannonTransform];

        Vector3 downPos = upPos + new Vector3(0f, -1.5f, 0f);

        // 1. Pháo trồi lên
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            cannonTransform.position = Vector3.Lerp(downPos, upPos, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cannonTransform.position = upPos;

        // 2. Xoay ngẫu nhiên
        float randomAngle = Random.Range(-maxSpreadAngle, maxSpreadAngle);
        Quaternion targetRotation = originalRot * Quaternion.Euler(0f, randomAngle, 0f);

        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            cannonTransform.rotation = Quaternion.Slerp(originalRot, targetRotation, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cannonTransform.rotation = targetRotation;

        yield return new WaitForSeconds(0.1f);

        // 3. Khai hỏa
        Vector3 spawnPosition = cannonTransform.position;
        Quaternion spawnRotation = cannonTransform.rotation;

        Transform firePoint = cannonTransform.Find("FirePoint");
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
            spawnRotation = firePoint.rotation;
        }
        else
        {
            spawnPosition = cannonTransform.position + (cannonTransform.forward * 1.2f);
        }

        if (cannonShotSound != null)
        {
            AudioSource.PlayClipAtPoint(cannonShotSound, spawnPosition, shotVolume);
        }

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);

        // Cài đặt script va chạm trigger mới vào viên đạn
        IslandBulletCollision bulletScript = bullet.AddComponent<IslandBulletCollision>();
        bulletScript.Setup(this);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawnRotation * Vector3.forward * bulletSpeed;
        }

        Destroy(bullet, 4f);

        yield return new WaitForSeconds(0.3f);

        // 4. Lặn xuống
        elapsed = 0f;
        while (elapsed < 0.4f)
        {
            cannonTransform.position = Vector3.Lerp(upPos, downPos, elapsed / 0.4f);
            cannonTransform.rotation = Quaternion.Slerp(targetRotation, originalRot, elapsed / 0.4f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cannonTransform.position = downPos;
        cannonTransform.rotation = originalRot;
    }

    private List<Transform> GetRandomCannons(int count)
    {
        List<Transform> temp = new List<Transform>(cannonPositions);
        List<Transform> result = new List<Transform>();
        count = Mathf.Min(count, temp.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, temp.Count);
            result.Add(temp[index]);
            temp.RemoveAt(index);
        }
        return result;
    }

    public void EndGame(string message)
    {
        isPlaying = false;
        StopAllCoroutines();
        if (gameStatusText != null) gameStatusText.text = message;
    }
}

// FIX VA CHẠM: Chuyển hoàn toàn sang dạng Trigger để bắt dính CharacterController của Player
public class IslandBulletCollision : MonoBehaviour
{
    private IslandGameManager manager;
    private bool hasCollided = false; // Biến cờ bảo vệ chống va chạm trùng lặp nhiều lần 1 frame

    public void Setup(IslandGameManager gameManager)
    {
        manager = gameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        // Quét chuẩn xác component nhận diện Player
        if (other.GetComponent<PlayerType>() != null)
        {
            hasCollided = true; // Khóa va chạm ngay lập tức

            if (manager != null)
            {
                manager.ProcessBulletHit(other.gameObject, transform.position);
            }
            Destroy(gameObject); // Xóa viên đạn ngay khi chạm mục tiêu
        }
    }
}