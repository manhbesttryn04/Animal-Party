using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton để các script khác có thể gọi UIManager.Instance
    public static UIManager Instance { get; private set; }

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

    #region Notification

    // Panel thông báo
    public GameObject notifiPanel;

    // Nội dung thông báo
    public TextMeshProUGUI textNotifi;

    #endregion

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

    //Buff hiện có
    public Image imageCurrentBuffP1;
    public Image imageCurrentBuffP2;
    //List Image Buff
    public List<Sprite> buffImageList;

    #endregion

    // Panel thưởng
    public GameObject bonusPanel;

    // Tham chiếu tới PlayerManager
    public PlayerManager playerManager1;
    public PlayerManager playerManager2;

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
        // Tìm Player theo Tag
        playerManager1 = GameObject.FindGameObjectWithTag("Player 1").GetComponent<PlayerManager>();
        playerManager2 = GameObject.FindGameObjectWithTag("Player 2").GetComponent<PlayerManager>();
    }
    private void Update()
    {
        // Nếu panel đang mở thì cập nhật liên tục
        if (notifiPlay.activeSelf)
        {
            UpdateCoinPowerUI();
            UpdateCoinAllPlayer();
            UpdateIndexPlayerWalk();
            UpdateCurrentBuffPlayer();
        }
        else
            return;
    }

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

        // Player 1
        for (int i = 0; i < coinRoot.childCount; i++)
        {
            // Panel đen che icon
            Transform blackPanel =
                coinRoot.GetChild(i).GetChild(0);

            // Nếu đã có Coin Power thì tắt panel đen
            blackPanel.gameObject.SetActive(i >= p1.countCoinPower);
        }

        // Player 2
        for (int i = 0; i < coinRoot2.childCount; i++)
        {
            Transform blackPanel =
                coinRoot2.GetChild(i).GetChild(0);

            blackPanel.gameObject.SetActive(i >= p2.countCoinPower);
        }
    }

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

    /// <summary>
    /// Cập nhật vị trí hiện tại trên bàn cờ.
    /// </summary>
    public void UpdateIndexPlayerWalk()
    {
        PlayerMoveAI p1 = playerManager1.playerMoveAI;
        PlayerMoveAI p2 = playerManager2.playerMoveAI;

        indexTextP1.text = $"{p1.currentIndex}/33";
        indexTextP2.text = $"{p2.currentIndex}/33";
    }
    public void UpdateCurrentBuffPlayer()
    {
        PlayerBuff p1 = playerManager1 .playerBuff;
        PlayerBuff p2 = playerManager2.playerBuff;

        if (p1.isBuffDeffense)
        {
            imageCurrentBuffP1.sprite = buffImageList[1];
        }else if (p1.isBuffMagic)
        {
            imageCurrentBuffP1.sprite= buffImageList[2];
        }else if (p1.isBuffCanon)
        {
            imageCurrentBuffP1.sprite = buffImageList[3];
        }else if(p1.isBuffDice>0 || p1.isBuffDiceNext > 0)
        {
            imageCurrentBuffP1 .sprite = buffImageList[4];
        }else imageCurrentBuffP1.sprite = buffImageList[0];


        //p2
        // Player 2
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

    /// <summary>
    /// Hiện hoặc ẩn panel thông báo.
    /// </summary>
    public void HidePlayerPlayPanel(bool i)
    {
        notifiPanel.gameObject.SetActive(i);
    }

   

    /// <summary>
    /// Hiển thị bảng kết quả cuối game.
    /// </summary>
    public void UpdateResultPanel(int coinP1, int coinP2)
    {
        if (resultPanel != null)
        {
            // Hiện panel
            resultPanel.SetActive(true);

            // Phát âm thanh mở panel
            AudioManager.Instance.PlaySFX(AudioManager.Instance.openResultPanel);

            // Hiển thị coin
            if (coinTextP1 != null)
                coinTextP1.text = $"{coinP1}";

            if (coinTextP2 != null)
                coinTextP2.text = $"{coinP2}";

            // Player 1 thắng
            if (coinP1 > coinP2)
            {
                player1ResultUI.transform.GetChild(0).gameObject.SetActive(true);   // Win
                player1ResultUI.transform.GetChild(1).gameObject.SetActive(false);  // Lose

                player2ResultUI.transform.GetChild(0).gameObject.SetActive(false);
                player2ResultUI.transform.GetChild(1).gameObject.SetActive(true);
            }
            // Player 2 thắng
            else if (coinP1 < coinP2)
            {
                player1ResultUI.transform.GetChild(0).gameObject.SetActive(false);
                player1ResultUI.transform.GetChild(1).gameObject.SetActive(true);

                player2ResultUI.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.transform.GetChild(1).gameObject.SetActive(false);
            }
            // Hòa
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

    /// <summary>
    /// Hiện hoặc ẩn bảng thông tin người chơi.
    /// </summary>
    public void HideNotifiPlayPanel(bool i)
    {
        notifiPlay.gameObject.SetActive(i);
    }

    /// <summary>
    /// Hiển thị panel Bonus trong 1 giây rồi tự tắt.
    /// </summary>
    public IEnumerator HideBonusPanel()
    {
        bonusPanel.SetActive(true);

        yield return new WaitForSeconds(1f);

        bonusPanel.SetActive(false);
    }
}