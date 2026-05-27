using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MiniGameManager : MonoBehaviour
{
    [Header("Index MiniGame")]

    // Index minigame hiện tại
    public int indexMiniGame = 1;

    // Danh sách minigame
    public MiniGameList miniGameList;

    public GameObject UIMiniGame;
    [Header("Camera")]

    // Danh sách camera + cutscene
    public CameraCutList miniGameCamera;

    [Header("Spawn")]

    // Vị trí spawn player
    public Transform spawnPoint;

    [Header("Player Prefab")]

    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Timer")]

    // UI timer
    public TextMeshProUGUI timerText;

    [Header("Coin Count")]

    // UI coin
    public TextMeshProUGUI cointextPlayer1;
    public TextMeshProUGUI cointextPlayer2;

    // UI avatar
    public Image characterImagePlayer1;
    public Image characterImagePlayer2;
    [Header("Loading UI")]
    public GameObject loadingCanvas;

    [Header("Countdown Time")]

    // Thời gian minigame
    public float countDownTime = 99f;

    [Header("Current Players")]

    // Player runtime
    public GameObject currentPlayer1;
    public GameObject currentPlayer2;

    // Kiểm tra game đang chơi
    public bool isPlaying = false;
    private void Start()
    {
        StartMiniGame();
    }
    public void StartMiniGame()
    {
        // Nếu game đang chạy thì không start nữa
        if (isPlaying)
            return;

        StartCoroutine(MiniGameRoutine());
    }

    IEnumerator MiniGameRoutine()
    {
        isPlaying = true;

        // =========================
        // CHECK INDEX
        // =========================

        if (indexMiniGame <= 0)
        {
            Debug.LogError("Index MiniGame invalid!");
            yield break;
        }

        // =========================
        // CHECK CAMERA LIST
        // =========================

        if (miniGameCamera == null)
        {
            Debug.LogError("MiniGameCamera is NULL!");
            yield break;
        }

        if (indexMiniGame - 1 >= miniGameCamera.cameraList.Count)
        {
            Debug.LogError("Camera index out of range!");
            yield break;
        }
        // =========================
        // LOADING
        // =========================

        loadingCanvas.SetActive(true);

        yield return new WaitForSeconds(3f);

        loadingCanvas.SetActive(false);

        // =========================
        // BẬT CAMERA
        // =========================

        miniGameCamera.cameraList[indexMiniGame - 1]
            .gameObject.SetActive(true);

        // =========================
        // HIỆN TIMER
        // =========================

        timerText.gameObject.SetActive(true);

        // =========================
        // SPAWN PLAYER
        // =========================

        currentPlayer1 =
            Instantiate(
                player1Prefab,
                spawnPoint.position,
                Quaternion.identity
            );

        currentPlayer2 =
            Instantiate(
                player2Prefab,
                spawnPoint.position + Vector3.right * 2f,
                Quaternion.identity
            );

        // =========================
        // PLAYER COMPONENT
        // =========================

        PlayerInfo avatar1 =
            currentPlayer1.GetComponent<PlayerInfo>();

        PlayerInfo avatar2 =
            currentPlayer2.GetComponent<PlayerInfo>();

        PlayerCoin coin1 =
            currentPlayer1.GetComponent<PlayerCoin>();

        PlayerCoin coin2 =
            currentPlayer2.GetComponent<PlayerCoin>();

        PlayerMiniGame p1 =
            currentPlayer1.GetComponent<PlayerMiniGame>();

        PlayerMiniGame p2 =
            currentPlayer2.GetComponent<PlayerMiniGame>();

        PlayerType player2Type =
            currentPlayer2.GetComponent<PlayerType>();

        // =========================
        // CHECK COMPONENT
        // =========================

        if (avatar1 == null || avatar2 == null)
        {
            Debug.LogError("PlayerInfo missing!");
            yield break;
        }

        if (coin1 == null || coin2 == null)
        {
            Debug.LogError("PlayerCoin missing!");
            yield break;
        }

        if (p1 == null || p2 == null)
        {
            Debug.LogError("PlayerMiniGame missing!");
            yield break;
        }

        if (player2Type == null)
        {
            Debug.LogError("PlayerType missing!");
            yield break;
        }

        // =========================
        // HIỆN AVATAR
        // =========================

        characterImagePlayer1.sprite =
            avatar1.avatarCharacter;

        characterImagePlayer2.sprite =
            avatar2.avatarCharacter;

        // =========================
        // HIỆN COIN
        // =========================

        cointextPlayer1.text =
            coin1.coinMiniGame.ToString();

        cointextPlayer2.text =
            coin2.coinMiniGame.ToString();

        // =========================
        // CHECKPOINT
        // =========================

        player2Type.isPlayer2 = true;

        p1.checkPoint = spawnPoint;
        p2.checkPoint = spawnPoint;

        // =========================
        // PLAY CUTSCENE
        // =========================

        if (miniGameCamera.MiniGameCameraList[indexMiniGame - 1] != null)
        {
            yield return StartCoroutine(
                miniGameCamera
                .MiniGameCameraList[indexMiniGame - 1]
                .PlayCutscene()
            );
        }
        UIMiniGame.SetActive( true );

        // =========================
        // START MINIGAME
        // =========================

        ExitStartMiniGame();

        // =========================
        // TIMER
        // =========================

        float timer = countDownTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            int seconds =
                Mathf.CeilToInt(timer);

            int minutes =
                seconds / 60;

            int remainSeconds =
                seconds % 60;

            timerText.text =
                minutes.ToString("00") +
                ":" +
                remainSeconds.ToString("00");

            // Update coin realtime
            cointextPlayer1.text =
                coin1.coinMiniGame.ToString();

            cointextPlayer2.text =
                coin2.coinMiniGame.ToString();

            yield return null;
        }

        // =========================
        // HẾT GIỜ
        // =========================

        timerText.text = "00:00";

        // =========================
        // STOP MINIGAME
        // =========================

        ExitStopMiniGame();

        // =========================
        // TẮT CAMERA
        // =========================

        miniGameCamera.cameraList[indexMiniGame - 1]
            .gameObject.SetActive(false);

        // =========================
        // ẨN TIMER
        // =========================

        timerText.gameObject.SetActive(false);
        UIMiniGame.SetActive(false);

        GameManager.Instance.CheckPlayerWinRound(coin1.coinMiniGame, coin2.coinMiniGame);

        // =========================
        // XOÁ PLAYER
        // =========================

        Destroy(currentPlayer1);

        Destroy(currentPlayer2);

        // =========================
        // RESET
        // =========================

        isPlaying = false;
    }

    // START MINIGAME LOGIC
    public void ExitStartMiniGame()
    {
        if (indexMiniGame == 1)
        {
            miniGameList.miniGame1.StartMiniGame();
        }
    }

    // STOP MINIGAME LOGIC
    public void ExitStopMiniGame()
    {
        if (indexMiniGame == 1)
        {
            miniGameList.miniGame1.StopMiniGame();
        }
    }
}