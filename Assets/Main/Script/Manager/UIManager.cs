using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    // Singleton để các script khác có thể gọi UIManager.Instance
    public static UIManager Instance { get; private set; }
    [Header("Player References")]
    public PlayerManager playerManager1;
    public PlayerManager playerManager2;

    [Header("Main UI")]
    public GameObject uiMain;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI coinTextP1;
    public TextMeshProUGUI coinTextP2;
    public GameObject player1ResultUI;
    public GameObject player2ResultUI;
    public Image avatarResultP1;
    public Image avatarResultP2;

    [Header("Notification UI")]
    public GameObject canvasNotifi;
    public GameObject notifiPanel;
    public TextMeshProUGUI textNotifi;
    public GameObject diceRollP1;
    public GameObject diceRollP2;
    public GameObject notifiP1;
    public GameObject notifiP2;

    [Header("Player Info UI")]
    public GameObject notifiPlay;
    public GameObject notifiplayer1;
    public GameObject notifiplayer2;
    public TextMeshProUGUI coinTextNotP1;
    public TextMeshProUGUI coinTextNotP2;
    public TextMeshProUGUI indexTextP1;
    public TextMeshProUGUI indexTextP2;
    public Image imageCurrentBuffP1;
    public Image imageCurrentBuffP2;
    public List<Sprite> buffImageList;
    public GameObject bonusCoinTextP1;
    public GameObject bonusCoinTextP2;

    [Header("Mini Game UI")]
    public GameObject canvasMiniGame;
    public TextMeshProUGUI timeMiniGameText;
    public TextMeshProUGUI coinMiniGameTextP1;
    public TextMeshProUGUI coinMiniGameTextP2;
    public Image avatarP1;
    public Image avatarP2;

    [Header("Instruction UI")]
    public GameObject canvasIntructGamePlay;
    public TextMeshProUGUI nameMiniGameText;
    public TextMeshProUGUI instructGamePlayText;
    public TextMeshProUGUI errorGamePlayText;
    public GameObject canvasInstructInput;

    [Header("Debuff UI")]
    public GameObject leftCardCanvas;
    public GameObject rightCardCanvas;
    public GameObject[] leftCardsList;
    public GameObject[] rightCardsList;
    public Sprite[] debuffSpriteList;
    public GameObject panelNotiifiChooseDebuff;
    public GameObject magicDebuffPanel;
    public GameObject cannonDebuffPanel;
    [Header("Buff UI")]
    public GameObject cannonPowerPanel;
    public GameObject cannonShieldPanel;
    public GameObject petrificationImmunityPanel;
    [Header("Trap UI")]
    public GameObject bombTrapPanel;
    public GameObject positionTrapPanel;

    [Header("Shop UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI[] playerCoinTextList;
    public Image[] playerItemImagesList;
    public TextMeshProUGUI timerShopText;
    public GameObject canvasRandomCard;
    public GameObject panelPlayer1Turn;
    public GameObject panelPlayer2Turn;
    public GameObject[] itemsList;
    public GameObject[] itemCardRandomList;
    [Header("Instruct Input KeyBoard")]
    public GameObject instructKeyBoard;
    public GameObject instructKeyBoardP1;
    public GameObject instructKeyBoardP2;
    public List<GameObject> instructKeyBoardInputListP1;
    public List<GameObject> instructKeyBoardInputListP2;
    public GameObject instructConsoleClone;
    public GameObject instructConsoleCloneP1;
    public GameObject instructConsoleCloneP2;
    public List<GameObject> instructConsoleInputListP1;
    public List<GameObject> instructConsoleInputListP2;
    public GameObject instructBothConsole;
    public List<GameObject> instructBothConsoleInputListP1;

    [Header("Screen Transition")]
    public GameObject blackPanel;
    public GameObject flastBlackPanel;

    [Header("Settings UI")]
    public GameObject settingPanel;
    public GameObject openSettingPanelButton;
    public GameObject exitMainMenuButton;

    [Header("Settings Animation")]
    [SerializeField] private Animator settingPanelAnimator;
    [SerializeField] private string settingOpenTrigger = "Open";
    [SerializeField] private string settingCloseTrigger = "Close";

    [Min(0f)]
    [SerializeField] private float settingOpenAnimationTime = 0.5f;

    [Min(0f)]
    [SerializeField] private float settingCloseAnimationTime = 0.5f;

    private Coroutine settingPanelAnimationCoroutine;

    public bool IsSettingPanelTransitioning
    {
        get;
        private set;
    }
    [Header("Cosole UI")]
    public GameObject consoleOpenImageP1;
    public GameObject consoleCloseImageP1;
    public GameObject consoleOpenImageP2;
    public GameObject consoleCloseImageP2;
    public GameObject instructConsolePanel;
    [Header("Key Board")]
    public GameObject instructKeyBoardPanel;
    public bool isShowKeyBoard = false;

    [Header("Bonus UI")]
    public GameObject bonusPanel;

    [Header("Update Settings")]
    public float updateUITime = 0.25f;
    private float updateTimer;
    public List<GameObject> allUI;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

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

        if (ControllerManager.Instance != null)
        {
            ControllerManager.Instance
                .RefreshInputInstructionUI();
        }
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
        if (playerManager1 != null && playerManager2 != null)
        {
            UpdateCoinPowerUI();
            UpdateCoinAllPlayer();
            UpdateIndexPlayerWalk();
            UpdateCurrentBuffPlayer();
        }
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
            .GetChild(1);

        // Root chứa icon Coin Power Player 2
        Transform coinRoot2 =
            notifiplayer2.transform
            .GetChild(2)
            .GetChild(1);
        Transform highlightYellowCoinPowerP1 = notifiplayer1.transform.GetChild(2).GetChild(0);
        Transform highlightYellowCoinPowerP2 = notifiplayer2.transform.GetChild(2).GetChild(0);

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
         var hightlightCoinPower = coinRoot.GetChild(i).GetComponent<UIImagePowerCoinColorEffect>();
            if( i >= p1.countCoinPower)
            {
                hightlightCoinPower.enabled = false;
                highlightYellowCoinPowerP1.GetChild(i).gameObject.SetActive(false);
            }else
            {
                               hightlightCoinPower.enabled = true;
                highlightYellowCoinPowerP1.GetChild(i).gameObject.SetActive(true);
            }
              
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
            var hightlightCoinPower = coinRoot2.GetChild(i).GetComponent<UIImagePowerCoinColorEffect>();
            if (i >= p2.countCoinPower)
            {
                hightlightCoinPower.enabled = false;
                highlightYellowCoinPowerP2.GetChild(i).gameObject.SetActive(false);
            }else
            {
                               hightlightCoinPower.enabled = true;
                highlightYellowCoinPowerP2.GetChild(i).gameObject.SetActive(true);
            }
               
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

        indexTextP1.text = $"{p1.currentIndex + 1}/33";
        indexTextP2.text = $"{p2.currentIndex + 1}/33";
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
  public IEnumerator ShowDebuffAndBuffPanel(GameObject ui)
    {
           if(ui!= null)
        {
            ui.SetActive(true);
            yield return new WaitForSeconds(1.4f);
            ui.SetActive(false);
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
            AudioManager.Instance.PlayUI(AudioManager.Instance.openResultPanel);

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

            avatarResultP1.sprite = playerManager1.GetComponent<PlayerInfo>().avatarCharacter;
            avatarResultP2.sprite = playerManager2.GetComponent<PlayerInfo>().avatarCharacter;

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

        yield return new WaitForSeconds(2f);

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

    public bool ActiveSettingPanel(bool active)
    {
        if (settingPanel == null ||
            IsSettingPanelTransitioning)
        {
            return false;
        }

        if (settingPanel.activeSelf == active)
        {
            return false;
        }

        settingPanelAnimationCoroutine =
            StartCoroutine(
                AnimateSettingPanel(active)
            );

        return true;
    }

    private IEnumerator AnimateSettingPanel(bool open)
    {
        IsSettingPanelTransitioning = true;

        if (settingPanel == null)
        {
            IsSettingPanelTransitioning = false;
            settingPanelAnimationCoroutine = null;
            yield break;
        }

        if (settingPanelAnimator == null)
        {
            settingPanelAnimator =
                settingPanel.GetComponent<Animator>();
        }

        if (open)
        {
            settingPanel.SetActive(true);

            if (settingPanelAnimator != null)
            {
                settingPanelAnimator.ResetTrigger(
                    settingCloseTrigger
                );

                settingPanelAnimator.SetTrigger(
                    settingOpenTrigger
                );
            }

            // Không kiểm tra Animator.
            // Sau đúng 0.5 giây thì Pause.
            yield return new WaitForSecondsRealtime(0.5f);

            if (PauseGameManager.Instance != null)
            {
                PauseGameManager.Instance.PauseGame();
            }
        }
        else
        {
            // Resume trước khi chạy animation đóng.
            if (PauseGameManager.Instance != null)
            {
                PauseGameManager.Instance.ResumeGame();
            }
            else
            {
                Time.timeScale = 1f;
            }

            if (settingPanelAnimator != null)
            {
                settingPanelAnimator.ResetTrigger(
                    settingOpenTrigger
                );

                settingPanelAnimator.SetTrigger(
                    settingCloseTrigger
                );
            }

            // Sau đúng 0.5 giây thì tắt panel.
            yield return new WaitForSecondsRealtime(0.7f);

            settingPanel.SetActive(false);
        }

        IsSettingPanelTransitioning = false;
        settingPanelAnimationCoroutine = null;
    }

    public void ForceHideSettingPanel()
    {
        if (settingPanelAnimationCoroutine != null)
        {
            StopCoroutine(settingPanelAnimationCoroutine);
            settingPanelAnimationCoroutine = null;
        }

        IsSettingPanelTransitioning = false;

        if (settingPanelAnimator == null &&
            settingPanel != null)
        {
            settingPanelAnimator =
                settingPanel.GetComponent<Animator>();
        }

        if (settingPanelAnimator != null)
        {
            settingPanelAnimator.ResetTrigger(
                settingOpenTrigger
            );

            settingPanelAnimator.ResetTrigger(
                settingCloseTrigger
            );
        }

        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    public void ActiveOpenSettingButton(bool i)
    {
        openSettingPanelButton.SetActive(i);
    }
    public void FindPlayerManager()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player 1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player 2");
        if (player1 != null)
        {
            playerManager1 = player1.GetComponent<PlayerManager>();
        }

        if (player2 != null)
        {
            playerManager2 = player2.GetComponent<PlayerManager>();
        }
    }
    public IEnumerator ShowConsoleConect(GameObject ui)
    {
        ui.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        ui.SetActive(false);
    }
    public IEnumerator ShowConsoleFailConect(GameObject ui)
    {
        ui.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        ui.SetActive(false);
    }
    public void HideAllUI()
    {
        for (int i = 0;i < allUI.Count; i++)
        {
            if(allUI[i] != null && allUI[i].activeSelf)
            {
                allUI[i].SetActive(false);
            }
            
        }
    }
}