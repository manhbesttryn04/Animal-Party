using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BombGameManager : MonoBehaviour
{
    public static BombGameManager Instance;

    [Header("--- MANAGER ---")]
    public MiniGameManager manager;

    [Header("--- CẤU HÌNH VÒNG CHƠI ---")]
    public float roundDuration = 15f;
    public float passCooldown = 0.5f;
    public float firstWaitTime = 3f;   // đếm ngược trước khi bắt đầu

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
    public AudioSource audioSource;
    public AudioClip tickBombClip;    // tiếng tích tắc
    public AudioClip explodeBombClip; // tiếng nổ

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

    private float timeLeft;
    private float blinkTimer;
    private bool roundActive = false;
    private bool isRunning = false;

    private void Awake()
    {
        Instance = this;
    }

    // ====== GỌI TỪ MINIGAME MANAGER ĐỂ BẮT ĐẦU ======
    public void StartMiniGame()
    {
        if (isRunning) return;

        // Lấy BombCarrier từ currentPlayer1/2
        carrier1 = manager.currentPlayer1?.GetComponent<BombCarrier>();
        carrier2 = manager.currentPlayer2?.GetComponent<BombCarrier>();

        if (carrier1 == null || carrier2 == null)
        {
            Debug.LogWarning("[BOMB] currentPlayer1 hoặc currentPlayer2 thiếu BombCarrier!");
            return;
        }

        // Kích hoạt carrier
        carrier1.Activate();
        carrier2.Activate();

        activePlayers.Clear();
        activePlayers.Add(carrier1);
        activePlayers.Add(carrier2);

        // Setup player movement
        SetUpAllPlayer();

        // Spawn bomb
        if (bombPrefab != null)
        {
            bombInstance = Instantiate(bombPrefab);
            bombInstance.SetActive(false);
            bombRenderer = bombInstance.GetComponentInChildren<Renderer>();
        }

        if (messageText) messageText.text = "";
        if (timerText) timerText.text = "";
        if (countdownText) countdownText.text = "";
        if (resultPanel) resultPanel.SetActive(false);

        // Hiện tên 2 player
        if (p1NameText) p1NameText.text = "PLAYER 1";
        if (p2NameText) p2NameText.text = "PLAYER 2";

        isRunning = true;
        StartCoroutine(GameRoutine());
    }

    // ====== GỌI TỪ MINIGAME MANAGER ĐỂ DỪNG ======
    public void StopMiniGame()
    {
        isRunning = false;
        roundActive = false;

        StopAllCoroutines();

        // Tắt carrier
        carrier1?.Deactivate();
        carrier2?.Deactivate();

        // Dọn bom
        if (flyCoroutine != null) StopCoroutine(flyCoroutine);
        if (bombInstance != null) Destroy(bombInstance);

        // Trao thưởng
        CheckFinishReward(manager.currentPlayer1);
        CheckFinishReward(manager.currentPlayer2);

        activePlayers.Clear();
    }

    // ====== GAME ROUTINE ======
    IEnumerator GameRoutine()
    {
        // ---- ĐẾM NGƯỢC 3,2,1 GO ----
        for (int i = (int)firstWaitTime; i > 0; i--)
        {
            if (countdownText) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        if (countdownText) countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        if (countdownText) countdownText.text = "";

        // ---- BẮT ĐẦU VÒNG ĐẦU ----
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
        roundActive = true;

        if (messageText) messageText.text = "";
    }

    private void Update()
    {
        if (!roundActive || !isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timerText) timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        // Bom nhấp nháy nhanh dần khi gần nổ
        UpdateBombBlink(timeLeft / roundDuration);

        // Tiếng tích tắc khi còn 5 giây
        if (timeLeft <= 5f && tickBombClip != null && audioSource != null)
        {
            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(tickBombClip);
        }

        if (timeLeft <= 0f)
            StartCoroutine(ExplodeBomb());
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
        // Tắt bom ở người cũ
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
        roundActive = false;
        if (flyCoroutine != null) StopCoroutine(flyCoroutine);

        // VFX nổ
        if (explosionVFX != null && currentBombHolder != null)
            Instantiate(explosionVFX, currentBombHolder.transform.position, Quaternion.identity);

        // SFX nổ
        if (explodeBombClip != null && audioSource != null)
            audioSource.PlayOneShot(explodeBombClip);

        if (bombInstance != null) bombInstance.SetActive(false);

        // Hiện tên người thua
        if (messageText != null && currentBombHolder != null)
            messageText.text = $"{currentBombHolder.name} bị loại!";

        // Animation chết
        PlayDeadAnimation(currentBombHolder.gameObject);

        // Loại player
        EliminatePlayer(currentBombHolder);

        yield return new WaitForSeconds(2f);

        StartNewRound();
    }

    void PlayDeadAnimation(GameObject playerObj)
    {
        PlayerAnimator playerAnimator = playerObj?.GetComponent<PlayerAnimator>();
        if (playerAnimator != null && playerAnimator.playerAnimator != null)
            playerAnimator.playerAnimator.SetBool("Die", true);

        PlayerMove move = playerObj?.GetComponent<PlayerMove>();
        if (move != null) move.isJumpAndMove = false;
    }

    void EliminatePlayer(BombCarrier carrier)
    {
        activePlayers.Remove(carrier);
        carrier.SetEliminated(true);
        carrier.Deactivate();
    }

    // ====== KẾT THÚC ======
    void EndGame()
    {
        roundActive = false;
        isRunning = false;

        if (resultPanel) resultPanel.SetActive(true);

        if (activePlayers.Count == 1)
        {
            string winnerName = activePlayers[0] == carrier1 ? "PLAYER 1" : "PLAYER 2";
            string color = activePlayers[0] == carrier1 ? "red" : "green";
            if (resultText)
                resultText.text = $"<color={color}>{winnerName} CHIẾN THẮNG!</color>";
            if (messageText)
                messageText.text = $"{winnerName} CHIẾN THẮNG!";
        }
        else
        {
            if (resultText) resultText.text = "<color=yellow>HÒA!</color>";
            if (messageText) messageText.text = "Hòa!";
        }

        CheckFinishReward(manager.currentPlayer1);
        CheckFinishReward(manager.currentPlayer2);
    }

    void CheckFinishReward(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerMiniGame mini = playerObj.GetComponent<PlayerMiniGame>();
        BombCarrier carrier = playerObj.GetComponent<BombCarrier>();
        if (mini == null || carrier == null) return;

        // Người không bị loại = thắng
        if (!carrier.IsEliminated)
            mini.UpCoin(1, 100);
        else
            mini.UpCoin(0, 100);
    }

    // ====== SETUP PLAYER (giống MiniGame4) ======
    void SetUpAllPlayer()
    {
        SetUpPlayer(manager.currentPlayer1);
        SetUpPlayer(manager.currentPlayer2);
    }

    void SetUpPlayer(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        PlayerAnimator anim = playerObj.GetComponent<PlayerAnimator>();

        if (move != null)
        {
            move.speed = 1f;
            move.isJumpAndMove = true;
            move.isWalk = true;
        }

        if (anim != null && anim.playerAnimator != null)
            anim.playerAnimator.SetBool("Die", false);
    }
}