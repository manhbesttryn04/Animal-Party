using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Nếu dùng TextMeshPro. Nếu dùng UI.Text thường thì đổi lại using UnityEngine.UI;

public class BombGameManager : MonoBehaviour
{
    public static BombGameManager Instance;

    [Header("Người chơi")]
    public List<BombCarrier> players = new List<BombCarrier>(); // Kéo Player1, Player2 vào đây

    [Header("Cấu hình vòng chơi")]
    public float roundDuration = 15f;      // Thời gian mỗi vòng (giây) trước khi bom nổ
    public float passCooldown = 0.5f;      // Thời gian miễn nhiễm sau khi vừa nhận bom

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text messageText;

    [Header("Bomb Prefab")]
    public GameObject bombPrefab;
    public float bombFlyDuration = 0.25f;
    private GameObject bombInstance;
    private Renderer bombRenderer;
    private float blinkTimer = 0f;
    private Coroutine flyCoroutine;

    private BombCarrier currentBombHolder;
    private float timeLeft;
    private bool roundActive = false;

    void Start()
    {
        Instance = this;

        if (bombPrefab != null)
        {
            bombInstance = Instantiate(bombPrefab);
            bombRenderer = bombInstance.GetComponentInChildren<Renderer>();
            Debug.Log($"[BOMB] Đã spawn bomb prefab. Renderer tìm thấy: {(bombRenderer != null)}");
        }
        else
        {
            Debug.LogWarning("[BOMB] bombPrefab chưa được kéo vào BombGameManager trong Inspector!");
        }

        StartNewRound();
    }

    void Update()
    {
        if (!roundActive) return;

        timeLeft -= Time.deltaTime;
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        UpdateBombBlink(timeLeft / roundDuration);

        if (timeLeft <= 0f)
        {
            Explode();
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

    void StartNewRound()
    {
        if (players.Count <= 1)
        {
            EndGame();
            return;
        }

        int index = Random.Range(0, players.Count);
        AssignBomb(players[index]);

        timeLeft = roundDuration;
        roundActive = true;
        Debug.Log($"[ROUND] Bắt đầu vòng mới. {players[index].name} cầm bom.");
        if (messageText != null) messageText.text = "";
    }

    public void TransferBomb(BombCarrier from, BombCarrier to)
    {
        if (!roundActive) return;
        if (from != currentBombHolder) return;
        if (to.IsOnCooldown()) return;

        Debug.Log($"[BOMB] Truyền từ {from.name} sang {to.name}");
        AssignBomb(to);
    }

    void AssignBomb(BombCarrier holder)
    {
        if (currentBombHolder != null)
            currentBombHolder.SetHoldingBomb(false);

        currentBombHolder = holder;
        currentBombHolder.SetHoldingBomb(true);
        currentBombHolder.StartCooldown(passCooldown);

        if (bombInstance != null && holder.bombAnchor != null)
        {
            bombInstance.SetActive(true);

            if (flyCoroutine != null)
                StopCoroutine(flyCoroutine);

            flyCoroutine = StartCoroutine(FlyBombTo(holder.bombAnchor));
        }
        else
        {
            if (bombInstance == null)
                Debug.LogWarning("[BOMB] bombInstance NULL - kiểm tra Bomb Prefab đã kéo vào chưa!");
            if (holder.bombAnchor == null)
                Debug.LogWarning($"[BOMB] {holder.name} chưa có Bomb Anchor!");
        }
    }

    IEnumerator FlyBombTo(Transform targetAnchor)
    {
        bombInstance.transform.SetParent(null);

        Vector3 startPos = bombInstance.transform.position;
        Quaternion startRot = bombInstance.transform.rotation;
        float t = 0f;

        Debug.Log($"[BOMB FLY] Từ {startPos} -> {targetAnchor.name} tại {targetAnchor.position} (khoảng cách: {Vector3.Distance(startPos, targetAnchor.position):F2})");

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

    void Explode()
    {
        roundActive = false;

        if (flyCoroutine != null)
            StopCoroutine(flyCoroutine);

        if (bombInstance != null)
            bombInstance.SetActive(false);

        if (messageText != null)
            messageText.text = currentBombHolder.name + " đã bị loại!";

        EliminatePlayer(currentBombHolder);

        Invoke(nameof(StartNewRound), 2f);
    }

    void EliminatePlayer(BombCarrier player)
    {
        Debug.Log($"[ELIMINATED] {player.name} bị loại vì bom nổ.");
        players.Remove(player);
        player.SetEliminated(true);
    }

    void EndGame()
    {
        roundActive = false;
        if (players.Count == 1)
            Debug.Log($"[GAME OVER] {players[0].name} CHIẾN THẮNG!");
        else
            Debug.Log("[GAME OVER] Game kết thúc.");

        if (messageText != null)
        {
            if (players.Count == 1)
                messageText.text = players[0].name + " CHIẾN THẮNG!";
            else
                messageText.text = "Game kết thúc.";
        }
    }
}