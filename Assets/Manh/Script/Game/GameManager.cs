using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static GameManager instance;
    public static GameManager Instance => instance;

    // =========================================================
    // WINNER DATA
    // =========================================================

    [Header("Winner")]

    // Lưu tên người thắng vòng hiện tại
    public string playerWinRound;

    // Object chính của Player 1 và Player 2
    public GameObject player1Main, player2Main;

    // =========================================================
    // MANAGER REFERENCES
    // =========================================================

    // Quản lý mini game
    MiniGameManager miniGameManager;

    // Quản lý trạng thái vòng chơi / story game
    StateStoryGame stateGame;

    // Cho phép bắt đầu vòng tiếp theo hay chưa
    public bool canStartNextRound = false;

    public bool canCheckPlayer2 = true;
    public bool canCheckMiniGame = true;

    private float timer;

    // =========================================================
    // UNITY FUNCTIONS
    // =========================================================

    private void Awake()
    {
        // Tạo Singleton
        if (instance == null)
        {
            instance = this;

            // Không bị hủy khi chuyển scene
            DontDestroyOnLoad(gameObject);

            // Tìm MiniGameManager trong scene
            miniGameManager = FindAnyObjectByType<MiniGameManager>();

            // Lấy StateStoryGame trên cùng GameObject
            stateGame = GetComponent<StateStoryGame>();
        }
        else
        {
            // Nếu đã tồn tại GameManager thì xóa bản mới
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tìm Player 1 và Player 2 theo Tag
        player1Main = GameObject.FindGameObjectWithTag("Player 1");
        player2Main = GameObject.FindGameObjectWithTag("Player 2");
    }

    private void Update()
    {
        // Nếu chưa qua vòng đầu tiên thì kiểm tra điều kiện bắt đầu minigame đầu
        if (!stateGame.isFistRound)
        {
            FistRoundMiniGame();
        }
        timer += Time.deltaTime;

        if (timer < 1f)
            return;

        timer = 0f;

        // Chờ Player 1 đi xong để tới lượt Player 2
        if (canCheckPlayer2)
            WaitPlayer1RollDice();

        // Nếu cả 2 player đã xong lượt thì bắt đầu minigame tiếp theo
        if (canCheckMiniGame)
            StartNextRound();
    }

    // =========================================================
    // ROUND RESULT
    // =========================================================

    // Kiểm tra ai thắng vòng dựa trên coin kiếm được trong minigame
    public void CheckPlayerWinRound(int playerCoin1, int playerCoin2)
    {
        if (playerCoin1 > playerCoin2)
        {
            playerWinRound = "Player 1";
        }
        else if (playerCoin2 > playerCoin1)
        {
            playerWinRound = "Player 2";
        }
        else
        {
            playerWinRound = "Draw";
        }

        // Bắt đầu cộng coin và xử lý kết quả sau vòng
        StartCoroutine(AddCoinToPlayer(playerCoin1, playerCoin2));
    }

    IEnumerator AddCoinToPlayer(int playerCoin1, int playerCoin2)
    {
        // Chờ một chút trước khi cộng coin
        yield return new WaitForSeconds(2f);

        PlayerCoin player1 = player1Main.GetComponent<PlayerCoin>();
        PlayerCoin player2 = player2Main.GetComponent<PlayerCoin>();

        // Cộng coin từ minigame vào coin chính của từng player
        player1.AddCoinToPlayerMain(playerCoin1);
        player2.AddCoinToPlayerMain(playerCoin2);

        yield return new WaitForSeconds(2f);

        // =========================
        // PLAYER 1 THẮNG
        // =========================
        if (playerWinRound == "Player 1")
        {
            yield return StartCoroutine(
                CameraManager.Instance.MoveToTarget(
                    player1Main.transform,
                    1f
                )
            );

            // Mở debuff cho Player 1 chọn
            DebuffManager.Instance.OpenDebuffInternal(0);
        }

        // =========================
        // PLAYER 2 THẮNG
        // =========================
        else if (playerWinRound == "Player 2")
        {
            yield return StartCoroutine(
                CameraManager.Instance.MoveToTarget(
                    player2Main.transform,
                    1f
                )
            );

            // Mở debuff cho Player 2 chọn
            DebuffManager.Instance.OpenDebuffInternal(1);
        }

        // =========================
        // HÒA
        // =========================
        else
        {
            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(
                    15f,
                    1f
                )
            );

            // Nếu hòa thì mở shop
            ShopManager.Instance.Open();
        }
    }

    // =========================================================
    // MINI GAME CONTROL
    // =========================================================

    // Bắt đầu minigame ngẫu nhiên
    public void JoinRandomMiniGame()
    {
        // Đánh dấu đã qua vòng đầu
        if (!stateGame.isFistRound)
        {
            stateGame.isFistRound = true;
        }

        /*
        var randomIndex = Random.Range(0, 2);

        if(randomIndex == 0)
        {
            miniGameManager.indexMiniGame = 0 + 4;
        }
        else if(randomIndex == 1)
        {
            miniGameManager.indexMiniGame = 0 + 4;
        }
        */

        // Chạy minigame
        miniGameManager.StartMiniGame();
    }

    // Kiểm tra cả 2 người chơi đã tới vòng 1 chưa
    public void FistRoundMiniGame()
    {
        PlayerRound r1 = player1Main.GetComponent<PlayerRound>();
        PlayerRound r2 = player2Main.GetComponent<PlayerRound>();

        // Nếu cả 2 đều ở vòng 1 thì bắt đầu minigame đầu tiên
        if (r1.isRound1 && r2.isRound1)
        {
            JoinRandomMiniGame();
        }
    }

    // =========================================================
    // RESET BUFF / DEBUFF
    // =========================================================

    // Reset debuff phép của cả 2 player
    public void ResetMagicDebuffAllPlayer()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        if (p1 != null && p1.playerBuff != null)
        {
            p1.playerDebuff.ResetDebuff();
        }

        if (p2 != null && p2.playerBuff != null)
        {
            p2.playerDebuff.ResetDebuff();
        }
    }

    // Reset buff của cả 2 player
    public void ResetBuffAllPlayer()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        if (p1 != null && p1.playerBuff != null)
        {
            p1.playerBuff.ResetBuff();
        }

        if (p2 != null && p2.playerBuff != null)
        {
            p2.playerBuff.ResetBuff();
        }
    }

    // Chuyển buff xúc xắc của cả 2 player
    public void ConvertBuffDiceAllPlayer()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        if (p1 != null && p1.playerBuff != null)
        {
            p1.playerBuff.ConvertBuffDice();
        }

        if (p2 != null && p2.playerBuff != null)
        {
            p2.playerBuff.ConvertBuffDice();
        }
    }

    // =========================================================
    // GAME LOOP / NEXT ROUND
    // =========================================================

    // Reset trạng thái vòng chơi để chuẩn bị lượt mới
    public void ResetGameLoop()
    {
        canStartNextRound = false;
        stateGame.isNextRound = false;
        canCheckPlayer2 = true;
        canCheckMiniGame = true;

        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        p1.playerRound.ResetNextRound();
        p2.playerRound.ResetNextRound();

        // ConvertBuffDiceAllPlayer();
    }

    // Thoát màn hình next round và bắt đầu lượt Player 1 đổ xúc xắc
    public void ExitNextRound()
    {
        ResetGameLoop();

        StartCoroutine(Player1Dice());
    }

    // =========================================================
    // PLAYER 1 DICE TURN
    // =========================================================

    public IEnumerator Player1Dice()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();

        // Nếu Player 1 không bị debuff cấm roll dice
        if (!p1.playerDebuff.isNoRollDice)
        {
            yield return new WaitForSeconds(0.5f);

            // Camera follow Player 1
            p1.playerCamera.isFllow2 = true;

            yield return new WaitForSeconds(2f);

            // Tắt follow sau khi camera đã tới
            p1.playerCamera.isFllow2 = false;

            // Hiện thông báo roll dice
            yield return StartCoroutine(
                player1Main.GetComponent<PlayerManager>().playerNotifi.SetNotifi()
            );

            // Cho phép Player 1 bấm xúc xắc
            p1.GetComponent<PlayerManager>().playerInputDice.isClick = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);

            // Camera vẫn bay tới Player 1 để báo lượt
            p1.playerCamera.isFllow2 = true;

            yield return new WaitForSeconds(2f);

            p1.playerCamera.isFllow2 = false;

            yield return new WaitForSeconds(1f);

            // Bị cấm roll dice nên bỏ lượt và đánh dấu đã xong lượt
            p1.playerRound.nextRound = true;
        }
    }

    // =========================================================
    // PLAYER 2 DICE TURN
    // =========================================================

    public IEnumerator Player2Dice()
    {
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        // Nếu Player 2 không bị debuff cấm roll dice
        if (!p2.playerDebuff.isNoRollDice)
        {
            yield return new WaitForSeconds(0.5f);

            // Camera follow Player 2
            p2.playerCamera.isFllow2 = true;

            yield return new WaitForSeconds(2f);

            // Tắt follow sau khi camera đã tới
            p2.playerCamera.isFllow2 = false;

            // Hiện thông báo roll dice
            yield return StartCoroutine(
                player2Main.GetComponent<PlayerManager>().playerNotifi.SetNotifi()
            );

            // Cho phép Player 2 bấm xúc xắc
            p2.GetComponent<PlayerManager>().playerInputDice.isClick = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);

            // Camera vẫn bay tới Player 2 để báo lượt
            p2.playerCamera.isFllow2 = true;

            yield return new WaitForSeconds(2f);

            p2.playerCamera.isFllow2 = false;

            yield return new WaitForSeconds(1f);

            // Bị cấm roll dice nên bỏ lượt và đánh dấu đã xong lượt
            p2.playerRound.nextRound = true;
        }
    }

    // =========================================================
    // TURN FLOW CHECK
    // =========================================================

    // Chờ Player 1 đi xong, sau đó chuyển sang Player 2
    public void WaitPlayer1RollDice()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        if (
            p1.playerRound.nextRound &&
            !p2.playerRound.nextRound &&
            !stateGame.isNextRound
        )
        {
            canCheckPlayer2 = false;
            StartCoroutine(Player2Dice());

            // Khóa để không gọi Player2Dice liên tục trong Update
            stateGame.isNextRound = true;
        }
    }

    // Nếu cả 2 player đều xong lượt thì bắt đầu minigame tiếp theo
    public void StartNextRound()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();

        if (
            p1.playerRound.nextRound &&
            p2.playerRound.nextRound &&
            !canStartNextRound
        )
        {
            canCheckMiniGame = false;
            JoinRandomMiniGame();

            // Khóa để tránh gọi minigame nhiều lần trong Update
            canStartNextRound = true;
        }
    }
}