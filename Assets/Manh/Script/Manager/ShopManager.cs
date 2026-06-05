using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    #region Singleton

    public static ShopManager _instance;
    public static ShopManager Instance => _instance;

    #endregion

    #region Inspector

    [Header("Item Data")]
    public Sprite[] itemSprites;

    [Header("Players")]
    public PlayerManager[] players;

    [Header("UI")]
    public GameObject canvasShop;
    public TextMeshProUGUI[] playerCoinTexts;
    public Image[] playerItemImages;
    public InputChooseItem inputChooseItem;
    public TextMeshProUGUI timerText;

    [Header("Random Card")]
    public GameObject canvasRandomCard;

    [Header("Turn Panels")]
    public GameObject panelPlayer1Turn;
    public GameObject panelPlayer2Turn;

    [Header("Shop Timer")]
    public int timeToBuy = 99;

    #endregion

    #region Private Variables

    private Coroutine shopTimerCoroutine;
    private int startTime;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
          
        }
        else
        {
            Destroy(gameObject);
        }
    }   



    public void Open()
    {
        SetupPlayers();
        SetupUI();
        OpenShop();
    }




    #endregion

    #region Setup

    private void SetupPlayers()
    {
        if (players == null || players.Length < 2)
            players = new PlayerManager[2];

        GameObject p1 = GameObject.FindGameObjectWithTag("Player 1");
        if (p1 != null)
            players[0] = p1.GetComponent<PlayerManager>();
        else
            Debug.LogError("Không tìm thấy Player 1");

        GameObject p2 = GameObject.FindGameObjectWithTag("Player 2");
        if (p2 != null)
            players[1] = p2.GetComponent<PlayerManager>();
        else
            Debug.LogError("Không tìm thấy Player 2");
        //RISET BUFF CHO TẤT CẢ NGƯỜI CHƠI TRƯỚC KHI MỞ SHOP
        GameManager.Instance.ResetBuffAllPlayer();
    }

    private void SetupUI()
    {
        UpdateCoin();

        foreach (Image img in playerItemImages)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }

        canvasShop?.SetActive(false);
    }

    #endregion

    #region Coin

    public void UpdateCoin()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null || playerCoinTexts[i] == null)
                continue;

            playerCoinTexts[i].text =
                players[i].playerCoin.coinEndMiniGame.ToString();
        }
    }

    private IEnumerator FlashCoinText(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerCoinTexts.Length)
            yield break;

        TextMeshProUGUI text = playerCoinTexts[playerIndex];

        if (text == null)
            yield break;

        Color originalColor = text.color;

        for (int i = 0; i < 3; i++)
        {
            text.color = Color.red;
            yield return new WaitForSeconds(0.08f);

            text.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
    }

    #endregion

    #region Item

    public bool BuyItem(int playerIndex, int itemIndex, int price)
    {
        if (playerIndex < 0 || playerIndex >= players.Length)
            return false;

        PlayerManager player = players[playerIndex];

        if (player == null)
            return false;

        if (player.playerCoin.coinEndMiniGame < price)
        {
            StartCoroutine(FlashCoinText(playerIndex));
            return false;
        }

        player.playerCoin.coinEndMiniGame -= price;

        UpdateCoin();

        if (itemIndex == 1)
            return true;

        ShowPlayerItem(playerIndex, itemIndex);

        return true;
    }

    public void ShowPlayerItem(int playerIndex, int itemIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerItemImages.Length)
            return;

        if (itemIndex < 0 || itemIndex >= itemSprites.Length)
            return;

        playerItemImages[playerIndex].sprite = itemSprites[itemIndex];
        playerItemImages[playerIndex].gameObject.SetActive(true);
        SendBuffToPlayer(playerIndex, itemIndex);
    }

    public void ResetShop()
    {
        foreach (Image img in playerItemImages)
        {
            img.sprite = null;
            img.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Shop

    public void OpenShop()
    {
        canvasShop.SetActive(true);

        ResetShop();
        UpdateCoin();

        Invoke("ShowPlayer1Turn", 2f);

        inputChooseItem.player1Index = 0;
        inputChooseItem.player2Index = 0;

        inputChooseItem.isPlayer1Choose = true;
        inputChooseItem.isPlayer2Choose = false;


        for (int i = 0; i < inputChooseItem.items.Length; i++)
        {
            inputChooseItem.items[i].transform.GetChild(1).gameObject.SetActive(false);
            inputChooseItem.items[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        inputChooseItem.items[0].transform.GetChild(1).gameObject.SetActive(true);

        StartShopTimer();
    }

    public void CloseShop()
    {
        if (shopTimerCoroutine != null)
        {
            StopCoroutine(shopTimerCoroutine);
            shopTimerCoroutine = null;
        }

        canvasShop.SetActive(false);
        GameManager.Instance.ExitNextRound();
    }

    #endregion

    #region Timer

    private void StartShopTimer()
    {
        startTime = timeToBuy;

        if (shopTimerCoroutine != null)
            StopCoroutine(shopTimerCoroutine);

        shopTimerCoroutine = StartCoroutine(ShopTimerRoutine());
    }

    private IEnumerator ShopTimerRoutine()
    {
        while (startTime > 0)
        {
            timerText.text = startTime.ToString();

            yield return new WaitForSeconds(1f);

            startTime--;
        }

        timerText.text = "0";

        CloseShop();
    }

    #endregion

    #region Turn Panel

    public void ShowPlayer1Turn()
    {
        StartCoroutine(ShowPlayerTurn(panelPlayer1Turn));
    }

    public void ShowPlayer2Turn()
    {
        StartCoroutine(ShowPlayerTurn(panelPlayer2Turn));
    }

    private IEnumerator ShowPlayerTurn(GameObject panel)
    {
        panelPlayer1Turn.SetActive(false);
        panelPlayer2Turn.SetActive(false);

        panel.SetActive(true);

        yield return new WaitForSeconds(0.9f);

        panel.SetActive(false);
    }
    public void SendBuffToPlayer(int playerIndex, int itemIndex)
    {
        if (playerIndex < 0 || playerIndex >= players.Length)
            return;

        PlayerBuff p = players[playerIndex].playerBuff;

        if (p == null)
            return;

        p.ApplyBuff(itemIndex);
    }

    #endregion
}