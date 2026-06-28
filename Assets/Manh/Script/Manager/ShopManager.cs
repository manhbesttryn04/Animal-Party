using System.Collections;
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

    private bool[] canErrorCoin = { true, true };

    #endregion

    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    #region Private Variables

    // Coroutine đếm thời gian shop
    private Coroutine shopTimerCoroutine;

    // Thời gian bắt đầu đếm ngược
    private int startTime;

    #endregion

    // =========================================================
    // UNITY METHODS
    // =========================================================

    #region Unity Methods

    private void Awake()
    {
        // Tạo singleton cho ShopManager
        if (_instance == null)
        {
            _instance = this;

            // Không bị hủy khi đổi scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu đã có ShopManager thì xóa bản mới
            Destroy(gameObject);
        }
    }
    #endregion
  

    // =========================================================
    // OPEN SHOP FLOW
    // =========================================================

    #region Open Shop Flow

    public void Open()
    {
        // Phát âm thanh mở shop
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openShopClip);

        // Tìm và setup player
        SetupPlayers();

        // Setup lại UI shop
        SetupUI();

        // Mở shop
        OpenShop();
    }

    #endregion

    // =========================================================
    // SETUP
    // =========================================================

    #region Setup

    private void SetupPlayers()
    {
        // Nếu mảng players chưa có hoặc chưa đủ 2 phần tử thì tạo lại
        if (players == null || players.Length < 2)
            players = new PlayerManager[2];

        // Tìm Player 1 theo tag
        GameObject p1 = GameObject.FindGameObjectWithTag("Player 1");

        if (p1 != null)
            players[0] = p1.GetComponent<PlayerManager>();
        else
            Debug.LogError("Không tìm thấy Player 1");

        // Tìm Player 2 theo tag
        GameObject p2 = GameObject.FindGameObjectWithTag("Player 2");

        if (p2 != null)
            players[1] = p2.GetComponent<PlayerManager>();
        else
            Debug.LogError("Không tìm thấy Player 2");

        // Reset buff cho tất cả người chơi trước khi mở shop
        GameManager.Instance.ResetBuffAllPlayer();
    }

    private void SetupUI()
    {
        // Cập nhật coin hiện tại của player lên UI
        UpdateCoin();

        // Ẩn toàn bộ icon item đang hiển thị
        foreach (Image img in playerItemImages)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }

        // Tắt canvas shop trước khi mở lại
        canvasShop?.SetActive(false);
    }

    #endregion

    // =========================================================
    // COIN
    // =========================================================

    #region Coin

    public void UpdateCoin()
    {
        // Cập nhật số coin của từng player lên text UI
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
        // Kiểm tra index player hợp lệ
        if (playerIndex < 0 || playerIndex >= playerCoinTexts.Length)
            yield break;

        TextMeshProUGUI text = playerCoinTexts[playerIndex];

        if (text == null)
            yield break;

        // Lưu màu gốc của text coin
        Color originalColor = text.color;

        // Nhấp nháy đỏ 3 lần khi không đủ coin
        for (int i = 0; i < 3; i++)
        {
            text.color = Color.red;
            yield return new WaitForSeconds(0.08f);

            text.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
        // Cho phép phát lại âm thanh và hiệu ứng
        canErrorCoin[playerIndex] = true;
    }

    #endregion

    // =========================================================
    // ITEM BUY / SHOW / RESET
    // =========================================================

    #region Item

    public bool BuyItem(int playerIndex, int itemIndex, int price)
    {
        // Kiểm tra index player hợp lệ
        if (playerIndex < 0 || playerIndex >= players.Length)
            return false;

        PlayerManager player = players[playerIndex];

        if (player == null)
            return false;

        // Nếu không đủ coin thì báo lỗi và không mua
        if (player.playerCoin.coinEndMiniGame < price)
        {
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
            return false;

        }

        // Nếu itemIndex là 1 thì phát âm thanh mở card random
        if (itemIndex == 1)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.openCardRamdomClip);
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItemClip);
        }

        // Trừ tiền player sau khi mua
        player.playerCoin.coinEndMiniGame -= price;

        // Cập nhật lại UI coin
        UpdateCoin();

        // Nếu là random card thì trả true và không show item trực tiếp
        if (itemIndex == 1)
            return true;

        // Hiện item player đã mua
        ShowPlayerItem(playerIndex, itemIndex);

        return true;
    }

    public void ShowPlayerItem(int playerIndex, int itemIndex)
    {
        // Kiểm tra index player hợp lệ
        if (playerIndex < 0 || playerIndex >= playerItemImages.Length)
            return;

        // Kiểm tra index item hợp lệ
        if (itemIndex < 0 || itemIndex >= itemSprites.Length)
            return;

        // Gán sprite item vào UI của player
        playerItemImages[playerIndex].sprite = itemSprites[itemIndex];

        // Hiện icon item
        playerItemImages[playerIndex].gameObject.SetActive(true);

        // Gửi buff cho player tương ứng
        SendBuffToPlayer(playerIndex, itemIndex);
    }

    public void ResetShop()
    {
        // Reset icon item của cả 2 player
        foreach (Image img in playerItemImages)
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
        // Bật canvas shop
        canvasShop.SetActive(true);

        // Reset item hiển thị và cập nhật coin
        ResetShop();
        UpdateCoin();

        // Khóa input trước, không cho mua khi panel chưa hiện xong
        inputChooseItem.isPlayer1Choose = false;
        inputChooseItem.isPlayer2Choose = false;

        // Tắt toàn bộ highlight
        for (int i = 0; i < inputChooseItem.items.Length; i++)
        {
            inputChooseItem.items[i].transform.GetChild(1).gameObject.SetActive(false);
            inputChooseItem.items[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        // Sau 2 giây mới hiện panel Player 1
        Invoke(nameof(StartPlayer1Turn), 1f );

        // Bắt đầu đếm giờ shop
        StartShopTimer();
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

        inputChooseItem.items[inputChooseItem.player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        // Dừng timer shop nếu đang chạy
        if (shopTimerCoroutine != null)
        {
            StopCoroutine(shopTimerCoroutine);
            shopTimerCoroutine = null;
        }

        // Tắt shop
        canvasShop.SetActive(false);

        // Thoát shop và bắt đầu vòng tiếp theo
        GameManager.Instance.ExitNextRound();
    }



    #endregion

    // =========================================================
    // TIMER
    // =========================================================

    #region Timer

    private void StartShopTimer()
    {
        // Reset thời gian shop về thời gian ban đầu
        startTime = timeToBuy;

        // Nếu timer cũ đang chạy thì dừng lại
        if (shopTimerCoroutine != null)
            StopCoroutine(shopTimerCoroutine);

        // Chạy timer mới
        shopTimerCoroutine = StartCoroutine(ShopTimerRoutine());
    }

    private IEnumerator ShopTimerRoutine()
    {
        // Đếm ngược thời gian shop
        while (startTime > 0)
        {
            timerText.text = startTime.ToString();

            yield return new WaitForSeconds(1f);

            startTime--;
        }

        // Hết giờ thì set text về 0
        timerText.text = "0";

        // Đóng shop
        CloseShop();
    }

    #endregion

    // =========================================================
    // TURN PANEL
    // =========================================================

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
        // Tắt cả 2 panel trước
        panelPlayer1Turn.SetActive(false);
        panelPlayer2Turn.SetActive(false);

        // Bật panel lượt hiện tại
        panel.SetActive(true);

        yield return new WaitForSeconds(0.9f);

        // Tắt panel sau khi hiện xong
        panel.SetActive(false);
    }

    #endregion

    // =========================================================
    // SEND BUFF
    // =========================================================

    #region Send Buff

    public void SendBuffToPlayer(int playerIndex, int itemIndex)
    {
        // Kiểm tra index player hợp lệ
        if (playerIndex < 0 || playerIndex >= players.Length)
            return;

        PlayerBuff p = players[playerIndex].playerBuff;

        if (p == null)
            return;

        // Gửi buff theo itemIndex
        p.ApplyBuff(itemIndex);
    }

    #endregion
}