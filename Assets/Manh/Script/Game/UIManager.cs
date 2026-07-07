using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    // Singleton để các script khác có thể gọi UIManager.Instance
    public static UIManager Instance { get; private set; }

    // =========================================================
    // RESULT PANEL
    // =========================================================
    public GameObject uiMain;

    #region Result Panel

    // Panel hiện kết quả cuối game
    public GameObject resultPanel;

    // Text hiển thị số coin của Player 1 và Player 2
    public TextMeshProUGUI coinTextP1;
    public TextMeshProUGUI coinTextP2;

    // UI kết quả của từng người chơi
    public GameObject player1ResultUI;
    public GameObject player2ResultUI;

    #endregion

    // =========================================================
    // NOTIFICATION
    // =========================================================

    #region Notification

    // Panel thông báo
    public GameObject notifiPanel;

    // Nội dung thông báo
    public TextMeshProUGUI textNotifi;

    #endregion

    // =========================================================
    // PLAYER STATUS PANEL
    // =========================================================

    #region Player Status Panel

    // Panel hiển thị thông tin trong lúc chơi
    public GameObject notifiPlay;

    // Khung thông tin của từng người chơi
    public GameObject notifiplayer1;
    public GameObject notifiplayer2;

    // Coin hiện tại của từng người
    public TextMeshProUGUI coinTextNotP1;
    public TextMeshProUGUI coinTextNotP2;

    // Vị trí hiện tại trên bàn cờ
    public TextMeshProUGUI indexTextP1;
    public TextMeshProUGUI indexTextP2;

    // Ảnh buff hiện tại của Player 1 và Player 2
    public Image imageCurrentBuffP1;
    public Image imageCurrentBuffP2;

    // Danh sách sprite buff
    public List<Sprite> buffImageList;

    // Bonus Coin UI
    public GameObject bonusCoinTextP1;
    public GameObject bonusCoinTextP2;
   
    public GameObject blackPanel;

    #endregion

    // =========================================================
    // BONUS PANEL
    // =========================================================

    // Panel thưởng
    public GameObject bonusPanel;

    // =========================================================
    // PLAYER REFERENCES
    // =========================================================

    // Tham chiếu tới PlayerManager
    public PlayerManager playerManager1;
    public PlayerManager playerManager2;

    // =========================================================
    // UNITY FUNCTIONS
    // =========================================================
    public float updateUITime = 0.25f;
    private float updateTimer;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        // Tìm Player 1 theo Tag
        playerManager1 =
            GameObject.FindGameObjectWithTag("Player 1")
            .GetComponent<PlayerManager>();

        // Tìm Player 2 theo Tag
        playerManager2 =
            GameObject.FindGameObjectWithTag("Player 2")
            .GetComponent<PlayerManager>();
        UpdateAllPlayMainUI();
    }

    private void Update()
    {
        if (!notifiPlay.activeSelf) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateUITime)
        {
            updateTimer = 0f;
            UpdateAllPlayMainUI();
        }
    }

    public void UpdateAllPlayMainUI()
    {
        UpdateCoinPowerUI();
        UpdateCoinAllPlayer();
        UpdateIndexPlayerWalk();
        UpdateCurrentBuffPlayer();
    }


    // =========================================================
    // COIN POWER UI
    // =========================================================

    /// <summary>
    /// Cập nhật số lượng Coin Power hiển thị trên UI.
    /// Coin chưa có sẽ bị che bởi panel màu đen.
    /// </summary>
    public void UpdateCoinPowerUI()
    {
        PlayerBuff p1 = playerManager1.playerBuff;
        PlayerBuff p2 = playerManager2.playerBuff;

        // Root chứa icon Coin Power Player 1
        Transform coinRoot =
            notifiplayer1.transform
            .GetChild(2)
            .GetChild(0);

        // Root chứa icon Coin Power Player 2
        Transform coinRoot2 =
            notifiplayer2.transform
            .GetChild(2)
            .GetChild(0);

        // =========================
        // PLAYER 1 COIN POWER
        // =========================

        for (int i = 0; i < coinRoot.childCount; i++)
        {
            // Panel đen che icon
            Transform blackPanel =
                coinRoot.GetChild(i).GetChild(0);

            // Nếu đã có Coin Power thì tắt panel đen
            blackPanel.gameObject.SetActive(i >= p1.countCoinPower);
        }

        // =========================
        // PLAYER 2 COIN POWER
        // =========================

        for (int i = 0; i < coinRoot2.childCount; i++)
        {
            // Panel đen che icon
            Transform blackPanel =
                coinRoot2.GetChild(i).GetChild(0);

            // Nếu đã có Coin Power thì tắt panel đen
            blackPanel.gameObject.SetActive(i >= p2.countCoinPower);
        }
    }

    // =========================================================
    // COIN UI
    // =========================================================

    /// <summary>
    /// Cập nhật số coin của cả hai người chơi.
    /// </summary>
    public void UpdateCoinAllPlayer()
    {
        PlayerCoin p1 = playerManager1.playerCoin;
        PlayerCoin p2 = playerManager2.playerCoin;

        coinTextNotP1.text = p1.coinEndMiniGame.ToString();
        coinTextNotP2.text = p2.coinEndMiniGame.ToString();
    }

    // =========================================================
    // PLAYER BOARD INDEX UI
    // =========================================================

    /// <summary>
    /// Cập nhật vị trí hiện tại trên bàn cờ.
    /// </summary>
    public void UpdateIndexPlayerWalk()
    {
        PlayerMoveAI p1 = playerManager1.playerMoveAI;
        PlayerMoveAI p2 = playerManager2.playerMoveAI;

        indexTextP1.text = $"{p1.currentIndex +1}/33";
        indexTextP2.text = $"{p2.currentIndex +1}/33";
    }

    // =========================================================
    // CURRENT BUFF UI
    // =========================================================

    public void UpdateCurrentBuffPlayer()
    {
        PlayerBuff p1 = playerManager1.playerBuff;
        PlayerBuff p2 = playerManager2.playerBuff;

        // =========================
        // PLAYER 1 CURRENT BUFF
        // =========================

        if (p1.isBuffDeffense)
        {
            imageCurrentBuffP1.sprite = buffImageList[1];
        }
        else if (p1.isBuffMagic)
        {
            imageCurrentBuffP1.sprite = buffImageList[2];
        }
        else if (p1.isBuffCanon)
        {
            imageCurrentBuffP1.sprite = buffImageList[3];
        }
        else if (p1.isBuffDice > 0 || p1.isBuffDiceNext > 0)
        {
            imageCurrentBuffP1.sprite = buffImageList[4];
        }
        else
        {
            imageCurrentBuffP1.sprite = buffImageList[0];
        }

        // =========================
        // PLAYER 2 CURRENT BUFF
        // =========================

        if (p2.isBuffDeffense)
        {
            imageCurrentBuffP2.sprite = buffImageList[1];
        }
        else if (p2.isBuffMagic)
        {
            imageCurrentBuffP2.sprite = buffImageList[2];
        }
        else if (p2.isBuffCanon)
        {
            imageCurrentBuffP2.sprite = buffImageList[3];
        }
        else if (p2.isBuffDice > 0 || p2.isBuffDiceNext > 0)
        {
            imageCurrentBuffP2.sprite = buffImageList[4];
        }
        else
        {
            imageCurrentBuffP2.sprite = buffImageList[0];
        }
    }

    // =========================================================
    // NOTIFICATION PANEL
    // =========================================================

    /// <summary>
    /// Hiện hoặc ẩn panel thông báo.
    /// </summary>
    public void HidePlayerPlayPanel(bool i)
    {
        notifiPanel.gameObject.SetActive(i);
    }

    /// <summary>
    /// Hiển thị thông báo trong 2 giây.
    /// </summary>
    public void SendNotifi(string message)
    {
        if (notifiPanel != null && textNotifi != null)
        {
            notifiPanel.SetActive(true);
            textNotifi.text = message;

            // Sau 2 giây sẽ tự ẩn
            Invoke(nameof(HideNotifi), 2f);
        }
    }

    /// <summary>
    /// Ẩn panel thông báo.
    /// </summary>
    private void HideNotifi()
    {
        if (notifiPanel != null)
        {
            notifiPanel.SetActive(false);
        }
    }

    // =========================================================
    // RESULT PANEL
    // =========================================================

    /// <summary>
    /// Hiển thị bảng kết quả cuối game.
    /// </summary>
    public void UpdateResultPanel(int coinP1, int coinP2)
    {
        if (resultPanel != null)
        {
            // Hiện panel kết quả
            resultPanel.SetActive(true);

            // Phát âm thanh mở panel kết quả
            AudioManager.Instance.PlaySFX(AudioManager.Instance.openResultPanel);

            // Cập nhật coin Player 1
            if (coinTextP1 != null)
                coinTextP1.text = $"{coinP1}";

            // Cập nhật coin Player 2
            if (coinTextP2 != null)
                coinTextP2.text = $"{coinP2}";

            // =========================
            // PLAYER 1 THẮNG
            // =========================

            if (coinP1 > coinP2)
            {
                player1ResultUI.transform.GetChild(0).gameObject.SetActive(true);   // Win
                player1ResultUI.transform.GetChild(1).gameObject.SetActive(false);  // Lose

                player2ResultUI.transform.GetChild(0).gameObject.SetActive(false);
                player2ResultUI.transform.GetChild(1).gameObject.SetActive(true);
            }

            // =========================
            // PLAYER 2 THẮNG
            // =========================

            else if (coinP1 < coinP2)
            {
                player1ResultUI.transform.GetChild(0).gameObject.SetActive(false);
                player1ResultUI.transform.GetChild(1).gameObject.SetActive(true);

                player2ResultUI.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.transform.GetChild(1).gameObject.SetActive(false);
            }

            // =========================
            // HÒA
            // =========================

            else
            {
                player1ResultUI.transform.GetChild(0).gameObject.SetActive(true);
                player1ResultUI.transform.GetChild(1).gameObject.SetActive(false);

                player2ResultUI.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Ẩn bảng kết quả.
    /// </summary>
    public void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    // =========================================================
    // PLAYER PLAY PANEL
    // =========================================================

    /// <summary>
    /// Hiện hoặc ẩn bảng thông tin người chơi.
    /// </summary>
    public void HideNotifiPlayPanel(bool i)
    {
        notifiPlay.gameObject.SetActive(i);
    }

    // =========================================================
    // BONUS PANEL
    // =========================================================

    /// <summary>
    /// Hiển thị panel Bonus trong 1 giây rồi tự tắt.
    /// </summary>
    public IEnumerator HideBonusPanel()
    {
        bonusPanel.SetActive(true);

        yield return new WaitForSeconds(1f);

        bonusPanel.SetActive(false);
    }

    public void ShowBonusCoin(int playerID)
    {
        // Player 1 bonus coin UI
        if (playerID == 0)
        {
            StartCoroutine(ShowUIBonusCoin(bonusCoinTextP1));
        }

        // Player 2 bonus coin UI
        else if (playerID == 1)
        {
            StartCoroutine(ShowUIBonusCoin(bonusCoinTextP2));
        }
    }

    IEnumerator ShowUIBonusCoin(GameObject canvas)
    {
        // Hiện UI bonus coin
        canvas.SetActive(true);

        yield return new WaitForSeconds(3f);

        // Ẩn UI bonus coin
        canvas.SetActive(false);
    }
    public IEnumerator BlackPanelRoutine()
    {
        // Hiện panel
        blackPanel.SetActive(true);

        yield return null;
    }
    public void HideUIMain()
    {
        uiMain.SetActive(false);
    }
}