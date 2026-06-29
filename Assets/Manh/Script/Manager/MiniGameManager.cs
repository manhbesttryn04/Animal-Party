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
    public int indexMiniGame = 1;

    // =========================================================
    // DATA LIST
    // =========================================================

    [Header("MiniGame Data")]
    public MiniGameList miniGameList;
    public IntrusTextList intrusTextList;
    public VideoInstructList videoInstructList;
    public MapMiniGameList mapMiniGameList;

    public GameObject mainMap;

    public GameObject playersMain;

    // =========================================================
    // CAMERA
    // =========================================================

    [Header("Camera")]
    public CameraCutList miniGameCamera;

    // =========================================================
    // UI
    // =========================================================

    [Header("Main UI")]
    public GameObject UIMiniGame;
    public TextMeshProUGUI textNameMiniGameMain;

    public GameObject canvasInstruct;
    public GameObject blackPanel;

    // =========================================================
    // INSTRUCTION UI
    // =========================================================

    [Header("Instruction UI")]
    public VideoPlayer videoIntrucs;

    public TextMeshProUGUI textNameMiniGame;
    public TextMeshProUGUI textInstrucs;
    public TextMeshProUGUI textError;

    // =========================================================
    // TIMER UI
    // =========================================================

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;

    // =========================================================
    // COIN UI
    // =========================================================

    [Header("Coin UI")]
    public TextMeshProUGUI cointextPlayer1;
    public TextMeshProUGUI cointextPlayer2;

    // =========================================================
    // AVATAR UI
    // =========================================================

    [Header("Avatar UI")]
    public Image characterImagePlayer1;
    public Image characterImagePlayer2;

    // =========================================================
    // SPAWN
    // =========================================================

    [Header("Spawn")]
    public TransSpawPlayerList spawnPoint;

    // =========================================================
    // CURRENT PLAYERS
    // =========================================================

    [Header("Current Players")]
    public GameObject currentPlayer1;
    public GameObject currentPlayer2;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Countdown Time")]
    public float countDownTime = 99f;

    // =========================================================
    // STATE
    // =========================================================

    [Header("Game State")]
    public bool isPlaying = false;

    // =========================================================
    // START MINIGAME
    // =========================================================

    public void StartMiniGame()
    {
        // Nếu minigame đang chạy thì không cho chạy thêm lần nữa
        if (isPlaying)
            return;

        // Chạy toàn bộ quy trình minigame
        StartCoroutine(MiniGameRoutine());
    }

    // =========================================================
    // MAIN ROUTINE
    // =========================================================

    private IEnumerator MiniGameRoutine()
    {
        // =====================================================
        // CHECK INDEX TRƯỚC
        // =====================================================

        // Vì list dùng indexMiniGame - 1 nên indexMiniGame phải lớn hơn 0
        if (indexMiniGame <= 0)
        {
            Debug.LogError("Index MiniGame invalid!");
            yield break;
        }

        int miniGameIndex = indexMiniGame - 1;

        // =====================================================
        // CHECK DATA NULL
        // =====================================================

        if (mapMiniGameList == null)
        {
            Debug.LogError("MapMiniGameList is NULL!");
            yield break;
        }

        if (miniGameCamera == null)
        {
            Debug.LogError("MiniGameCamera is NULL!");
            yield break;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is NULL!");
            yield break;
        }

        if (miniGameList == null)
        {
            Debug.LogError("MiniGameList is NULL!");
            yield break;
        }

        // =====================================================
        // CHECK INDEX LIST
        // =====================================================

        if (miniGameIndex >= mapMiniGameList.mapMiniGameList.Count)
        {
            Debug.LogError("Map MiniGame index out of range!");
            yield break;
        }

        if (miniGameIndex >= miniGameCamera.cameraList.Count)
        {
            Debug.LogError("Camera index out of range!");
            yield break;
        }

        if (miniGameIndex >= spawnPoint.transSpawPlayerList.Count)
        {
            Debug.LogError("Spawn index out of range!");
            yield break;
        }

        // =====================================================
        // SETUP BAN ĐẦU
        // =====================================================

        // Set thời gian minigame theo index
        SetUpStartLightAndTime();

        // Đánh dấu minigame đang chạy
        isPlaying = true;

        // Tắt map chính
        mainMap.SetActive(false);
        playersMain.SetActive(false);

        // Bật map minigame theo index
        mapMiniGameList.mapMiniGameList[miniGameIndex].SetActive(true);

        // Tắt nhạc map chính
        AudioManager.Instance.StopMusic();

        // Ẩn bảng thông báo play
        UIManager.Instance.HideNotifiPlayPanel(false);

        // =====================================================
        // LOADING
        // =====================================================

        // Hiện loading
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());

        // Tắt loading
        LoadingManager.Instance.HideLoading();

        // Bật màn đen nếu muốn che cảnh lúc đổi camera
        if (blackPanel != null)
            blackPanel.SetActive(true);

        // Mở nhạc minigame
        OpenMusicMiniGame();

        // =====================================================
        // ENABLE CAMERA
        // =====================================================

        // Bật camera minigame
        miniGameCamera.cameraList[miniGameIndex].gameObject.SetActive(true);

        // =====================================================
        // ENABLE TIMER UI
        // =====================================================

        // Hiện text timer
        timerText.gameObject.SetActive(true);

        // =====================================================
        // SPAWN PLAYER
        // =====================================================

        Transform spawn = spawnPoint.transSpawPlayerList[miniGameIndex];

        // Spawn player 1 tại điểm spawn
        currentPlayer1 = Instantiate(
            CharacterManager.Instance.playerPlaylist[CharacterManager.Instance.indexPlayer1],
            spawn.position,
            Quaternion.identity
        );

        // Spawn player 2 lệch sang phải 2 đơn vị để không dính vào player 1
        currentPlayer2 = Instantiate(
            CharacterManager.Instance.playerPlaylist[CharacterManager.Instance.indexPlayer2],
            spawn.position + Vector3.right * 2f,
            Quaternion.identity
        );

        // =====================================================
        // GET COMPONENT PLAYER
        // =====================================================

        // Lấy thông tin avatar
        PlayerInfo avatar1 = currentPlayer1.GetComponent<PlayerInfo>();
        PlayerInfo avatar2 = currentPlayer2.GetComponent<PlayerInfo>();

        // Lấy coin
        PlayerCoin coin1 = currentPlayer1.GetComponent<PlayerCoin>();
        PlayerCoin coin2 = currentPlayer2.GetComponent<PlayerCoin>();

        // Lấy script minigame của player
        PlayerMiniGame p1 = currentPlayer1.GetComponent<PlayerMiniGame>();
        PlayerMiniGame p2 = currentPlayer2.GetComponent<PlayerMiniGame>();

        // Lấy PlayerType để đánh dấu player 2
        PlayerType player2Type = currentPlayer2.GetComponent<PlayerType>();

        // Lấy script di chuyển
        PlayerMove move1 = currentPlayer1.GetComponent<PlayerMove>();
        PlayerMove move2 = currentPlayer2.GetComponent<PlayerMove>();

        // =====================================================
        // CHECK COMPONENT
        // =====================================================

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

        // =====================================================
        // LOCK PLAYER MOVE
        // =====================================================

        // Chờ nửa giây để player spawn ổn định
        yield return new WaitForSeconds(0.5f);

        // Khóa di chuyển trước khi cutscene / hướng dẫn
        if (move1 != null)
            move1.isJumpAndMove = false;

        if (move2 != null)
            move2.isJumpAndMove = false;

        // =====================================================
        // SETUP PLAYER
        // =====================================================

        // Đánh dấu object thứ 2 là player 2
        player2Type.isPlayer2 = true;

        // Set checkpoint để khi rớt / chết thì respawn về spawn minigame
        p1.checkPoint = spawn;
        p2.checkPoint = spawn;

        // =====================================================
        // UPDATE UI BAN ĐẦU
        // =====================================================

        // Set avatar player 1
        characterImagePlayer1.sprite = avatar1.avatarCharacter;

        // Set avatar player 2
        characterImagePlayer2.sprite = avatar2.avatarCharacter;

        // Set coin ban đầu
        cointextPlayer1.text = coin1.coinMiniGame.ToString();
        cointextPlayer2.text = coin2.coinMiniGame.ToString();

        // =====================================================
        // PLAY CUTSCENE
        // =====================================================

        // Nếu minigame này có cutscene thì chạy cutscene
        if (miniGameCamera.MiniGameCameraList[miniGameIndex] != null)
        {
            yield return StartCoroutine(
                miniGameCamera.MiniGameCameraList[miniGameIndex].PlayCutscene()
            );
        }

        // =====================================================
        // SHOW INSTRUCTION
        // =====================================================

        // Hiện bảng hướng dẫn
        canvasInstruct.SetActive(true);

        // Set text hướng dẫn
        textInstrucs.text = intrusTextList.instructTextList[miniGameIndex];

        // Set tên minigame
        textNameMiniGame.text = intrusTextList.nameMiniGameList[miniGameIndex];

        // Set text lỗi / cảnh báo
        textError.text = intrusTextList.errorTextList[miniGameIndex];

        // Set video hướng dẫn
        videoIntrucs.clip = videoInstructList.videoInstructList[miniGameIndex];

        // Cho người chơi đọc hướng dẫn 5 giây
        yield return new WaitForSeconds(5f);

        // Tắt bảng hướng dẫn
        canvasInstruct.SetActive(false);

        // Tắt màn đen sau khi chuẩn bị xong
        if (blackPanel != null)
            blackPanel.SetActive(false);

        // =====================================================
        // UNLOCK PLAYER MOVE
        // =====================================================

        // Mở lại di chuyển
        if (move1 != null)
            move1.isJumpAndMove = true;

        if (move2 != null)
            move2.isJumpAndMove = true;

        // =====================================================
        // SHOW GAME UI
        // =====================================================

        // Hiện UI chính của minigame
        UIMiniGame.SetActive(true);

        // =====================================================
        // START MINIGAME LOGIC
        // =====================================================

        // Gọi StartMiniGame của minigame tương ứng
        StartMiniGameByIndex();

        // =====================================================
        // TIMER LOOP
        // =====================================================

        float timer = countDownTime;

        while (timer > 0)
        {
            // Giảm thời gian theo frame
            timer -= Time.deltaTime;

            // Làm tròn lên để timer không hiện 00:00 quá sớm
            int seconds = Mathf.CeilToInt(timer);

            // Tính phút
            int minutes = seconds / 60;

            // Tính giây còn lại
            int remainSeconds = seconds % 60;

            // Update timer UI dạng 00:00
            timerText.text =
                minutes.ToString("00") + ":" + remainSeconds.ToString("00");

            // Update coin realtime
            cointextPlayer1.text = coin1.coinMiniGame.ToString();
            cointextPlayer2.text = coin2.coinMiniGame.ToString();

            yield return null;
        }

        // =====================================================
        // TIME OUT
        // =====================================================

        // Khi hết giờ, ép timer về 00:00
        timerText.text = "00:00";

        // =====================================================
        // STOP MINIGAME
        // =====================================================

        // Tắt nhạc minigame
        AudioManager.Instance.StopMusic();

        // Gọi StopMiniGame của minigame hiện tại
        ExitStopMiniGame();

        // Ẩn UI minigame
        UIMiniGame.SetActive(false);

        // =====================================================
        // SHOW RESULT
        // =====================================================

        // Hiện bảng kết quả coin của 2 player
        UIManager.Instance.UpdateResultPanel(
            coin1.coinMiniGame,
            coin2.coinMiniGame
        );

        // Chờ 5 giây cho người chơi xem kết quả
        yield return new WaitForSeconds(5f);

        // Ẩn bảng kết quả
        UIManager.Instance.HideResultPanel();

        // =====================================================
        // LOADING BACK TO MAIN MAP
        // =====================================================

        // Hiện loading khi quay về map chính
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());

        playersMain.SetActive(true);
        // Bật lại map chính
        mainMap.SetActive(true);
        // riset lightt
        ResetLight();
        // tang
        SetIndex();

        // Tắt loading
        LoadingManager.Instance.HideLoading();

        // =====================================================
        // DISABLE CAMERA
        // =====================================================

        // Tắt camera minigame
        miniGameCamera.cameraList[miniGameIndex].gameObject.SetActive(false);

        // =====================================================
        // HIDE UI
        // =====================================================

        // Ẩn timer
        timerText.gameObject.SetActive(false);

        // =====================================================
        // CHECK WINNER
        // =====================================================

        // Kiểm tra ai thắng round dựa vào coin minigame
        GameManager.Instance.CheckPlayerWinRound(
            coin1.coinMiniGame,
            coin2.coinMiniGame
        );

        // Reset debuff phép
        GameManager.Instance.ResetMagicDebuffAllPlayer();

        // Convert buff xúc xắc nếu có
        GameManager.Instance.ConvertBuffDiceAllPlayer();

        // =====================================================
        // DESTROY PLAYER
        // =====================================================

        // Xóa player runtime trong minigame
        Destroy(currentPlayer1);
        Destroy(currentPlayer2);

        // =====================================================
        // DISABLE MAP MINIGAME
        // =====================================================

        // Tắt map minigame
        mapMiniGameList.mapMiniGameList[miniGameIndex].SetActive(false);

        // Phát âm thanh sang round tiếp theo
        AudioManager.Instance.PlaySFX(AudioManager.Instance.nextRound);

        // =====================================================
        // RESET STATE
        // =====================================================

        // Cho phép start minigame lần sau
        isPlaying = false;

        // Hiện lại bảng play ngoài map chính
        UIManager.Instance.HideNotifiPlayPanel(true);

        // Mở lại nhạc main map
        AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMainClip);
    }

    // =========================================================
    // START MINIGAME LOGIC BY INDEX
    // =========================================================

    public void StartMiniGameByIndex()
    {
        // Gọi logic StartMiniGame theo index hiện tại

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
            // Chưa có minigame 6
        }
        else if (indexMiniGame == 7)
        {
            // Chưa có minigame 7
        }
        else if (indexMiniGame == 8)
        {
            // Chưa có minigame 8
        }
        else if (indexMiniGame == 9)
        {
            // Chưa có minigame 9
        }
        else if (indexMiniGame == 10)
        {
            // Chưa có minigame 10
        }
    }

    // =========================================================
    // STOP MINIGAME LOGIC BY INDEX
    // =========================================================

    public void ExitStopMiniGame()
    {
        // Gọi logic StopMiniGame theo index hiện tại

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
            // Chưa có minigame 6
        }
        else if (indexMiniGame == 7)
        {
            // Chưa có minigame 7
        }
        else if (indexMiniGame == 8)
        {
            // Chưa có minigame 8
        }
        else if (indexMiniGame == 9)
        {
            // Chưa có minigame 9
        }
        else if (indexMiniGame == 10)
        {
            // Chưa có minigame 10
        }
    }

    // =========================================================
    // OPEN MUSIC MINIGAME
    // =========================================================

    public void OpenMusicMiniGame()
    {
        // Mở nhạc theo index minigame
        AudioManager.Instance.OpenMusicminiGame(indexMiniGame);
    }

    // =========================================================
    // SETUP TIME BY MINIGAME
    // =========================================================

    public void SetUpStartLightAndTime()
    {
        // Set thời gian chơi khác nhau theo từng minigame

        switch (indexMiniGame)
        {
            case 1:
                countDownTime = 60f;
                break;

            case 2:
                countDownTime = 60f;
                VolumeManager.Instance.SetBloomIntensity(1);
                break;

            case 3:
                countDownTime = 60f;
                break;

            case 4:
                countDownTime = 90;
                break;

            case 5:
                countDownTime = 20f;
                VolumeManager.Instance.SetBloomIntensity(0.5f);
                break;

            case 6:
                // Chưa set thời gian
                break;

            case 7:
                // Chưa set thời gian
                break;

            case 8:
                // Chưa set thời gian
                break;

            case 9:
                // Chưa set thời gian
                break;

            case 10:
                // Chưa set thời gian
                break;

            default:
                Debug.LogWarning("Index minigame chưa được setup thời gian!");
                break;
        }
    }
    //Riset Light 
    public void ResetLight()
    {
        VolumeManager.Instance.SetBloomIntensity(4);
    }

    public void SetIndex()
    {
        indexMiniGame++;
            if (indexMiniGame >= 6) indexMiniGame = 1;
    }
    
}