using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    #region Singleton

    public static ShopManager _instance;
    public static ShopManager Instance => _instance;

    #endregion

    // =========================================================
    // INSPECTOR DATA
    // =========================================================

    #region Inspector

    [Header("Manager References")]
    public UIManager ui;
    public InputChooseItem inputChooseItem;

    [Header("Item Data")]
    public Sprite[] itemSprites;

    [Header("Players")]
    public PlayerManager[] players;

    [Header("Turn Timer")]
    public int timePerTurn = 20;

    [Header("Shop Settings")]
    public bool open = true;

    private bool[] canErrorCoin = { true, true };

    #endregion

    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    #region Private Variables

    private Coroutine turnTimerCoroutine;
    private int currentTime;

    #endregion

    // =========================================================
    // UNITY METHODS
    // =========================================================

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
    private void Start()
    {
        ui = UIManager.Instance;
        if (open) { Open(); }

    }
    #endregion

    // =========================================================
    // OPEN SHOP FLOW
    // =========================================================

    #region Open Shop Flow

    public void Open()
    {
        var cursor = CursorManager.Instance;
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openShopClip);

        SetupPlayers();

        SetupUI();

        OpenShop();
    }

    #endregion

    // =========================================================
    // SETUP
    // =========================================================

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

        GameManager.Instance.ResetBuffAllPlayer();
    }

    private void SetupUI()
    {
        UpdateCoin();

        foreach (Image img in ui.playerItemImagesList)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }

        ui.shopPanel?.SetActive(false);

        if (ui.canvasRandomCard != null)
            ui.canvasRandomCard.SetActive(false);

        if (ui.timerShopText != null)
            ui.timerShopText.text = timePerTurn.ToString();
    }

    #endregion

    // =========================================================
    // COIN
    // =========================================================

    #region Coin

    public void UpdateCoin()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null || ui.playerCoinTextList[i] == null)
                continue;

            ui.playerCoinTextList[i].text =
                players[i].playerCoin.coinEndMiniGame.ToString();
        }
    }

    private IEnumerator FlashCoinText(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= ui.playerCoinTextList.Length)
            yield break;

        TextMeshProUGUI text = ui.playerCoinTextList[playerIndex];

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

        canErrorCoin[playerIndex] = true;
    }

    #endregion

    // =========================================================
    // ITEM BUY / SHOW / RESET
    // =========================================================

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
            if (canErrorCoin[playerIndex])
            {
                canErrorCoin[playerIndex] = false;

                AudioManager.Instance.PlaySFX(AudioManager.Instance.noCoinBuyItemClip);
                StartCoroutine(FlashCoinText(playerIndex));
            }

            return false;
        }

        if (itemIndex == 1)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.openCardRamdomClip);
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItemClip);
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
        if (playerIndex < 0 || playerIndex >= ui.playerItemImagesList.Length)
            return;

        if (itemIndex < 0 || itemIndex >= itemSprites.Length)
            return;

        ui.playerItemImagesList[playerIndex].sprite = itemSprites[itemIndex];

        ui.playerItemImagesList[playerIndex].gameObject.SetActive(true);

        SendBuffToPlayer(playerIndex, itemIndex);
    }

    public void ResetShop()
    {
        foreach (Image img in ui.playerItemImagesList)
        {
            img.sprite = null;
            img.gameObject.SetActive(false);
        }
    }

    #endregion

    // =========================================================
    // SHOP OPEN / CLOSE
    // =========================================================

    #region Shop

    public void OpenShop()
    {
        StopTurnTimer();

        ui.shopPanel.SetActive(true);

        ResetShop();
        UpdateCoin();

        inputChooseItem.isPlayer1Choose = false;
        inputChooseItem.isPlayer2Choose = false;

        for (int i = 0; i < ui.itemsList.Length; i++)
        {
            ui.itemsList[i].transform.GetChild(1).gameObject.SetActive(false);
            ui.itemsList[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        if (ui.canvasRandomCard != null)
            ui.canvasRandomCard.SetActive(false);

        if (ui.timerShopText != null)
        {
            ui.timerShopText.text = "";
            ui.timerShopText.gameObject.SetActive(false);
        }

        CancelInvoke(nameof(StartPlayer1Turn));
        Invoke(nameof(StartPlayer1Turn), 1f);
    }

    private void StartPlayer1Turn()
    {
        StartCoroutine(Player1TurnRoutine());
    }

    private IEnumerator Player1TurnRoutine()
    {
        ShowPlayer1Turn();

        yield return new WaitForSeconds(0.6f);

        inputChooseItem.isPlayer1Choose = true;
        inputChooseItem.isPlayer2Choose = false;

        ui.itemsList[inputChooseItem.player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(true);

        StartTurnTimer(inputChooseItem.TimeOutPlayer1);
    }

    public void CloseShop()
    {
        StopTurnTimer();
        CancelInvoke(nameof(StartPlayer1Turn));

        if(ui.intructInputBuyPanel.activeSelf == true) SettingManager.Instance.ResetGuide();

        ui.shopPanel.SetActive(false);

        GameManager.Instance.CheckWinnerOrNextRound();
    }

    #endregion

    // =========================================================
    // TURN TIMER
    // =========================================================

    #region Turn Timer

    public void StartTurnTimer(Action onTimeOut)
    {
        StopTurnTimer();

        currentTime = timePerTurn;

        if (ui.timerShopText != null)
        {
            ui.timerShopText.gameObject.SetActive(true);
            ui.timerShopText.text = currentTime.ToString();
        }

        turnTimerCoroutine = StartCoroutine(TurnTimerRoutine(onTimeOut));
    }

    public void StopTurnTimer()
    {
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
            turnTimerCoroutine = null;
        }

        if (ui.timerShopText != null)
        {
            ui.timerShopText.text = "";
            ui.timerShopText.gameObject.SetActive(false);
        }
    }

    private IEnumerator TurnTimerRoutine(Action onTimeOut)
    {
        while (currentTime > 0)
        {
            if (ui.timerShopText != null)
                ui.timerShopText.text = currentTime.ToString();

            yield return new WaitForSeconds(1f);

            currentTime--;
        }

        if (ui.timerShopText != null)
        {
            ui.timerShopText.text = "";
            ui.timerShopText.gameObject.SetActive(false);
        }

        turnTimerCoroutine = null;

        onTimeOut?.Invoke();
    }

    #endregion

    // =========================================================
    // TURN PANEL
    // =========================================================

    #region Turn Panel

    public void ShowPlayer1Turn()
    {
        AudioManager.Instance.PlayUI(AudioManager.Instance.playerOneClip);
        StartCoroutine(ShowPlayerTurn(ui.panelPlayer1Turn));
    }

    public void ShowPlayer2Turn()
    {
        AudioManager.Instance.PlayUI(AudioManager.Instance.playerTwoClip);
        StartCoroutine(ShowPlayerTurn(ui.panelPlayer2Turn));
    }

    private IEnumerator ShowPlayerTurn(GameObject panel)
    {
        ui.panelPlayer1Turn.SetActive(false);
        ui.panelPlayer2Turn.SetActive(false);

        panel.SetActive(true);

        yield return new WaitForSeconds(0.9f);

        panel.SetActive(false);
    }

    #endregion

    // =========================================================
    // SEND BUFF
    // =========================================================

    #region Send Buff

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