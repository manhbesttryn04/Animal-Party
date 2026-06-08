using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGamePaint : MonoBehaviour
{
    [Header("Trạng thái quản lý Minigame")]
    public bool isPlaying = false;

    [Header("Danh sách các ô màu (Chính bạn tự đặt và kéo vào đây)")]
    public List<GameObject> allPadRenderers = new List<GameObject>();

    private List<PaintPadData> allPads = new List<PaintPadData>();

    [Header("Cấu hình màu sắc của 2 Player")]
    public Color player1Color = Color.blue;
    public Color player2Color = Color.red;

    [Header("UI Giao diện")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float gameDuration = 30f;

    void Awake()
    {
        InitializeManualPads();
    }

    void Start()
    {
        StartMiniGame();
    }

    void Update()
    {
        // Khi game đang chạy, liên tục quét kiểm tra vị trí Player trên toàn bộ các ô
        if (isPlaying)
        {
            foreach (PaintPadData pad in allPads)
            {
                pad.UpdateDetection();
            }
        }
    }

    void InitializeManualPads()
    {
        allPads.Clear();
        foreach (GameObject padObj in allPadRenderers)
        {
            if (padObj != null)
            {
                // Trả các BoxCollider về trạng thái sàn cứng mặc định để Player đứng vững
                BoxCollider col = padObj.GetComponent<BoxCollider>();
                if (col != null) col.isTrigger = false;

                MeshRenderer renderer = padObj.GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    PaintPadData data = new PaintPadData(padObj, renderer, this);
                    allPads.Add(data);
                    data.ResetColor();
                }
            }
        }
    }

    public void StartMiniGame()
    {
        if (isPlaying) return;

        isPlaying = true;
        if (resultText != null) resultText.text = "Trận đấu bắt đầu!";

        foreach (PaintPadData pad in allPads)
        {
            pad.ResetColor();
        }

        StartCoroutine(PaintGameRoutine());
    }

    public void StopMiniGame()
    {
        isPlaying = false;
        StopAllCoroutines();
        if (timerText != null) timerText.text = "-";
        Debug.Log("Minigame Tranh Màu đã dừng.");
    }

    IEnumerator PaintGameRoutine()
    {
        float timeLeft = gameDuration;

        while (timeLeft > 0 && isPlaying)
        {
            if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        isPlaying = false;
        if (timerText != null) timerText.text = "HẾT GIỜ!";

        CalculateFinalScore();
    }

    void CalculateFinalScore()
    {
        int p1Count = 0;
        int p2Count = 0;

        foreach (PaintPadData pad in allPads)
        {
            if (pad.ownerTag == "Player 1") p1Count++;
            else if (pad.ownerTag == "Player 2") p2Count++;
        }

        if (resultText != null)
        {
            if (p1Count > p2Count)
            {
                resultText.text = $"P1 THẮNG! ({p1Count} vs {p2Count})";
                resultText.color = player1Color;
            }
            else if (p2Count > p1Count)
            {
                resultText.text = $"P2 THẮNG! ({p2Count} vs {p1Count})";
                resultText.color = player2Color;
            }
            else
            {
                resultText.text = $"HÒA NHAU! ({p1Count} vs {p2Count})";
                resultText.color = Color.white;
            }
        }
    }

    public void OnPadTriggered(PaintPadData padData, GameObject playerObj)
    {
        PlayerType pType = playerObj.GetComponent<PlayerType>();
        if (pType != null)
        {
            if (!pType.isPlayer2)
            {
                padData.SetOwner("Player 1", player1Color);
            }
            else
            {
                padData.SetOwner("Player 2", player2Color);
            }
        }
    }
}

// ---- CLASS QUẢN LÝ DỮ LIỆU VÀ TỰ QUÉT NGƯỜI CHƠI TRÊN BỀ MẶT ----
public class PaintPadData
{
    public GameObject padObject;
    public MeshRenderer renderer;
    public string ownerTag = "";

    private MiniGamePaint manager;
    private Vector3 boxCenterOffset = new Vector3(0f, 0.6f, 0f); // Chiều cao vùng quét trên mặt ô
    private Vector3 boxHalfExtents = new Vector3(0.45f, 0.4f, 0.45f); // Kích thước hộp quét ngầm

    public PaintPadData(GameObject obj, MeshRenderer meshRenderer, MiniGamePaint gameManager)
    {
        padObject = obj;
        renderer = meshRenderer;
        manager = gameManager;
    }

    // Cơ chế quét vùng không gian phía trên ô để tìm Player (Không cần Rigidbody)
    public void UpdateDetection()
    {
        Vector3 centerPosition = padObject.transform.position + boxCenterOffset;

        // Quét tất cả vật thể nằm trong vùng không khí ngay trên mặt khối Cube
        Collider[] hitColliders = Physics.OverlapBox(centerPosition, boxHalfExtents, padObject.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject.GetComponent<PlayerType>() != null)
            {
                manager.OnPadTriggered(this, col.gameObject);
            }
        }
    }

    public void SetOwner(string tag, Color color)
    {
        ownerTag = tag;
        if (renderer != null) renderer.material.color = color;
    }

    public void ResetColor()
    {
        ownerTag = "";
        if (renderer != null) renderer.material.color = Color.white;
    }
}