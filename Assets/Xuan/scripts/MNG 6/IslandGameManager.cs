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

    [Header("UI Giao diện")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI p1PercentText;
    public TextMeshProUGUI p2PercentText;

    [Header("Cấu hình Lực Đẩy Vật Lý (Cho CharacterController)")]
    public float baseKnockbackForce = 12f;     // Lực đẩy tối thiểu khi 0%
    public float knockbackScaling = 0.45f;    // Tốc độ tăng lực đẩy theo %
    public float superKnockbackForce = 65f;   // Siêu lực đẩy khi đạt 100%
    public float knockbackDecay = 6f;         // Tốc độ dừng lại sau khi bị đẩy (Số càng cao dừng càng nhanh)

    // Biến lưu trữ đối tượng Player tự động tìm kiếm
    private GameObject player1Obj;
    private GameObject player2Obj;
    private PlayerMove p1MoveScript;
    private PlayerMove p2MoveScript;

    // Biến lưu trữ % tích tụ nội bộ trong GameManager
    private int p1Percent = 0;
    private int p2Percent = 0;

    // Lưu trữ vận tốc đẩy lùi của từng người chơi để xử lý CharacterController mượt mà
    private Vector3 p1KnockbackVelocity = Vector3.zero;
    private Vector3 p2KnockbackVelocity = Vector3.zero;

    private Dictionary<Transform, Vector3> cannonOriginalPositions = new Dictionary<Transform, Vector3>();

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

        // Lấy liên kết đến script di chuyển của Player để chuẩn bị đẩy bằng Controller
        if (player1Obj != null) p1MoveScript = player1Obj.GetComponent<PlayerMove>();
        if (player2Obj != null) p2MoveScript = player2Obj.GetComponent<PlayerMove>();

        if (player1Obj == null) Debug.LogError("Không tìm thấy Player 1 trong Scene!");
        if (player2Obj == null) Debug.LogError("Không tìm thấy Player 2 trong Scene!");
    }

    void Update()
    {
        if (!isPlaying) return;

        // XỬ LÝ DI CHUYỂN ĐẨY LÙI CHO CHARACTER CONTROLLER THEO TỪNG FRAME
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

    // Hàm liên tục thực thi lực đẩy bằng lệnh .Move() của Player
    void ApplyCharacterControllerKnockback()
    {
        // Xử lý đẩy Player 1
        if (p1KnockbackVelocity.magnitude > 0.1f && p1MoveScript != null && p1MoveScript.controller != null)
        {
            p1MoveScript.controller.Move(p1KnockbackVelocity * Time.deltaTime);
            // Giảm dần lực đẩy theo thời gian để tạo độ ma sát lướt đi rồi dừng lại
            p1KnockbackVelocity = Vector3.Lerp(p1KnockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        // Xử lý đẩy Player 2
        if (p2KnockbackVelocity.magnitude > 0.1f && p2MoveScript != null && p2MoveScript.controller != null)
        {
            p2MoveScript.controller.Move(p2KnockbackVelocity * Time.deltaTime);
            p2KnockbackVelocity = Vector3.Lerp(p2KnockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }
    }

    // HÀM XỬ LÝ TRÚNG ĐẠN MỚI: TÍNH VECTOR VẬN TỐC ĐẨY THAY VÌ RIGIDBODY
    public void ProcessBulletHit(GameObject hitPlayer, Vector3 bulletPosition)
    {
        if (!isPlaying) return;

        bool isP1 = hitPlayer.CompareTag("Player 1") || hitPlayer == player1Obj;
        bool isP2 = hitPlayer.CompareTag("Player 2") || hitPlayer == player2Obj;

        if (!isP1 && !isP2) return;

        int addedPercent = 2;
        if (timeLeft > 60) addedPercent = Random.Range(2, 6);
        else addedPercent = Random.Range(6, 11);

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

        // Tính toán hướng đẩy từ tâm viên đạn ra Player
        Vector3 pushDirection = (hitPlayer.transform.position - bulletPosition).normalized;
        pushDirection.y = 0.1f; // Độ nảy nhẹ trên không sàn phẳng

        float finalForce = baseKnockbackForce;

        if (currentPercent >= 100)
        {
            finalForce = superKnockbackForce; // Siêu lực hất văng khi 100%

            if (isP1) p1Percent = 0; else p2Percent = 0;
            Invoke("UpdatePercentUI", 0.6f);
        }
        else
        {
            // Lực tỷ lệ thuận với số phần trăm đang có
            finalForce = baseKnockbackForce + (currentPercent * knockbackScaling);
        }

        // Nạp vector vận tốc đẩy vào biến nội bộ của GameManager để cập nhật ở Update()
        if (isP1)
        {
            p1KnockbackVelocity = pushDirection * finalForce;
        }
        else
        {
            p2KnockbackVelocity = pushDirection * finalForce;
        }
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
        if (cannonOriginalPositions.ContainsKey(cannonTransform))
        {
            upPos = cannonOriginalPositions[cannonTransform];
        }

        Vector3 downPos = upPos + new Vector3(0f, -1.5f, 0f);

        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            cannonTransform.position = Vector3.Lerp(downPos, upPos, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cannonTransform.position = upPos;

        yield return new WaitForSeconds(0.1f);

        // --- ĐOẠN ĐIỀU CHỈNH VỊ TRÍ ĐẦU NÒNG SÚNG MỚI ---
        Vector3 spawnPosition = cannonTransform.position;
        Quaternion spawnRotation = cannonTransform.rotation;

        // Tự tìm kiếm Object trống con tên là "FirePoint" nằm bên dưới khẩu đại bác
        Transform firePoint = cannonTransform.Find("FirePoint");

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
            spawnRotation = firePoint.rotation;
        }
        else
        {
            // Bù trừ vị trí lên phía trước nếu chưa tạo FirePoint trên Editor
            spawnPosition = cannonTransform.position + (cannonTransform.forward * 1.2f);
        }

        // Tạo đạn tại đúng vị trí đầu nòng
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);

        IslandBulletCollision bulletScript = bullet.AddComponent<IslandBulletCollision>();
        bulletScript.Setup(this);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Viên đạn bay thẳng theo hướng mặt của đầu nòng súng
            rb.linearVelocity = spawnRotation * Vector3.forward * bulletSpeed;
        }

        Destroy(bullet, 4f);

        yield return new WaitForSeconds(0.3f);

        elapsed = 0f;
        while (elapsed < 0.4f)
        {
            cannonTransform.position = Vector3.Lerp(upPos, downPos, elapsed / 0.4f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cannonTransform.position = downPos;
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

public class IslandBulletCollision : MonoBehaviour
{
    private IslandGameManager manager;

    public void Setup(IslandGameManager gameManager)
    {
        manager = gameManager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerType>() != null)
        {
            if (manager != null)
            {
                manager.ProcessBulletHit(collision.gameObject, transform.position);
            }
            Destroy(gameObject);
        }
    }
}