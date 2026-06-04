using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Winner")]

    // Lưu người thắng vòng hiện tại
    public string playerWinRound;

    // Object chính của Player 1 và Player 2
    public GameObject player1Main, player2Main;

    // Tham chiếu tới các manager khác
    MiniGameManager miniGameManager;
    StateStoryGame stateGame;
    public bool canStartNextRound = false;

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
        // Nếu chưa qua vòng đầu tiên
        if (!stateGame.isFistRound)
        {
            // Kiểm tra điều kiện bắt đầu minigame
            FistRoundMiniGame();
        }
        WaitPlayer1RollDice();
        StartNextRound();

    }

    // Kiểm tra ai thắng vòng
    public void CheckPlayerWinRound(int playerCoin1, int playerCoin2)
    {

        // Player 1 nhiều coin hơn
        if (playerCoin1 > playerCoin2)
        {
            playerWinRound = "Player 1";
        }
        // Player 2 nhiều coin hơn
        else if (playerCoin2 > playerCoin1)
        {
            playerWinRound = "Player 2";
        }
        // Hòa
        else
        {
            playerWinRound = "Draw";
        }

        // Bắt đầu cộng coin và xử lý kết quả
        StartCoroutine(AddCoinToPlayer(playerCoin1, playerCoin2));
    }

    IEnumerator AddCoinToPlayer(int playerCoin1, int playerCoin2)
    {
        yield return new WaitForSeconds(2f);

        PlayerCoin player1 =
            player1Main.GetComponent<PlayerCoin>();

        PlayerCoin player2 =
            player2Main.GetComponent<PlayerCoin>();

        player1.AddCoin(playerCoin1);
        player2.AddCoin(playerCoin2);

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

            ShopManager.Instance.Open();
        }
    }

    // Bắt đầu minigame ngẫu nhiên
    public void JoinRandomMiniGame()
    {
        // Đánh dấu đã qua vòng đầu
        if (!stateGame.isFistRound)
        {
            stateGame.isFistRound = true;
        }
         
        var randomIndex = Random.Range(0, 2);
            if(randomIndex == 0)
            {
                miniGameManager.indexMiniGame = 0
                +2;
        }
            else if(randomIndex == 1)
        {
                miniGameManager.indexMiniGame = 0
                +2;
        }
        // Chạy minigame
        miniGameManager.StartMiniGame();
    }

    // Kiểm tra cả 2 người đã tới vòng 1 chưa
    public void FistRoundMiniGame()
    {
        // Lấy thông tin vòng chơi
        PlayerRound r1 = player1Main.GetComponent<PlayerRound>();
        PlayerRound r2 = player2Main.GetComponent<PlayerRound>();

        // Nếu cả 2 đều ở vòng 1
        if (r1.isRound1 && r2.isRound1)
        {
            // Bắt đầu minigame
            JoinRandomMiniGame();
        }
    }

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
    public void ResetGameLoop()
    {
        canStartNextRound = false;
        stateGame.isNextRound = false;

        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();
        p1.playerRound.ResetNextRound();
        p2.playerRound.ResetNextRound();

    }
    public void ExitNextRound()
    {
        ResetGameLoop();

        StartCoroutine(Player1Dice());
    }

    public IEnumerator Player1Dice()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        if (!p1.playerDebuff.isNoRollDice)
        {
            yield return new WaitForSeconds(0.5f);
            p1.playerCamera.isFllow2 = true;
            yield return new WaitForSeconds(2f);
            p1.playerCamera.isFllow2 = false;
            StartCoroutine(player1Main.GetComponent<PlayerManager>().playerNotifi.SetNotifi());
            p1.GetComponent<PlayerManager>().playerInputDice.isClick = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            p1.playerCamera.isFllow2 = true;
            yield return new WaitForSeconds(2f);
            p1.playerCamera.isFllow2 = false;
            yield return new WaitForSeconds(1f);
            p1.playerRound.nextRound = true;
        }


    }
    public IEnumerator Player2Dice()
    {
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();
        if (!p2.playerDebuff.isNoRollDice)
        {
            yield return new WaitForSeconds(0.5f);
            p2.playerCamera.isFllow2 = true;
            yield return new WaitForSeconds(2f);
            p2.playerCamera.isFllow2 = false;
            StartCoroutine(player2Main.GetComponent<PlayerManager>().playerNotifi.SetNotifi());
            p2.GetComponent<PlayerManager>().playerInputDice.isClick = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            p2.playerCamera.isFllow2 = true;
            yield return new WaitForSeconds(2f);
            p2.playerCamera.isFllow2 = false;
            yield return new WaitForSeconds(1f);
            p2.playerRound.nextRound = true;
        }

    }

    public void WaitPlayer1RollDice()
    {
       PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
       PlayerManager p2 = player2Main.GetComponent<PlayerManager>();
      
        if(p1.playerRound.nextRound && !p2.playerRound.nextRound&& !stateGame.isNextRound)
        {
           
            StartCoroutine(Player2Dice());
            stateGame.isNextRound = true;
        }
    }
    public void StartNextRound()
    {
        PlayerManager p1 = player1Main.GetComponent<PlayerManager>();
        PlayerManager p2 = player2Main.GetComponent<PlayerManager>();
        if(p1.playerRound.nextRound && p2.playerRound.nextRound&& !canStartNextRound)
        {
            JoinRandomMiniGame();
            canStartNextRound = true;
        }
    }
}
