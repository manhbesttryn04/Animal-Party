using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Winner")]
    public string playerWinRound;
    public GameObject player1Main, player2Main;
    MiniGameManager miniGameManager;
    StateStoryGame stateGame;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            miniGameManager = FindAnyObjectByType<MiniGameManager>();
            stateGame = GetComponent<StateStoryGame>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player1Main = GameObject.FindGameObjectWithTag("Player 1");
        player2Main = GameObject.FindGameObjectWithTag("Player 2");
    }

    private void Update()
    {
        if (!stateGame.isFistRound)
        {
            FistRoundMiniGame();
           
        }
    }

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

     StartCoroutine(AddCoinToPlayer(playerCoin1, playerCoin2));
    }
    IEnumerator AddCoinToPlayer(int playerCoin1, int playerCoin2)
    {
        yield return new WaitForSeconds(2f);
        PlayerCoin player1 = player1Main.GetComponent<PlayerCoin>();
        PlayerCoin player2 = player2Main.GetComponent<PlayerCoin>();
        player2.AddCoin(playerCoin2);
        yield return new WaitForSeconds(2f);
        if(playerWinRound == "Player 1")
        {

        }else if(playerWinRound == "Player 2")
        {

        }
        else
        {

        }
    }
   
    public void JoinRandomMiniGame()
    {
        stateGame.isFistRound = true;
        miniGameManager.StartMiniGame();

    }
    public void FistRoundMiniGame()
    {
        PlayerRound r1 = player1Main.GetComponent<PlayerRound>();
        PlayerRound r2 = player2Main.GetComponent< PlayerRound>();
        if (r1.isRound1 && r2.isRound1)
        {
            JoinRandomMiniGame();
        }
    }
}