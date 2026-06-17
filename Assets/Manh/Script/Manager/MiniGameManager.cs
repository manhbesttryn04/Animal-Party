using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using Unity.VisualScripting;

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

    public MapMiniGameList mapMiniGameList;

    [Header("Light Setting")]
    public Light light;
    public float startIntensity;


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
    // Tên minigame
    public TextMeshProUGUI textNameMiniGameMain;

    // UI hướng dẫn
    public GameObject canvasInstruct;

    // =========================================================
    // INSTRUCTION UI
    // =========================================================

    [Header("Instruction UI")]


    // Video hướng dẫn
    public VideoPlayer videoIntrucs;

    //Tên minigame
    public TextMeshProUGUI textNameMiniGame;
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
    public TransSpawPlayerList spawnPoint;

   

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
    public Material normalSkybox;
    public Material stormSkybox;

    [Header("Game State")]

    // Kiểm tra game đang chạy
    public bool isPlaying = false;

    // =========================================================
    // START
    // =========================================================


    // =========================================================
    // START MINIGAME
    // =========================================================

    private void Start()
    {
        
        if (light != null)
        {
            startIntensity = light.intensity;
        }
    }
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
        //set light theo minigame
        SetUpStartLightAndTime();
        // Đánh dấu game đang chạy
        isPlaying = true;
        mapMiniGameList.mapMiniGameList[indexMiniGame - 1].SetActive(true);

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
        //tat nhac
        AudioManager.Instance.StopMusic();
        //Tat bang game
        UIManager.Instance.HideNotifiPlayPanel(false);
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        //Mo am thanh minigame
        OpenMusicminiGame();

      

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
                 CharacterManager.Instance.playerPlaylist[CharacterManager.Instance.indexPlayer1],
                spawnPoint.transSpawPlayerList[indexMiniGame - 1].position,
                Quaternion.identity
            );

        // Spawn player 2
        currentPlayer2 =
            Instantiate(
                CharacterManager.Instance.playerPlaylist[CharacterManager.Instance.indexPlayer2],
              spawnPoint.transSpawPlayerList[indexMiniGame - 1].position + Vector3.right * 2f,
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
        //Lấy Player move
        PlayerMove move1 = currentPlayer1.GetComponent<PlayerMove>();
        PlayerMove move2 = currentPlayer2.GetComponent<PlayerMove>();
        //Khóa di chuyển
        yield return new WaitForSeconds(0.5f);
        if (move1 != null)
            move1.isJumpAndMove = false;

        if (move2 != null)
            move2.isJumpAndMove = false;

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
        p1.checkPoint = spawnPoint.transSpawPlayerList[indexMiniGame - 1];
        p2.checkPoint = spawnPoint.transSpawPlayerList[indexMiniGame - 1];

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

        // Set tên minigame
        textNameMiniGame.text =
            intrusTextList.nameMiniGameList[indexMiniGame - 1];

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
        // Mở di chuyển
        if (move1 != null)
            move1.isJumpAndMove = true;

        if (move2 != null)
            move2.isJumpAndMove = true;

        // =====================================================
        // SHOW GAME UI
        // =====================================================

        // Hiện UI minigame
        UIMiniGame.SetActive(true);

        // =====================================================
        // START MINIGAME LOGIC
        // =====================================================

        // Bắt đầu gameplay
        StartMiniGameByIndex();

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
        //Tatt am thanh
        AudioManager.Instance.StopMusic();

        // Stop gameplay
        ExitStopMiniGame();
        // Ẩn UI minigame
        UIMiniGame.SetActive(false);
        UIManager.Instance.UpdateResultPanel(
         coin1.coinMiniGame,
         coin2.coinMiniGame
     );

        yield return new WaitForSeconds(5f);
        UIManager.Instance.HideResultPanel();
        //Tra light
        SetupStopLightAndTime();
        // Hiện loading
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        LoadingManager.Instance.HideLoading();

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

       

        // =====================================================
        // CHECK WINNER
        // =====================================================

        // Kiểm tra người thắng
        GameManager.Instance.CheckPlayerWinRound(
            coin1.coinMiniGame,
            coin2.coinMiniGame
        );
        GameManager.Instance.ResetMagicDebuffAllPlayer();
        GameManager.Instance.ConvertBuffDiceAllPlayer();


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
        mapMiniGameList.mapMiniGameList[indexMiniGame - 1].SetActive(false);
       
       //     yield return StartCoroutine(LoadingManager.Instance.ShowLoading());

        AudioManager.Instance.PlaySFX(AudioManager.Instance.nextRound);

        // =====================================================
        // RESET

        // Reset trạng thái
        isPlaying = false;
        // =====================================================
        //Mo bang play
        UIManager.Instance.HideNotifiPlayPanel(true);
        //Mo nhac maingame
        AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMainClip);
    }

    // =========================================================
    // START MINIGAME LOGIC
    // =========================================================
    public void StartMiniGameByIndex()
    {
        if (indexMiniGame == 1)
        {
            miniGameList.miniGame1.StartMiniGame();
        }
        else if (indexMiniGame == 2)
        {
            miniGameList.miniGame2.StartMiniGame();
        }
        else if (indexMiniGame == 3)
        {
            miniGameList.miniGame3.StartMiniGame();
        }
        else if (indexMiniGame == 4)
        {
            miniGameList.miniGame4.StartMiniGame();
        }
        else if (indexMiniGame == 5)
        {
            miniGameList.miniGame5.StartMiniGame();
        }
        else if (indexMiniGame == 6)
        {

        }
        else if (indexMiniGame == 7)
        {

        }
        else if (indexMiniGame == 8)
        {

        }
        else if (indexMiniGame == 9)
        {

        }
        else if (indexMiniGame == 10)
        {

        }
    }

    // =========================================================
    // STOP MINIGAME LOGIC
    // =========================================================

    public void ExitStopMiniGame()
    {
        if (indexMiniGame == 1)
        {
            miniGameList.miniGame1.StopMiniGame();
        }
        else if (indexMiniGame == 2)
        {
            miniGameList.miniGame2.StopMiniGame();
        }
        else if (indexMiniGame == 3)
        {
            miniGameList.miniGame3.StopMiniGame();
        }
        else if (indexMiniGame == 4)
        {
           miniGameList.miniGame4.StopMiniGame();
        }
        else if (indexMiniGame == 5)
        {
            miniGameList.miniGame5.StopMiniGame();
        }
        else if (indexMiniGame == 6)
        {

        }
        else if (indexMiniGame == 7)
        {

        }
        else if (indexMiniGame == 8)
        {

        }
        else if (indexMiniGame == 9)
        {

        }
        else if (indexMiniGame == 10)
        {

        }
    }
    public void OpenMusicminiGame()
    {
       AudioManager.Instance.OpenMusicminiGame(indexMiniGame);
    }
    public void SetUpStartLightAndTime()
    {
        switch (indexMiniGame)
        {
            case 1:
              //  countDownTime = 60f;    
                break;

            case 2:
               // countDownTime = 60f;
                break;

            case 3:
                light.intensity = 0;
              //  countDownTime = 99f;
                break;

            case 4:
                light.intensity = 0.2f;
                ChangeToStormSky();
               // countDownTime = 99f;
                break;

            case 5:
                light.intensity = 0.2f;
               // countDownTime = 60f;
                break;

            case 6:
                break;

            case 7:
                break;

            case 8:
                break;

            case 9:
                break;

            case 10:
                break;

            default:
                break;
        }
    }
    public void SetupStopLightAndTime()
    {
        light.intensity = startIntensity;
        ChangeToNormalSky();
    }
    public void ChangeToStormSky()
    {
        RenderSettings.skybox = stormSkybox;
        DynamicGI.UpdateEnvironment();
    }

    public void ChangeToNormalSky()
    {
        RenderSettings.skybox = normalSkybox;
        DynamicGI.UpdateEnvironment();
    }
}