using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGame2 : MonoBehaviour
{
    [Header("Manager")]
    public MiniGameManager manager;

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
        easyColors.Add(Color.red);
        easyColors.Add(Color.blue);
        easyColors.Add(Color.yellow);
        easyColors.Add(Color.green);
        easyColors.Add(Color.magenta);
        easyColors.Add(Color.cyan);

        mediumColors.AddRange(easyColors);
        mediumColors.Add(new Color(1f, 0.5f, 0f));
        mediumColors.Add(new Color(0.5f, 0f, 0.5f));
        mediumColors.Add(new Color(0f, 0.5f, 0f));
        mediumColors.Add(new Color(0.6f, 0.8f, 1f));
        mediumColors.Add(new Color(1f, 0.75f, 0.8f));

        hardColors.AddRange(mediumColors);
        hardColors.Add(new Color(0.75f, 1f, 0f));
        hardColors.Add(new Color(0f, 1f, 0.5f));
        hardColors.Add(new Color(1f, 0.3f, 0.5f));
        hardColors.Add(new Color(0.5f, 0.25f, 0f));
        hardColors.Add(new Color(0.4f, 0.4f, 0.4f));
        hardColors.Add(new Color(0.85f, 0.85f, 0.85f));
    }

    IEnumerator ColorGameLoop()
    {
        isRunning = true;

        currentRound = 1;

        yield return new WaitForSeconds(startDelay);

        while (isRunning)
        {
            if (roundText != null)
            {
                roundText.text = "Lượt: " + currentRound;
            }

            int safePadsCount = 8;
            float maxTimeForChoice = 5f;

            if (currentRound >= 1 && currentRound <= 5)
            {
                activeColorPool = easyColors;
                safePadsCount = Random.Range(8, 12);
                maxTimeForChoice = 5f;
            }
            else if (currentRound >= 6 && currentRound <= 10)
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

            // Làm sập các ô sai
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
}