using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance { get; private set; }
    public GameObject resultPanel;
    public TextMeshProUGUI coinTextP1;
    public TextMeshProUGUI coinTextP2;
    public GameObject player1ResultUI;
    public GameObject player2ResultUI;

    public GameObject notifiPanel;
    public TextMeshProUGUI textNotifi;

    public GameObject notifiPlay;
    public GameObject notifiplayer1;
    public GameObject notifiplayer2;
    public TextMeshProUGUI coinTextNotP1;
    public TextMeshProUGUI coinTextNotP2;
    public TextMeshProUGUI indexTextP1;
    public TextMeshProUGUI indexTextP2;


    public PlayerManager playerManager1;
    public PlayerManager playerManager2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void Start()
    {
        playerManager1 = GameObject.FindGameObjectWithTag("Player 1").GetComponent<PlayerManager>();
        playerManager2 = GameObject.FindGameObjectWithTag("Player 2").GetComponent<PlayerManager>();

    }


    public void UpdateCoinPowerUI()
    {
        PlayerBuff p1 = playerManager1.playerBuff;
        PlayerBuff p2 = playerManager2.playerBuff;

        Transform coinRoot =
            notifiplayer1.transform
            .GetChild(2)
            .GetChild(0);

        Transform coinRoot2 =
            notifiplayer2.transform
            .GetChild(2)
            .GetChild(0);

        // Player 1
        for (int i = 0; i < coinRoot.childCount; i++)
        {
            Transform blackPanel =
                coinRoot.GetChild(i).GetChild(0);

            // Có coin => tắt panel đen
            // Chưa có coin => bật panel đen
            blackPanel.gameObject.SetActive(
                i >= p1.countCoinPower
            );
        }

        // Player 2
        for (int i = 0; i < coinRoot2.childCount; i++)
        {
            Transform blackPanel =
                coinRoot2.GetChild(i).GetChild(0);

            blackPanel.gameObject.SetActive(
                i >= p2.countCoinPower
            );
        }
    }
    public void UpdateCoinAllPlayer()
    {
        PlayerCoin p1 = playerManager1.playerCoin;
        PlayerCoin p2 = playerManager2.playerCoin;
        coinTextNotP1.text= p1.coinEndMiniGame.ToString();
       coinTextNotP2.text = p2.coinEndMiniGame.ToString();
    }
    public void UpdateIndexPlayerWalk()
    {
        PlayerMoveAI p1 = playerManager1.playerMoveAI;
        PlayerMoveAI p2 = playerManager2.playerMoveAI;
       indexTextP1.text = $"{p1.currentIndex}/22";
        indexTextP2.text = $"{p2.currentIndex}/22";


    }
    public void HidePlayerPlayPanel(bool i)
    {
        notifiPanel.gameObject.SetActive( i );
    }
    public void Update()
    {
        if (notifiPlay.activeSelf)
        {
            UpdateCoinPowerUI();
            UpdateCoinAllPlayer();
            UpdateIndexPlayerWalk();
        }
        else return;
      
    }

    public void UpdateResultPanel(int coinP1, int coinP2)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (coinTextP1 != null)
                coinTextP1.text = $"{coinP1}";
            if (coinTextP2 != null)
                coinTextP2.text = $"{coinP2}";
            if (coinP1 > coinP2)
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(true);

            }
            else if (coinP1 < coinP2)
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);

            }
        }
    }
    public void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    public void SendNotifi(string message)
    {
        if (notifiPanel != null && textNotifi != null)
        {
            notifiPanel.SetActive(true);
            textNotifi.text = message;
           
            Invoke(nameof(HideNotifi), 2f);
        }
    }

    private void HideNotifi()
    {
        if (notifiPanel != null)
        {
            notifiPanel.SetActive(false);
        }
    }
}