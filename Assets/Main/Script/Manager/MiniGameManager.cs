using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class MiniGameManager : MonoBehaviour
{
    #region INSPECTOR / STATE

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
    public TimeMinigame timeMinigame;
    public InstructInputMinigame inputMinigame;

    public GameObject mainMap;

    public GameObject playersMain;

    // =========================================================
    // CAMERA
    // =========================================================

    [Header("Camera")]
    public CameraCutList miniGameCamera;

    [Header("Video MiniGame")]
    public VideoPlayer videoIntrucs;

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
    public float timer;

    // =========================================================
    // STATE
    // =========================================================

    [Header("Game State")]
    public bool isPlaying = false;

    #endregion

    #region PUBLIC ENTRY

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

    #endregion

    #region MAIN MINIGAME FLOW

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
            // Debug.LogError("Index MiniGame invalid!");
            yield break;
        }

        int miniGameIndex = indexMiniGame - 1;

        // =====================================================
        // CHECK DATA NULL
        // =====================================================

        if (mapMiniGameList == null)
        {
            //Debug.LogError("MapMiniGameList is NULL!");
            yield break;
        }

        if (miniGameCamera == null)
        {
            // Debug.LogError("MiniGameCamera is NULL!");
            yield break;
        }

        if (spawnPoint == null)
        {
            // Debug.LogError("SpawnPoint is NULL!");
            yield break;
        }

        if (miniGameList == null)
        {
            //Debug.LogError("MiniGameList is NULL!");
            yield break;
        }

        // =====================================================
        // CHECK INDEX LIST
        // =====================================================

        if (miniGameIndex >= mapMiniGameList.mapMiniGameList.Count)
        {
            //Debug.LogError("Map MiniGame index out of range!");
            yield break;
        }

        if (miniGameIndex >= miniGameCamera.cameraList.Count)
        {
            //Debug.LogError("Camera index out of range!");
            yield break;
        }

        if (miniGameIndex >= spawnPoint.transSpawPlayerList.Count)
        {
            // Debug.LogError("Spawn index out of range!");
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

        // Bật map minigame theo index
        mapMiniGameList.mapMiniGameList[miniGameIndex].SetActive(true);

        // Tắt nhạc map chính
        AudioManager.Instance.PauseAudio();

        // Ẩn bảng thông báo play
        UIManager.Instance.HideNotifiPlayPanel(false);
        // SettingManager.Instance.ResetSetting();
        SettingManager.Instance.canOpenSettingByController = false;
        var cursor = CursorManager.Instance;
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActiveOpenSettingButton(false);
        }

        // =====================================================
        // LOADING
        // =====================================================

        // Hiện loading
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());

        // Tắt loading
        LoadingManager.Instance.HideLoading();

        // Bật màn đen nếu muốn che cảnh lúc đổi camera
        var ui = UIManager.Instance;
        ui.flastBlackPanel.SetActive(false);
        ui.flastBlackPanel.SetActive(true);

        // Mở nhạc minigame
        SetupMusicMiniGame();

        // =====================================================
        // ENABLE CAMERA
        // =====================================================

        // Bật camera minigame
        miniGameCamera.cameraList[miniGameIndex].gameObject.SetActive(true);

        // =====================================================
        // ENABLE TIMER UI
        // =====================================================

        // Hiện text timer
        // timerText.gameObject.SetActive(true);
        ui.timeMiniGameText.gameObject.SetActive(true);
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
        PlayerVFX vfx1 = currentPlayer1.GetComponent<PlayerVFX>();
        PlayerVFX vfx2 = currentPlayer2.GetComponent<PlayerVFX>();

        // =====================================================
        // CHECK COMPONENT
        // =====================================================

        if (vfx1 == null || vfx2 == null)
        {
            yield break;
        }
        if (avatar1 == null || avatar2 == null)
        {
            // Debug.LogError("PlayerInfo missing!");
            yield break;
        }

        if (coin1 == null || coin2 == null)
        {
            // Debug.LogError("PlayerCoin missing!");
            yield break;
        }

        if (p1 == null || p2 == null)
        {
            //Debug.LogError("PlayerMiniGame missing!");
            yield break;
        }

        if (player2Type == null)
        {
            // Debug.LogError("PlayerType missing!");
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

        if (vfx2 != null)
        {
            StartCoroutine(vfx2.DissolveInRoutine1());
        }
        if (vfx1 != null)
        {
            StartCoroutine(vfx1.DissolveInRoutine1());
        }
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
        // characterImagePlayer1.sprite = avatar1.avatarCharacter;
        ui.avatarP1.sprite = avatar1.avatarCharacter;

        // Set avatar player 2
        //characterImagePlayer2.sprite = avatar2.avatarCharacter;
        ui.avatarP2.sprite = avatar2.avatarCharacter;

        // Set coin ban đầu
        // cointextPlayer1.text = coin1.coinMiniGame.ToString();
        ui.coinMiniGameTextP1.text = coin1.coinMiniGame.ToString();

        // cointextPlayer2.text = coin2.coinMiniGame.ToString();
        ui.coinMiniGameTextP2.text = coin2.coinMiniGame.ToString();

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
        playersMain.SetActive(false);

        // =====================================================
        // SHOW INSTRUCTION
        // =====================================================

        // Hiện bảng hướng dẫn
        // canvasInstruct.SetActive(true);
        ui.canvasIntructGamePlay.SetActive(true);

        // Set text hướng dẫn
        // textInstrucs.text = intrusTextList.instructTextList[miniGameIndex];
        ui.instructGamePlayText.text = intrusTextList.instructTextList[miniGameIndex];

        // Set tên minigame
        // textNameMiniGame.text = intrusTextList.nameMiniGameList[miniGameIndex];
        ui.nameMiniGameText.text = intrusTextList.nameMiniGameList[miniGameIndex];

        // Set text lỗi / cảnh báo
        ui.errorGamePlayText.text = intrusTextList.errorTextList[miniGameIndex];

        // Set video hướng dẫn
        if (videoInstructList.videoInstructList[miniGameIndex] != null)
        {
            videoIntrucs.clip = videoInstructList.videoInstructList[miniGameIndex];
        }

        // Cho người chơi đọc hướng dẫn 5 giây
        yield return new WaitForSeconds(5f);

        videoIntrucs.Stop();
        videoIntrucs.time = 0;

        if (videoIntrucs.targetTexture != null)
        {
            videoIntrucs.targetTexture.Release();
        }
        videoIntrucs.clip = null;
        // Tắt bảng hướng dẫn
        ui.canvasIntructGamePlay.SetActive(false);

        ui.canvasInstructInput.SetActive(true);

        if (inputMinigame != null)
        {
            inputMinigame.ShowInputMinigame(indexMiniGame);
        }
        yield return new WaitForSeconds(5f);
        if (inputMinigame != null)
        {
            inputMinigame.HideAllInput();
        }
        ui.canvasInstructInput.SetActive(false);
        // Tắt màn đen sau khi chuẩn bị xong
        ui.flastBlackPanel.SetActive(false);
        UIManager.Instance.ActiveOpenSettingButton(true);
        if (cursor != null)
        {
            cursor.ShowGameCursor();
        }
        var setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.canOpenSettingByController = true;
        }

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
        ui.canvasMiniGame.SetActive(true);

        // =====================================================
        // START MINIGAME LOGIC
        // =====================================================

        // Gọi StartMiniGame của minigame tương ứng
        StartMiniGameByIndex();

        // =====================================================
        // TIMER LOOP
        // =====================================================

        timer = countDownTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            int seconds = Mathf.CeilToInt(timer);
            int minutes = seconds / 60;
            int remainSeconds = seconds % 60;

            ui.timeMiniGameText.text =
                minutes.ToString("00") + ":" +
                remainSeconds.ToString("00");

            ui.coinMiniGameTextP1.text =
                coin1.coinMiniGame.ToString();

            ui.coinMiniGameTextP2.text =
                coin2.coinMiniGame.ToString();

            yield return null;
        }

        // MiniGame 4: khi hết giờ, bắn chết các player chưa về đích
        // rồi mới cho MiniGameManager chạy tiếp phần kết quả.
        if (indexMiniGame == 4 &&
            miniGameList != null &&
            miniGameList.miniGame4 != null)
        {
            ui.timeMiniGameText.text = "00:00";

            miniGameList.miniGame4.BeginTimeoutSequence();

            float maxWaitTime = 20f;

            while (miniGameList.miniGame4.IsFinishingSequence &&
                   maxWaitTime > 0f)
            {
                ui.timeMiniGameText.text = "00:00";

                maxWaitTime -= Time.deltaTime;
                yield return null;
            }
        }

        // Chờ MiniGame7 xử lý xong cá mập,
        // thông báo thắng và giọng nói.
        if (indexMiniGame == 7 &&
            miniGameList != null &&
            miniGameList.miniGame7 != null)
        {
            float maxWaitTime = 10f;

            while (miniGameList.miniGame7.IsFinishingSequence &&
                   maxWaitTime > 0f)
            {
                ui.timeMiniGameText.text = "00:00";

                maxWaitTime -= Time.deltaTime;
                yield return null;
            }
        }

        // =====================================================
        // TIME OUT
        // =====================================================

        // Khi hết giờ, ép timer về 00:00
        ui.timeMiniGameText.text = "00:00";

        // =====================================================
        // STOP MINIGAME
        // =====================================================

        // Tắt nhạc minigame
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.ZeroAllAudio();

        // Gọi StopMiniGame của minigame hiện tại
        ExitStopMiniGame();

        // Ẩn UI minigame
        ui.canvasMiniGame.SetActive(false);

        // =====================================================
        // SHOW RESULT
        // =====================================================
        AudioManager.Instance.ZeroAllAudio();
        // SettingManager.Instance.ResetSetting();
        SettingManager.Instance.canOpenSettingByController = false;
        UIManager.Instance.ActiveOpenSettingButton(false);
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }

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
        AudioManager.Instance.SetupMainGameAudio();
        playersMain.SetActive(true);
        // Bật lại map chính
        mainMap.SetActive(true);
        // riset lightt
        ResetLight();
        // tang
        SetIndex();

        // Tắt loading
        LoadingManager.Instance.HideLoading();

        UIManager.Instance.ActiveOpenSettingButton(true);
        if (cursor != null)
        {

            cursor.ShowGameCursor();
        }
        if (setting != null)
        {
            setting.canOpenSettingByController = true;
        }

        // =====================================================
        // DISABLE CAMERA
        // =====================================================

        // Tắt camera minigame
        miniGameCamera.cameraList[miniGameIndex].gameObject.SetActive(false);

        // =====================================================
        // HIDE UI
        // =====================================================

        // Ẩn timer
        ui.timeMiniGameText.gameObject.SetActive(false);

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
        GameManager.Instance.PlayerTeleportToMain();

        // Convert buff xúc xắc nếu có
        GameManager.Instance.ConvertBuffDiceAllPlayer();

        // =====================================================
        // DESTROY PLAYER
        // =====================================================

        // Xóa player runtime trong minigame
        Destroy(currentPlayer1);
        Destroy(currentPlayer2);

        currentPlayer1 = null;
        currentPlayer2 = null;

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

    #endregion

    #region MINIGAME START / STOP DISPATCH

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
            miniGameList.miniGame6.StartMiniGame();
        }
        else if (indexMiniGame == 7)
        {
            miniGameList.miniGame7.StartMiniGame();
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

            miniGameList.miniGame6.StopMiniGame();
        }
        else if (indexMiniGame == 7)
        {
            miniGameList.miniGame7.StopMiniGame();
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

    #endregion

    #region AUDIO / TIME / LIGHT

    // =========================================================
    // OPEN MUSIC MINIGAME
    // =========================================================

    public void SetupMusicMiniGame()
    {
        // Mở nhạc theo index minigame
        AudioManager.Instance.SetupMusicMiniGame(indexMiniGame);
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
                countDownTime = timeMinigame.timeMinigame1;
                break;

            case 2:
                countDownTime = timeMinigame.timeMinigame2;

                VolumeManager.Instance.SetBloomIntensity(0.5f);
                break;

            case 3:
                countDownTime = timeMinigame.timeMinigame3;
                break;

            case 4:
                countDownTime = timeMinigame.timeMinigame4;
                break;

            case 5:
                countDownTime = timeMinigame.timeMinigame5;
                VolumeManager.Instance.SetBloomIntensity(1f);
                break;

            case 6:
                countDownTime = timeMinigame.timeMinigame6;
                break;

            case 7:
                countDownTime = timeMinigame.timeMinigame7;
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
                //Debug.LogWarning("Index minigame chưa được setup thời gian!");
                break;
        }
    }
    // =========================================================
    // RESET LIGHT
    // =========================================================

    public void ResetLight()
    {
        VolumeManager.Instance.SetBloomIntensity(4);
    }

    // =========================================================
    // SET INDEX
    // =========================================================

    public void SetIndex()
    {
        // indexMiniGame++;
        //if (indexMiniGame > 7) indexMiniGame = 1;
    }

    #endregion
}