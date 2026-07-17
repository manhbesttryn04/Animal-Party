using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MiniGame7 : MonoBehaviour
{
    [Header("Manager")]
    public MiniGameManager manager;

    [Header("Thời gian chơi")]
    public float gameDuration = 120f;

    private float timeLeft;
    private bool isPlaying;

    [Header("Cấu hình Đại Bác")]
    public GameObject bulletPrefab;
    public List<Transform> cannonPositions = new List<Transform>();
    public float bulletSpeed = 15f;

    [Header("Cấu hình Quay Nòng Pháo")]
    public float maxSpreadAngle = 25f;

    [Header("Âm thanh")]
    public AudioClip cannonShotSound;

    [Range(0f, 1f)]
    public float shotVolume = 0.8f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI p1PercentText;
    public TextMeshProUGUI p2PercentText;
    public GameObject canvasMain;

    [Header("Knockback")]
    public float baseKnockbackForce = 18f;
    public float knockbackScaling = 0.65f;
    public float superKnockbackForce = 75f;
    public float knockbackDecay = 4.5f;

    [Header("Điều kiện rơi khỏi đảo")]
    public float fallHeight = -6f;

    private GameObject player1Obj;
    private GameObject player2Obj;

    private PlayerMove p1MoveScript;
    private PlayerMove p2MoveScript;

    private int p1Percent;
    private int p2Percent;

    private Vector3 p1KnockbackVelocity;
    private Vector3 p2KnockbackVelocity;

    private Dictionary<Transform, Vector3> cannonOriginalPositions =
        new Dictionary<Transform, Vector3>();

    private Dictionary<Transform, Quaternion> cannonOriginalRotations =
        new Dictionary<Transform, Quaternion>();

    // Coroutine chính
    private Coroutine timerRoutine;
    private Coroutine cannonRoutine;

    // Các Coroutine đại bác đang chạy
    private readonly List<Coroutine> activeCannonRoutines =
        new List<Coroutine>();

    private void Awake()
    {
        SaveCannonOriginalTransform();
        HideAllCannons();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        ApplyCharacterControllerKnockback();
        CheckPlayersFalling();
    }

    // =========================================================
    // START MINIGAME
    // =========================================================

    public void StartMiniGame()
    {
      
        // Dừng lần chơi cũ trước
        StopMiniGame();
        canvasMain.SetActive(true);
        FindAndAssignPlayers();
        SetUpAllPlayer();
        ResetGameData();
        ResetPlayers();
        ResetCannons();

        isPlaying = true;

        if (gameStatusText != null)
            gameStatusText.text = "";

        UpdatePercentUI();
        UpdateTimerUI();

        timerRoutine = StartCoroutine(GameTimerRoutine());
        cannonRoutine = StartCoroutine(CannonAttackRoutine());
    }

    // Giữ tên hàm cũ để những script đang tham chiếu vẫn dùng được
    public void StartIslandGame()
    {
        StartMiniGame();
    }

    // =========================================================
    // STOP MINIGAME
    // =========================================================

    public void StopMiniGame()
    {
        canvasMain?.SetActive(false);
        isPlaying = false;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        if (cannonRoutine != null)
        {
            StopCoroutine(cannonRoutine);
            cannonRoutine = null;
        }

        StopActiveCannonRoutines();
        DestroyAllBullets();

        p1KnockbackVelocity = Vector3.zero;
        p2KnockbackVelocity = Vector3.zero;

        ResetCannons();
    }

    // =========================================================
    // RESET
    // =========================================================

    private void ResetGameData()
    {
        timeLeft = gameDuration;

        p1Percent = 0;
        p2Percent = 0;

        p1KnockbackVelocity = Vector3.zero;
        p2KnockbackVelocity = Vector3.zero;
    }

    private void ResetPlayers()
    {
        if (player1Obj != null)
        {
            player1Obj.SetActive(true);

            p1MoveScript = player1Obj.GetComponent<PlayerMove>();
        }

        if (player2Obj != null)
        {
            player2Obj.SetActive(true);

            p2MoveScript = player2Obj.GetComponent<PlayerMove>();
        }
    }

    // =========================================================
    // TÌM PLAYER
    // =========================================================

    private void FindAndAssignPlayers()
    {
        player1Obj = null;
        player2Obj = null;

        // Ưu tiên Player từ MiniGameManager
        if (manager != null)
        {
            player1Obj = manager.currentPlayer1;
            player2Obj = manager.currentPlayer2;
        }

        // Nếu Manager chưa có thì tìm bằng Tag
        if (player1Obj == null)
            player1Obj = GameObject.FindWithTag("Player 1");

        if (player2Obj == null)
            player2Obj = GameObject.FindWithTag("Player 2");

        // Nếu vẫn chưa có thì tìm bằng PlayerType
        if (player1Obj == null || player2Obj == null)
        {
            PlayerType[] allPlayerTypes =
                FindObjectsByType<PlayerType>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (PlayerType playerType in allPlayerTypes)
            {
                if (playerType == null)
                    continue;

                if (!playerType.isPlayer2)
                    player1Obj = playerType.gameObject;
                else
                    player2Obj = playerType.gameObject;
            }
        }

        if (player1Obj != null)
            p1MoveScript = player1Obj.GetComponent<PlayerMove>();

        if (player2Obj != null)
            p2MoveScript = player2Obj.GetComponent<PlayerMove>();
    }

    // =========================================================
    // KIỂM TRA RƠI KHỎI ĐẢO
    // =========================================================

    private void CheckPlayersFalling()
    {
        bool p1Fell =
            player1Obj != null &&
            player1Obj.transform.position.y < fallHeight;

        bool p2Fell =
            player2Obj != null &&
            player2Obj.transform.position.y < fallHeight;

        if (p1Fell && p2Fell)
        {          
            EndGame(-1, "DRAW");
            manager.timer = 2f;
        }
        else if (p1Fell)
        {
            EndGame(1, "PLAYER 2 WIN");
            manager.timer = 2f;
        }
        else if (p2Fell)
        {
            EndGame(0, "PLAYER 1 WIN");
            manager.timer = 2f;
        }
        
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    private void ApplyCharacterControllerKnockback()
    {
        if (p1KnockbackVelocity.magnitude > 0.1f &&
            p1MoveScript != null &&
            p1MoveScript.controller != null)
        {
            p1MoveScript.controller.Move(
                p1KnockbackVelocity * Time.deltaTime
            );

            p1KnockbackVelocity = Vector3.Lerp(
                p1KnockbackVelocity,
                Vector3.zero,
                knockbackDecay * Time.deltaTime
            );
        }
        else
        {
            p1KnockbackVelocity = Vector3.zero;
        }

        if (p2KnockbackVelocity.magnitude > 0.1f &&
            p2MoveScript != null &&
            p2MoveScript.controller != null)
        {
            p2MoveScript.controller.Move(
                p2KnockbackVelocity * Time.deltaTime
            );

            p2KnockbackVelocity = Vector3.Lerp(
                p2KnockbackVelocity,
                Vector3.zero,
                knockbackDecay * Time.deltaTime
            );
        }
        else
        {
            p2KnockbackVelocity = Vector3.zero;
        }
    }

    public void ProcessBulletHit(
        GameObject hitPlayer,
        Vector3 bulletPosition
    )
    {
        if (!isPlaying || hitPlayer == null)
            return;

        // Hỗ trợ Collider nằm ở object con
        PlayerType playerType =
            hitPlayer.GetComponentInParent<PlayerType>();

        GameObject playerRoot =
            playerType != null
                ? playerType.gameObject
                : hitPlayer;

        bool isP1 =
            playerRoot == player1Obj ||
            playerRoot.CompareTag("Player 1");

        bool isP2 =
            playerRoot == player2Obj ||
            playerRoot.CompareTag("Player 2");

        if (!isP1 && !isP2)
            return;

        int addedPercent;

        if (timeLeft > 60f)
            addedPercent = Random.Range(4, 9);
        else
            addedPercent = Random.Range(9, 16);

        int currentPercent;

        if (isP1)
        {
            p1Percent = Mathf.Min(
                p1Percent + addedPercent,
                100
            );

            currentPercent = p1Percent;
        }
        else
        {
            p2Percent = Mathf.Min(
                p2Percent + addedPercent,
                100
            );

            currentPercent = p2Percent;
        }

        UpdatePercentUI();

        Vector3 pushDirection =
            playerRoot.transform.position - bulletPosition;

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.001f)
            pushDirection = -playerRoot.transform.forward;

        pushDirection.Normalize();
        pushDirection.y = 0.05f;
        pushDirection.Normalize();

        float finalForce =
            baseKnockbackForce +
            currentPercent * knockbackScaling;

        if (currentPercent >= 100)
        {
            finalForce = superKnockbackForce;

            if (isP1)
                p1Percent = 0;
            else
                p2Percent = 0;

            StartCoroutine(UpdatePercentUIDelay());
        }

        if (isP1)
            p1KnockbackVelocity = pushDirection * finalForce;
        else
            p2KnockbackVelocity = pushDirection * finalForce;
    }

    private IEnumerator UpdatePercentUIDelay()
    {
        yield return new WaitForSeconds(0.6f);

        if (isPlaying)
            UpdatePercentUI();
    }

    // =========================================================
    // TIMER
    // =========================================================

    private IEnumerator GameTimerRoutine()
    {
        while (timeLeft > 0f && isPlaying)
        {
            UpdateTimerUI();

            yield return null;

            timeLeft -= Time.deltaTime;
        }

        timeLeft = 0f;
        UpdateTimerUI();

        if (isPlaying)
            EndGame(-1, "DRAW");

        timerRoutine = null;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(timeLeft) + "s";
        }
    }

    // =========================================================
    // CANNON ATTACK
    // =========================================================

    private IEnumerator CannonAttackRoutine()
    {
        while (isPlaying)
        {
            float spawnDelay;
            int cannonsToFire;

            if (timeLeft > 90f)
            {
                spawnDelay = 2.5f;
                cannonsToFire = 1;
            }
            else if (timeLeft > 60f)
            {
                spawnDelay = 1.8f;
                cannonsToFire = 2;
            }
            else if (timeLeft > 30f)
            {
                spawnDelay = 1.2f;
                cannonsToFire = 3;
            }
            else
            {
                spawnDelay = 0.7f;
                cannonsToFire = 4;
            }

            yield return new WaitForSeconds(spawnDelay);

            if (!isPlaying)
                break;

            List<Transform> selectedCannons =
                GetRandomCannons(cannonsToFire);

            foreach (Transform cannon in selectedCannons)
            {
                if (cannon == null)
                    continue;

                Coroutine routine =
                    StartCoroutine(AnimateAndShoot(cannon));

                activeCannonRoutines.Add(routine);
            }
        }

        cannonRoutine = null;
    }

    private IEnumerator AnimateAndShoot(
        Transform cannonTransform
    )
    {
        if (cannonTransform == null)
            yield break;

        Vector3 upPosition =
            cannonOriginalPositions[cannonTransform];

        Quaternion originalRotation =
            cannonOriginalRotations[cannonTransform];

        Vector3 downPosition =
            upPosition + Vector3.down * 1.5f;

        // Pháo đi lên
        yield return MoveCannon(
            cannonTransform,
            downPosition,
            upPosition,
            originalRotation,
            originalRotation,
            0.3f
        );

        if (!isPlaying)
            yield break;

        // Xoay pháo
        float randomAngle =
            Random.Range(
                -maxSpreadAngle,
                maxSpreadAngle
            );

        Quaternion targetRotation =
            originalRotation *
            Quaternion.Euler(0f, randomAngle, 0f);

        yield return RotateCannon(
            cannonTransform,
            originalRotation,
            targetRotation,
            0.2f
        );

        yield return new WaitForSeconds(0.1f);

        if (!isPlaying)
            yield break;

        ShootBullet(cannonTransform);

        yield return new WaitForSeconds(0.3f);

        // Pháo đi xuống
        yield return MoveCannon(
            cannonTransform,
            upPosition,
            downPosition,
            targetRotation,
            originalRotation,
            0.4f
        );

        cannonTransform.position = downPosition;
        cannonTransform.rotation = originalRotation;
    }

    private IEnumerator MoveCannon(
        Transform cannon,
        Vector3 startPosition,
        Vector3 endPosition,
        Quaternion startRotation,
        Quaternion endRotation,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPlaying)
                yield break;

            float progress = elapsed / duration;

            cannon.position = Vector3.Lerp(
                startPosition,
                endPosition,
                progress
            );

            cannon.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                progress
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        cannon.position = endPosition;
        cannon.rotation = endRotation;
    }

    private IEnumerator RotateCannon(
        Transform cannon,
        Quaternion startRotation,
        Quaternion endRotation,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPlaying)
                yield break;

            cannon.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                elapsed / duration
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        cannon.rotation = endRotation;
    }

    private void ShootBullet(Transform cannonTransform)
    {
        if (bulletPrefab == null || cannonTransform == null)
            return;

        Transform firePoint =
            cannonTransform.Find("FirePoint");

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
            spawnRotation = firePoint.rotation;

            // Tìm ParticleSystem trong FirePoint hoặc object con của FirePoint
            ParticleSystem muzzleFlash =
                firePoint.GetComponentInChildren<ParticleSystem>(true);

            if (muzzleFlash != null)
            {
                // Xóa particle cũ để mỗi lần bắn hiệu ứng chạy lại rõ ràng
                muzzleFlash.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                muzzleFlash.Play(true);
            }
        }
        else
        {
            // Nếu không có FirePoint thì bắn từ phía trước đại bác
            spawnPosition =
                cannonTransform.position +
                cannonTransform.forward * 1.2f;

            spawnRotation =
                cannonTransform.rotation;
        }

        // Phát âm thanh bắn
        AudioManager.Instance.PlaySFX(AudioManager.Instance.cannonClip);

        // Tạo viên đạn
        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            spawnRotation
        );

        // Lấy hoặc thêm script va chạm cho đạn
        IslandBulletCollision bulletCollision =
            bullet.GetComponent<IslandBulletCollision>();

        if (bulletCollision == null)
        {
            bulletCollision =
                bullet.AddComponent<IslandBulletCollision>();
        }

        bulletCollision.Setup(this);

        // Cho đạn bay theo hướng của FirePoint
        Rigidbody rb =
            bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                spawnRotation *
                Vector3.forward *
                bulletSpeed;
        }
        else
        {
            Debug.LogWarning(
                "Bullet prefab does not have a Rigidbody component!"
            );
        }

        // Tự hủy đạn sau 4 giây
        Destroy(bullet, 4f);
    }

    // =========================================================
    // CANNON RESET
    // =========================================================

    private void SaveCannonOriginalTransform()
    {
        cannonOriginalPositions.Clear();
        cannonOriginalRotations.Clear();

        foreach (Transform cannon in cannonPositions)
        {
            if (cannon == null)
                continue;

            cannonOriginalPositions[cannon] =
                cannon.position;

            cannonOriginalRotations[cannon] =
                cannon.rotation;
        }
    }

    private void HideAllCannons()
    {
        foreach (Transform cannon in cannonPositions)
        {
            if (cannon == null)
                continue;

            if (!cannonOriginalPositions.ContainsKey(cannon))
                continue;

            cannon.position =
                cannonOriginalPositions[cannon] +
                Vector3.down * 1.5f;

            cannon.rotation =
                cannonOriginalRotations[cannon];
        }
    }

    private void ResetCannons()
    {
        foreach (Transform cannon in cannonPositions)
        {
            if (cannon == null)
                continue;

            if (!cannonOriginalPositions.ContainsKey(cannon))
                continue;

            cannon.position =
                cannonOriginalPositions[cannon] +
                Vector3.down * 1.5f;

            cannon.rotation =
                cannonOriginalRotations[cannon];
        }
    }

    private void StopActiveCannonRoutines()
    {
        foreach (Coroutine routine in activeCannonRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        activeCannonRoutines.Clear();
    }

    private void DestroyAllBullets()
    {
        IslandBulletCollision[] bullets =
            FindObjectsByType<IslandBulletCollision>(
                FindObjectsSortMode.None
            );

        foreach (IslandBulletCollision bullet in bullets)
        {
            if (bullet != null)
                Destroy(bullet.gameObject);
        }
    }

    private List<Transform> GetRandomCannons(int count)
    {
        List<Transform> availableCannons =
            new List<Transform>();

        foreach (Transform cannon in cannonPositions)
        {
            if (cannon != null)
                availableCannons.Add(cannon);
        }

        List<Transform> result =
            new List<Transform>();

        count = Mathf.Min(
            count,
            availableCannons.Count
        );

        for (int i = 0; i < count; i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableCannons.Count
                );

            result.Add(
                availableCannons[randomIndex]
            );

            availableCannons.RemoveAt(
                randomIndex
            );
        }

        return result;
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdatePercentUI()
    {
        if (p1PercentText != null)
        {
            p1PercentText.text =
                p1Percent + "%";

            p1PercentText.color =
                Color.Lerp(
                    Color.white,
                    Color.red,
                    p1Percent / 100f
                );
        }

        if (p2PercentText != null)
        {
            p2PercentText.text =
                p2Percent + "%";

            p2PercentText.color =
                Color.Lerp(
                    Color.white,
                    Color.red,
                    p2Percent / 100f
                );
        }
    }

    // =========================================================
    // END GAME
    // winnerIndex:
    // 0 = Player 1
    // 1 = Player 2
    // -1 = Hòa
    // =========================================================

    public void EndGame(int winnerIndex, string message)
    {
        if (!isPlaying)
            return;

        StartCoroutine(EndGameRoutine(winnerIndex, message));
    }

    private IEnumerator EndGameRoutine(
        int winnerIndex,
        string message
    )
    {
        // Khóa gameplay ngay lập tức
        isPlaying = false;

        // Dừng timer
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        // Dừng đại bác
        if (cannonRoutine != null)
        {
            StopCoroutine(cannonRoutine);
            cannonRoutine = null;
        }

        StopActiveCannonRoutines();
        DestroyAllBullets();

        p1KnockbackVelocity = Vector3.zero;
        p2KnockbackVelocity = Vector3.zero;

        ResetCannons();

        // Giữ Canvas mở để hiện kết quả
        if (canvasMain != null)
            canvasMain.SetActive(true);

        if (gameStatusText != null)
            gameStatusText.text = message;

        // Xử lý người thắng
        if (winnerIndex == 0)
        {
            Debug.Log("Player 1 thắng");
        }
        else if (winnerIndex == 1)
        {
            Debug.Log("Player 2 thắng");
        }
        else
        {
            Debug.Log("Minigame hòa");
        }

        // Hiện message trong 1.5 giây
        yield return new WaitForSeconds(1.5f);

        // Xóa message
        if (gameStatusText != null)
            gameStatusText.text = "";

        // Tắt UI minigame
        if (canvasMain != null)
            canvasMain.SetActive(false);
    }

    // Giữ lại hàm cũ nếu script khác đang gọi
    public void EndGame(string message)
    {
        EndGame(-1, message);
    }

    private void OnDisable()
    {
        StopMiniGame();
    }
    public void SetUpAllPlayer()
    {
        PlayerDefense p1 = player1Obj.GetComponent<PlayerDefense>();
        PlayerDefense p2 = player2Obj.GetComponent<PlayerDefense>();

        if(p1 != null && p2 != null)
        {
            p1.hasDefense = true;
            p2.hasDefense = true;
        }
    }
}

// =============================================================
// BULLET COLLISION
// =============================================================

public class IslandBulletCollision : MonoBehaviour
{
    private MiniGame7 manager;
    private bool hasCollided;

    public void Setup(MiniGame7 gameManager)
    {
        manager = gameManager;
        hasCollided = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided)
            return;

        // Đụng khiên thì chặn đạn
        if (other.CompareTag("Defense"))
        {
            hasCollided = true;

            Destroy(gameObject, 4f);
            return;
        }

        PlayerType playerType =
            other.GetComponentInParent<PlayerType>();

        if (playerType == null)
            return;

        hasCollided = true;

        if (manager != null)
        {
            manager.ProcessBulletHit(
                playerType.gameObject,
                transform.position
            );
        }

        Destroy(gameObject);
    }
}