using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BombGameManager : MonoBehaviour
{
    public static BombGameManager Instance;

    [Header("--- MANAGER ---")]
    public MiniGameManager manager;

    [Header("--- TEST (không cần MiniGameManager) ---")]
    public GameObject testPlayer1;
    public GameObject testPlayer2;
    public bool autoStartOnPlay = true;

    [Header("--- CẤU HÌNH VÒNG CHƠI ---")]
    public float roundDuration = 15f;
    public float passCooldown = 0.5f;
    public float firstWaitTime = 3f;

    [Header("--- PLAYER SETTINGS ---")]
    public float playerMoveSpeed = 5f;
    [Tooltip("Tốc độ tối đa khi gần hết giờ")]
    public float maxMoveSpeed = 9f;
    [Tooltip("Bắt đầu tăng tốc khi còn bao nhiêu giây")]
    public float speedBoostTime = 5f;

    [Header("--- SCREEN SHAKE ---")]
    public Camera mainCamera;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.3f;

    [Header("--- BOMB PREFAB ---")]
    public GameObject bombPrefab;
    public float bombFlyDuration = 0.25f;

    [Header("--- UI ---")]
    public TMP_Text timerText;
    public TMP_Text messageText;
    public TMP_Text countdownText;
    public TMP_Text p1NameText;
    public TMP_Text p2NameText;
    public TMP_Text resultText;
    public GameObject resultPanel;

    [Header("--- AUDIO ---")]
    public AudioSource audioSource;   // SFX (tick, explode)
    public AudioSource musicSource;   // Nhạc nền loop riêng
    public AudioClip bgMusicClip;   // Nhạc nền
    public AudioClip tickBombClip;
    public AudioClip explodeBombClip;

    [Header("--- VFX ---")]
    public GameObject explosionVFX;

    // Internal
    private BombCarrier carrier1;
    private BombCarrier carrier2;
    private List<BombCarrier> activePlayers = new List<BombCarrier>();

    private BombCarrier currentBombHolder;
    private GameObject bombInstance;
    private Renderer bombRenderer;
    private Coroutine flyCoroutine;
    private float blinkTimer;

    private float timeLeft;
    private bool roundActive = false;
    private bool isRunning = false;
    private bool hasPlayedTick = false;

    private void Awake() { Instance = this; }

    private void Start()
    {
        if (autoStartOnPlay && manager == null)
            StartMiniGame();
    }

    // ====== BẮT ĐẦU ======
    public void StartMiniGame()
    {
        if (isRunning) return;

        // Lấy player
        GameObject p1obj = manager != null ? manager.currentPlayer1 : testPlayer1;
        GameObject p2obj = manager != null ? manager.currentPlayer2 : testPlayer2;

        carrier1 = p1obj?.GetComponent<BombCarrier>();
        carrier2 = p2obj?.GetComponent<BombCarrier>();

        if (carrier1 == null || carrier2 == null)
        {
            Debug.LogWarning("[BOMB] Missing BombCarrier on player!");
            return;
        }

        // Kích hoạt BombCarrier
        carrier1.SetGameActive(true);
        carrier2.SetGameActive(true);

        activePlayers.Clear();
        activePlayers.Add(carrier1);
        activePlayers.Add(carrier2);

        // Setup movement
        SetUpPlayer(p1obj);
        SetUpPlayer(p2obj);

        // Spawn bomb
        if (bombPrefab != null)
        {
            bombInstance = Instantiate(bombPrefab);
            bombInstance.SetActive(false);
            bombRenderer = bombInstance.GetComponentInChildren<Renderer>();
        }

        // Reset UI
        if (messageText) messageText.text = "";
        if (timerText) timerText.text = "";
        if (countdownText) countdownText.text = "";
        if (resultPanel) resultPanel.SetActive(false);
        if (p1NameText) p1NameText.text = "PLAYER 1";
        if (p2NameText) p2NameText.text = "PLAYER 2";

        isRunning = true;

        // Phát nhạc nền
        if (musicSource != null && bgMusicClip != null)
        {
            musicSource.clip = bgMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        StartCoroutine(GameRoutine());
    }

    // ====== DỪNG ======
    public void StopMiniGame()
    {
        isRunning = false;
        roundActive = false;

        StopAllCoroutines();

        // Dừng nhạc nền
        if (musicSource != null) musicSource.Stop();
        if (audioSource != null) audioSource.Stop();

        // Tắt BombCarrier
        carrier1?.SetGameActive(false);
        carrier2?.SetGameActive(false);

        if (flyCoroutine != null) StopCoroutine(flyCoroutine);
        if (bombInstance != null) Destroy(bombInstance);

        // Trao thưởng
        if (manager != null)
        {
            CheckFinishReward(manager.currentPlayer1);
            CheckFinishReward(manager.currentPlayer2);
        }

        activePlayers.Clear();
    }

    // ====== GAME ROUTINE ======
    IEnumerator GameRoutine()
    {
        // Đếm ngược 3,2,1 GO
        for (int i = (int)firstWaitTime; i > 0; i--)
        {
            if (countdownText) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText) countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        if (countdownText) countdownText.text = "";

        StartNewRound();
    }

    void StartNewRound()
    {
        if (!isRunning) return;

        if (activePlayers.Count <= 1)
        {
            EndGame();
            return;
        }

        // Random người cầm bom
        int index = Random.Range(0, activePlayers.Count);
        AssignBomb(activePlayers[index]);

        timeLeft = roundDuration;
        blinkTimer = 0f;
        hasPlayedTick = false;
        roundActive = true;

        if (messageText) messageText.text = "";
    }

    private void Update()
    {
        if (!roundActive || !isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timerText) timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        UpdateBombBlink(timeLeft / roundDuration);

        // Tiếng tích tắc khi còn 5 giây
        if (timeLeft <= 5f && !hasPlayedTick && tickBombClip != null && audioSource != null)
        {
            hasPlayedTick = true;
            audioSource.clip = tickBombClip;
            audioSource.loop = false;
            audioSource.Play();
        }

        if (timeLeft <= 0f)
        {
            roundActive = false;
            StartCoroutine(ExplodeBomb());
        }

        // Tăng tốc độ player khi gần hết giờ
        if (timeLeft <= speedBoostTime)
        {
            float t = 1f - (timeLeft / speedBoostTime); // 0 → 1
            float newSpeed = Mathf.Lerp(playerMoveSpeed, maxMoveSpeed, t);
            SetPlayerSpeed(newSpeed);
        }
    }

    void UpdateBombBlink(float progressLeft)
    {
        if (bombRenderer == null) return;

        float blinkSpeed = Mathf.Lerp(10f, 1f, progressLeft);
        blinkTimer += Time.deltaTime * blinkSpeed;
        float alpha = (Mathf.Sin(blinkTimer * 10f) + 1f) / 2f;

        Color c = bombRenderer.material.color;
        c.a = Mathf.Lerp(0.3f, 1f, alpha);
        bombRenderer.material.color = c;
    }

    // ====== TRUYỀN BOM ======
    public void TransferBomb(BombCarrier from, BombCarrier to)
    {
        if (!roundActive || !isRunning) return;
        if (from != currentBombHolder) return;
        if (to.IsOnCooldown()) return;

        AssignBomb(to);
    }

    void AssignBomb(BombCarrier holder)
    {
        currentBombHolder?.SetHoldingBomb(false);

        currentBombHolder = holder;
        currentBombHolder.SetHoldingBomb(true);
        currentBombHolder.StartCooldown(passCooldown);

        if (bombInstance != null && holder.bombAnchor != null)
        {
            bombInstance.SetActive(true);
            if (flyCoroutine != null) StopCoroutine(flyCoroutine);
            flyCoroutine = StartCoroutine(FlyBombTo(holder.bombAnchor));
        }
    }

    IEnumerator FlyBombTo(Transform targetAnchor)
    {
        bombInstance.transform.SetParent(null);

        Vector3 startPos = bombInstance.transform.position;
        Quaternion startRot = bombInstance.transform.rotation;
        float t = 0f;

        while (t < bombFlyDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / bombFlyDuration);

            Vector3 pos = Vector3.Lerp(startPos, targetAnchor.position, p);
            pos.y += Mathf.Sin(p * Mathf.PI) * 0.8f;

            bombInstance.transform.position = pos;
            bombInstance.transform.rotation = Quaternion.Slerp(startRot, targetAnchor.rotation, p);

            yield return null;
        }

        bombInstance.transform.SetParent(targetAnchor);
        bombInstance.transform.localPosition = Vector3.zero;
        bombInstance.transform.localRotation = Quaternion.identity;
    }

    // ====== BOM NỔ ======
    IEnumerator ExplodeBomb()
    {
        if (flyCoroutine != null) StopCoroutine(flyCoroutine);

        // Dừng nhạc nền
        if (musicSource != null) musicSource.Stop();

        // Reset tốc độ player
        SetPlayerSpeed(playerMoveSpeed);

        // VFX + SFX nổ
        if (explosionVFX != null && currentBombHolder != null)
            Instantiate(explosionVFX, currentBombHolder.transform.position, Quaternion.identity);

        if (explodeBombClip != null && audioSource != null)
            audioSource.PlayOneShot(explodeBombClip);

        if (bombInstance != null) bombInstance.SetActive(false);

        // Rung màn hình
        if (mainCamera != null)
            StartCoroutine(ShakeCamera());

        // Văng player ra khỏi màn hình
        if (currentBombHolder != null)
            StartCoroutine(LaunchPlayer(currentBombHolder.gameObject));

        // Thông báo thua
        string loserName = currentBombHolder == carrier1 ? "PLAYER 1" : "PLAYER 2";
        if (messageText) messageText.text = $"{loserName} has been eliminated!";

        // Animation chết
        PlayDeadAnimation(currentBombHolder.gameObject);

        // Loại player
        EliminatePlayer(currentBombHolder);

        yield return new WaitForSeconds(2f);

        StartNewRound();
    }

    void PlayDeadAnimation(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerAnimator anim = playerObj.GetComponent<PlayerAnimator>();
        if (anim != null && anim.playerAnimator != null)
            anim.playerAnimator.SetBool("Die", true);
    }

    void EliminatePlayer(BombCarrier carrier)
    {
        activePlayers.Remove(carrier);
        carrier.SetEliminated(true);
        carrier.SetGameActive(false);
    }

    // ====== KẾT THÚC ======
    void EndGame()
    {
        roundActive = false;
        isRunning = false;

        // Dừng nhạc nền
        if (musicSource != null) musicSource.Stop();

        if (resultPanel) resultPanel.SetActive(true);

        if (activePlayers.Count == 1)
        {
            string winnerName = activePlayers[0] == carrier1 ? "PLAYER 1" : "PLAYER 2";
            string color = activePlayers[0] == carrier1 ? "red" : "green";
            if (resultText) resultText.text = $"<color={color}>{winnerName} WINS!</color>";
            if (messageText) messageText.text = $"{winnerName} WINS!";
        }
        else
        {
            if (resultText) resultText.text = "<color=yellow>IT'S A TIE!</color>";
            if (messageText) messageText.text = "It's a tie!";
        }

        if (manager != null)
        {
            CheckFinishReward(manager.currentPlayer1);
            CheckFinishReward(manager.currentPlayer2);
        }
    }

    void CheckFinishReward(GameObject playerObj)
    {
        if (playerObj == null || manager == null) return;

        PlayerMiniGame mini = playerObj.GetComponent<PlayerMiniGame>();
        BombCarrier carrier = playerObj.GetComponent<BombCarrier>();
        if (mini == null || carrier == null) return;

        mini.UpCoin(carrier.IsEliminated() ? 0 : 1, 100);
    }

    [Header("--- LAUNCH ON EXPLODE ---")]
    [Tooltip("Lực văng lên trời")]
    public float launchForce = 8f;
    [Tooltip("Lực văng lên trên (cao)")]
    public float launchUpForce = 30f;
    [Tooltip("Thời gian trước khi ẩn player")]
    public float launchHideTime = 2f;

    // ====== LAUNCH PLAYER KHI NỔ ======
    IEnumerator LaunchPlayer(GameObject playerObj)
    {
        if (playerObj == null) yield break;

        // Tắt PlayerMove để không bị override
        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        if (move != null) move.isJumpAndMove = false;

        // Dùng Rigidbody nếu có
        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        CharacterController cc = playerObj.GetComponent<CharacterController>();

        // Hướng văng lên trời + xoay tròn nhẹ
        Vector3 randomDir = new Vector3(
            Random.Range(-0.3f, 0.3f),  // ngang ít thôi
            1f,                          // lên trời là chính
            Random.Range(-0.3f, 0.3f)
        ).normalized;

        Vector3 launchVelocity = randomDir * launchForce + Vector3.up * launchUpForce;

        if (rb != null)
        {
            // Dùng Rigidbody
            rb.isKinematic = false;
            rb.AddForce(launchVelocity, ForceMode.Impulse);
        }
        else if (cc != null)
        {
            // Dùng CharacterController — move thủ công
            StartCoroutine(LaunchCharacterController(playerObj, cc, launchVelocity));
            yield break;
        }

        // Chờ rồi ẩn player
        yield return new WaitForSeconds(launchHideTime);
        playerObj.SetActive(false);
    }

    IEnumerator LaunchCharacterController(GameObject playerObj, CharacterController cc, Vector3 velocity)
    {
        float timer = 0f;
        float gravity = -18f;

        while (timer < launchHideTime)
        {
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        playerObj.SetActive(false);
    }

    // ====== SCREEN SHAKE ======
    IEnumerator ShakeCamera()
    {
        if (mainCamera == null) yield break;

        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            mainCamera.transform.position = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset về đúng vị trí ban đầu
        mainCamera.transform.position = originalPos;
    }

    // ====== SET TỐC ĐỘ PLAYER ======
    void SetPlayerSpeed(float speed)
    {
        SetSpeedForPlayer(carrier1?.gameObject, speed);
        SetSpeedForPlayer(carrier2?.gameObject, speed);
    }

    void SetSpeedForPlayer(GameObject playerObj, float speed)
    {
        if (playerObj == null) return;
        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        if (move != null && !carrier1.IsEliminated() && !carrier2.IsEliminated())
            move.speed = speed;
    }

    void SetUpPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        PlayerAnimator anim = playerObj.GetComponent<PlayerAnimator>();

        if (move != null)
        {
            move.speed = playerMoveSpeed;
            move.isJumpAndMove = true;
            move.isWalk = true;
        }

        if (anim != null && anim.playerAnimator != null)
            anim.playerAnimator.SetBool("Die", false);
    }
}