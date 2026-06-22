using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGame2 : MonoBehaviour
{
    [Header("Manager")]
    public MiniGameManager manager;
    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeDuration = 1.2f;
    public float shakeStrength = 0.25f;
    [Header("Audio")]
    public AudioSource source;
    public AudioClip brickFallClip;
    public AudioSource javaSource;


    [Header("Danh sách ô màu")]
    public List<ColorPad> allPads = new List<ColorPad>();

    [Header("UI Giao diện")]
    public GameObject canvasMiniGame;
    public Image targetColorImage;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;

    [Header("Start Delay")]
    public float startDelay = 3f;

    bool isRunning = false;

    private List<Color> easyColors = new List<Color>();
    private List<Color> mediumColors = new List<Color>();
    private List<Color> hardColors = new List<Color>();

    private List<Color> activeColorPool = new List<Color>();

    private Color targetColor;
    private int currentRound = 1;

    private void Awake()
    {
        InitializeColorPools();
    }

    public void StartMiniGame()
    {
        if (isRunning)
            return;
        if (canvasMiniGame != null)
        {
            canvasMiniGame.SetActive(true);
        }

        if (allPads.Count == 0)
        {
            Debug.LogError("Bạn chưa kéo các ô vào danh sách allPads!");
            return;
        }

        StartCoroutine(ColorGameLoop());
    }

    public void StopMiniGame()
    {
        isRunning = false;
        javaSource.enabled = false;
        StopAllCoroutines();

        if (canvasMiniGame != null)
        {
            canvasMiniGame.SetActive(false);
        }

        foreach (ColorPad pad in allPads)
        {
            if (pad != null)
                pad.ResetPad();
        }

        if (timerText != null)
            timerText.text = "";

        if (roundText != null)
            roundText.text = "";
    }

    void InitializeColorPools()
    {
        // CẤP ĐỘ DỄ: Còn 4 màu (Đã xóa Magenta và Cyan)
        easyColors.Add(Color.red);
        easyColors.Add(Color.blue);
        easyColors.Add(Color.yellow);
        easyColors.Add(Color.green);

        // CẤP ĐỘ TRUNG BÌNH: Lấy 4 màu trên cộng thêm 3 màu mới (Đã xóa 2 màu cuối là Xanh dương nhạt và Hồng nhạt)
        mediumColors.AddRange(easyColors);
        mediumColors.Add(new Color(1f, 0.5f, 0f));     // Màu Cam
        mediumColors.Add(new Color(0.5f, 0f, 0.5f));   // Màu Tím
        mediumColors.Add(new Color(0f, 0.5f, 0f));     // Màu Xanh lá đậm

        // CẤP ĐỘ KHÓ: Lấy tất cả màu trung bình cộng thêm 4 màu mới (Đã xóa 2 màu cuối là Xám và Xám nhạt)
        hardColors.AddRange(mediumColors);
        hardColors.Add(new Color(0.75f, 1f, 0f));      // Màu Chanh Tây (Lime)
        hardColors.Add(new Color(0f, 1f, 0.5f));       // Màu Xanh bạc hà (Mint)
        hardColors.Add(new Color(1f, 0.3f, 0.5f));     // Màu Hồng hạc (Flamingo)
        hardColors.Add(new Color(0.5f, 0.25f, 0f));    // Màu Nâu đất
    }

    IEnumerator ColorGameLoop()
    {
        isRunning = true;
        javaSource.enabled = true;
        currentRound = 1;

        yield return new WaitForSeconds(startDelay);

        while (isRunning)
        {
            if (roundText != null)
            {
                roundText.text = "Round:" + currentRound;
            }

            int safePadsCount = 8;
            float maxTimeForChoice = 5f;

            // Đã giữ nguyên logic chỉnh sửa lượt chơi của bạn
            if (currentRound >= 1 && currentRound <= 2)
            {
                activeColorPool = easyColors;
                safePadsCount = Random.Range(8, 12);
                maxTimeForChoice = 5f;
            }
            else if (currentRound >= 3 && currentRound <= 4)
            {
                activeColorPool = mediumColors;
                safePadsCount = Random.Range(4, 7);
                maxTimeForChoice = 4f;
            }
            else
            {
                activeColorPool = hardColors;
                safePadsCount = Random.Range(1, 3);
                maxTimeForChoice = 3f;
            }

            targetColor =
                activeColorPool[
                    Random.Range(
                        0,
                        activeColorPool.Count
                    )
                ];

            if (targetColorImage != null)
            {
                targetColorImage.color = targetColor;
            }

            List<ColorPad> shuffledPads =
                new List<ColorPad>(allPads);

            for (int i = 0; i < shuffledPads.Count; i++)
            {
                ColorPad temp = shuffledPads[i];

                int randomIndex =
                    Random.Range(
                        i,
                        shuffledPads.Count
                    );

                shuffledPads[i] =
                    shuffledPads[randomIndex];

                shuffledPads[randomIndex] = temp;
            }

            for (int i = 0; i < shuffledPads.Count; i++)
            {
                if (i < safePadsCount)
                {
                    shuffledPads[i].SetPadColor(
                        targetColor
                    );

                    shuffledPads[i].isSafe = true;
                }
                else
                {
                    Color randomColor;

                    do
                    {
                        randomColor =
                            activeColorPool[
                                Random.Range(
                                    0,
                                    activeColorPool.Count
                                )
                            ];
                    }
                    while (randomColor == targetColor);

                    shuffledPads[i].SetPadColor(
                        randomColor
                    );

                    shuffledPads[i].isSafe = false;
                }
            }

            float timeLeft = maxTimeForChoice;

            while (timeLeft > 0 && isRunning)
            {
                if (timerText != null)
                {
                    timerText.text =
                        Mathf.CeilToInt(timeLeft)
                        .ToString();
                }

                yield return new WaitForSeconds(1f);

                timeLeft -= 1f;
            }

            if (!isRunning)
                yield break;

            if (timerText != null)
            {
                timerText.text = "??";
            }

            // âm thanh sập
            source.PlayOneShot(brickFallClip);
            if (cameraShake != null)
            {
                cameraShake.Shake(shakeDuration, shakeStrength);
            }

            yield return new WaitForSeconds(0.5f);

            // ---- ĐÃ SỬA: QUÉT ĐẾM PLAYER THEO ĐỘ RỘNG HỘP TỐI ƯU HƠN ----
            foreach (ColorPad pad in allPads)
            {
                if (pad != null && pad.isSafe)
                {
                    int playerCountOnThisPad = CountPlayersOnPad(pad.gameObject);

                    if (playerCountOnThisPad >= 2)
                    {
                        pad.isSafe = false;
                       // Debug.Log($"<Color=Red>Ô {pad.gameObject.name} bị sập vì có {playerCountOnThisPad} Player cùng đứng!</Color>");
                    }
                }
            }

            // Làm sập các ô sai
            foreach (ColorPad pad in allPads)
            {
                pad.CheckSurvival();
            }

            // Chờ người chơi rơi
            yield return new WaitForSeconds(2f);

            // Hồi lại các ô
            foreach (ColorPad pad in allPads)
            {
                pad.ResetPad();
            }

            // Chờ animation hồi sàn hoàn tất
            yield return new WaitForSeconds(1f);

            // Respawn Player 1
            if (
                manager.currentPlayer1 != null &&
                manager.currentPlayer1.transform.position.y <= -10f
            )
            {
                PlayerMiniGame player1 =
                    manager.currentPlayer1.GetComponent<PlayerMiniGame>();

                if (player1 != null)
                {
                    player1.Respawn();
                }
            }

            // Respawn Player 2
            if (
                manager.currentPlayer2 != null &&
                manager.currentPlayer2.transform.position.y <= -10f
            )
            {
                PlayerMiniGame player2 =
                    manager.currentPlayer2.GetComponent<PlayerMiniGame>();

                if (player2 != null)
                {
                    player2.Respawn();
                }
            }

            // Chờ người chơi ổn định trên sàn
            yield return new WaitForSeconds(1f);

            // Sang vòng tiếp theo
            currentRound++;
        }
    }

    // ---- ĐÃ CẬP NHẬT: HÀM QUÉT ĐẾM KHÔNG BỊ SÓT VÀ KHÔNG KÉN TAG ----
    private int CountPlayersOnPad(GameObject padObj)
    {
        int count = 0;

        // Tăng chiều cao hộp quét lên (1.5f) và nới rộng ra sát viền ô (0.49f) để không sót Player đứng rìa
        Vector3 center = padObj.transform.position + new Vector3(0f, 1.0f, 0f);
        Vector3 halfExtents = new Vector3(0.49f, 1.0f, 0.49f);

        // Quét tất cả vật thể nằm trong phạm vi trên không của ô
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, padObj.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            // Bỏ qua nếu quét trúng chính cái ô sàn hoặc các ô sàn lân cận
            if (col.gameObject == padObj || col.gameObject.GetComponent<ColorPad>() != null)
                continue;

            // Nhận diện Player dựa trên bất kỳ script cốt lõi nào của nhân vật (PlayerMove, PlayerMiniGame, v.v.)
            bool isPlayer = col.CompareTag("Player") ||
                            col.GetComponent<PlayerMove>() != null ||
                            col.GetComponentInParent<PlayerMove>() != null ||
                            col.GetComponent<PlayerMiniGame>() != null;

            if (isPlayer)
            {
                count++;
            }
        }
        return count;
    }
}