using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class MiniGameManager : MonoBehaviour
{
    // =========================================================
    // INDEX
    // =========================================================

    [Header("Index MiniGame")]

    // Index minigame hiện tại
    public int indexMiniGame = 1;

    // =========================================================
    // DATA LIST
    // =========================================================

    [Header("MiniGame Data")]

    // Danh sách logic minigame
    public MiniGameList miniGameList;

    // Danh sách text hướng dẫn
    public IntrusTextList intrusTextList;

    // Danh sách video hướng dẫn
    public VideoInstructList videoInstructList;

    // =========================================================
    // CAMERA
    // =========================================================

    [Header("Camera")]

    // Danh sách camera và cutscene
    public CameraCutList miniGameCamera;

    // =========================================================
    // UI
    // =========================================================

    [Header("Main UI")]

    // UI chính của minigame
    public GameObject UIMiniGame;

    // UI loading
    public GameObject loadingCanvas;

    // UI hướng dẫn
    public GameObject canvasInstruct;

    // =========================================================
    // INSTRUCTION UI
    // =========================================================

    [Header("Instruction UI")]

    // Video hướng dẫn
    public VideoPlayer videoIntrucs;

    // Text hướng dẫn gameplay
    public TextMeshProUGUI textInstrucs;

    // Text lỗi / cảnh báo
    public TextMeshProUGUI textError;

    // =========================================================
    // TIMER UI
    // =========================================================

    [Header("Timer UI")]

    // UI timer countdown
    public TextMeshProUGUI timerText;

    // =========================================================
    // COIN UI
    // =========================================================

    [Header("Coin UI")]

    // UI coin player 1
    public TextMeshProUGUI cointextPlayer1;

    // UI coin player 2
    public TextMeshProUGUI cointextPlayer2;

    // =========================================================
    // AVATAR UI
    // =========================================================

    [Header("Avatar UI")]

    // Avatar player 1
    public Image characterImagePlayer1;

    // Avatar player 2
    public Image characterImagePlayer2;

    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Spawn")]

    // Vị trí spawn player
    public Transform spawnPoint;

    [Header("Player Prefab")]

    // Prefab player 1
    public GameObject player1Prefab;

    // Prefab player 2
    public GameObject player2Prefab;

    // =========================================================
    // CURRENT PLAYER
    // =========================================================

    [Header("Current Players")]

    // Player 1 runtime
    public GameObject currentPlayer1;

    // Player 2 runtime
    public GameObject currentPlayer2;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Countdown Time")]

    // Thời gian minigame
    public float countDownTime = 99f;

    // =========================================================
    // STATE
    // =========================================================

    [Header("Game State")]

    // Kiểm tra game đang chạy
    public bool isPlaying = false;

    // =========================================================
    // START
    // =========================================================


    // =========================================================
    // START MINIGAME
    // =========================================================
 
    public void StartMiniGame()
    {
        // Nếu game đang chạy thì không start nữa
        if (isPlaying)
            return;

        // Chạy coroutine chính
        StartCoroutine(MiniGameRoutine());
    }

    // =========================================================
    // MAIN ROUTINE
    // =========================================================

    IEnumerator MiniGameRoutine()
    {
        // Đánh dấu game đang chạy
        isPlaying = true;

        // =====================================================
        // CHECK INDEX
        // =====================================================

        // Kiểm tra index hợp lệ
        if (indexMiniGame <= 0)
        {
            Debug.LogError("Index MiniGame invalid!");
            yield break;
        }

        // =====================================================
        // CHECK CAMERA
        // =====================================================

        // Kiểm tra camera list null
        if (miniGameCamera == null)
        {
            Debug.LogError("MiniGameCamera is NULL!");
            yield break;
        }

        // Kiểm tra index camera
        if (indexMiniGame - 1 >= miniGameCamera.cameraList.Count)
        {
            Debug.LogError("Camera index out of range!");
            yield break;
        }

        // =====================================================
        // LOADING
        // =====================================================

        // Hiện loading
        loadingCanvas.SetActive(true);

        // Delay loading
        yield return new WaitForSeconds(3f);

        // Tắt loading
        loadingCanvas.SetActive(false);

        // =====================================================
        // ENABLE CAMERA
        // =====================================================

        // Bật camera minigame
        miniGameCamera.cameraList[indexMiniGame - 1]
            .gameObject.SetActive(true);

        // =====================================================
        // ENABLE TIMER UI
        // =====================================================

        // Hiện timer
        timerText.gameObject.SetActive(true);

        // =====================================================
        // SPAWN PLAYER
        // =====================================================

        // Spawn player 1
        currentPlayer1 =
            Instantiate(
                player1Prefab,
                spawnPoint.position,
                Quaternion.identity
            );

        // Spawn player 2
        currentPlayer2 =
            Instantiate(
                player2Prefab,
                spawnPoint.position + Vector3.right * 2f,
                Quaternion.identity
            );

        // =====================================================
        // GET COMPONENT
        // =====================================================

        // Lấy PlayerInfo
        PlayerInfo avatar1 =
            currentPlayer1.GetComponent<PlayerInfo>();

        PlayerInfo avatar2 =
            currentPlayer2.GetComponent<PlayerInfo>();

        // Lấy PlayerCoin
        PlayerCoin coin1 =
            currentPlayer1.GetComponent<PlayerCoin>();

        PlayerCoin coin2 =
            currentPlayer2.GetComponent<PlayerCoin>();

        // Lấy PlayerMiniGame
        PlayerMiniGame p1 =
            currentPlayer1.GetComponent<PlayerMiniGame>();

        PlayerMiniGame p2 =
            currentPlayer2.GetComponent<PlayerMiniGame>();

        // Lấy PlayerType
        PlayerType player2Type =
            currentPlayer2.GetComponent<PlayerType>();

        // =====================================================
        // CHECK COMPONENT
        // =====================================================

        // Kiểm tra PlayerInfo
        if (avatar1 == null || avatar2 == null)
        {
            Debug.LogError("PlayerInfo missing!");
            yield break;
        }

        // Kiểm tra PlayerCoin
        if (coin1 == null || coin2 == null)
        {
            Debug.LogError("PlayerCoin missing!");
            yield break;
        }

        // Kiểm tra PlayerMiniGame
        if (p1 == null || p2 == null)
        {
            Debug.LogError("PlayerMiniGame missing!");
            yield break;
        }

        // Kiểm tra PlayerType
        if (player2Type == null)
        {
            Debug.LogError("PlayerType missing!");
            yield break;
        }

        // =====================================================
        // SETUP PLAYER
        // =====================================================

        // Đánh dấu player 2
        player2Type.isPlayer2 = true;

        // Set checkpoint
        p1.checkPoint = spawnPoint;
        p2.checkPoint = spawnPoint;

        // =====================================================
        // UPDATE UI
        // =====================================================

        // Update avatar
        characterImagePlayer1.sprite =
            avatar1.avatarCharacter;

        characterImagePlayer2.sprite =
            avatar2.avatarCharacter;

        // Update coin UI
        cointextPlayer1.text =
            coin1.coinMiniGame.ToString();

        cointextPlayer2.text =
            coin2.coinMiniGame.ToString();

        // =====================================================
        // PLAY CUTSCENE
        // =====================================================

        // Nếu có cutscene thì phát
        if (miniGameCamera.MiniGameCameraList[indexMiniGame - 1] != null)
        {
            yield return StartCoroutine(
                miniGameCamera
                .MiniGameCameraList[indexMiniGame - 1]
                .PlayCutscene()
            );
        }

        // =====================================================
        // SHOW INSTRUCTION
        // =====================================================

        // Hiện UI hướng dẫn
        canvasInstruct.SetActive(true);
        // Set text hướng dẫn
        textInstrucs.text =
            intrusTextList.instructTextList[indexMiniGame - 1];

        // Set text lỗi
        textError.text =
            intrusTextList.errorTextList[indexMiniGame - 1];

        // Set video hướng dẫn
        videoIntrucs.clip =
            videoInstructList.videoInstructList[indexMiniGame - 1];

        

        // Play video
        //videoIntrucs.Play();

        // Delay 5 giây
        yield return new WaitForSeconds(5f);

        // Tắt UI hướng dẫn
        canvasInstruct.SetActive(false);

        // =====================================================
        // SHOW GAME UI
        // =====================================================

        // Hiện UI minigame
        UIMiniGame.SetActive(true);

        // =====================================================
        // START MINIGAME LOGIC
        // =====================================================

        // Bắt đầu gameplay
        ExitStartMiniGame();

        // =====================================================
        // TIMER
        // =====================================================

        // Timer runtime
        float timer = countDownTime;

        // Loop timer
        while (timer > 0)
        {
            // Giảm timer
            timer -= Time.deltaTime;

            // Convert sang int
            int seconds =
                Mathf.CeilToInt(timer);

            // Tính phút
            int minutes =
                seconds / 60;

            // Tính giây
            int remainSeconds =
                seconds % 60;

            // Update UI timer
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

        // =====================================================
        // TIME OUT
        // =====================================================

        // Timer về 0
        timerText.text = "00:00";

        // =====================================================
        // STOP MINIGAME
        // =====================================================

        // Stop gameplay
        ExitStopMiniGame();

        // =====================================================
        // DISABLE CAMERA
        // =====================================================

        // Tắt camera minigame
        miniGameCamera.cameraList[indexMiniGame - 1]
            .gameObject.SetActive(false);

        // =====================================================
        // HIDE UI
        // =====================================================

        // Ẩn timer
        timerText.gameObject.SetActive(false);

        // Ẩn UI minigame
        UIMiniGame.SetActive(false);

        // =====================================================
        // CHECK WINNER
        // =====================================================

        // Kiểm tra người thắng
        GameManager.Instance.CheckPlayerWinRound(
            coin1.coinMiniGame,
            coin2.coinMiniGame
        );

        // =====================================================
        // DESTROY PLAYER
        // =====================================================

        // Xoá player 1
        Destroy(currentPlayer1);

        // Xoá player 2
        Destroy(currentPlayer2);

        // =====================================================
        // LOADING
        // =====================================================

        // Hiện loading
        loadingCanvas.SetActive(true);

        // Delay loading
        yield return new WaitForSeconds(3f);

        // Tắt loading
        loadingCanvas.SetActive(false);

        // =====================================================
        // RESET
        // =====================================================

        // Reset trạng thái
        isPlaying = false;
    }

    // =========================================================
    // START MINIGAME LOGIC
    // =========================================================

    public void ExitStartMiniGame()
    {
        // Nếu minigame 1
        if (indexMiniGame == 1)
        {
            // Start minigame 1
            miniGameList.miniGame1.StartMiniGame();
        }
    }

    // =========================================================
    // STOP MINIGAME LOGIC
    // =========================================================

    public void ExitStopMiniGame()
    {
        // Nếu minigame 1
        if (indexMiniGame == 1)
        {
            // Stop minigame 1
            miniGameList.miniGame1.StopMiniGame();
        }
    }
}