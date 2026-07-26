using System.Collections;
using UnityEngine;

public class InputChooseItem : MonoBehaviour
{
    #region References

    public ShopManager shopManager;
    public UIManager ui;

    #endregion

    #region Player State

    public int player1Index = 0;
    public int player2Index = 0;

    public bool isPlayer1Choose = false;
    public bool isPlayer2Choose = false;

    #endregion

    #region Navigation Settings

    [Header("Navigation Settings")]
    [SerializeField] private float navigationThreshold = 0.5f;
    [SerializeField] private float resetThreshold = 0.2f;

    private bool player1HorizontalReady = true;
    private bool player1VerticalReady = true;

    private bool player2HorizontalReady = true;
    private bool player2VerticalReady = true;

    #endregion

    #region Random Card State

    private bool isChoosingRandomCard = false;
    private int randomCardIndex = 0;

    // 0 = Player 1
    // 1 = Player 2
    private int randomCardPlayer = -1;

    private Coroutine randomCardCoroutine;
    private bool isChoosingCardRoutine = false;

    #endregion

    private void Start()
    {
        ui = UIManager.Instance;
        InitHighlight();
    }

    private void Update()
    {
        if (isChoosingRandomCard)
        {
            HandleRandomCardInput();
            return;
        }

        HandlePlayerInput();
    }

    #region Initialize

    private void InitHighlight()
    {
        for (int i = 0; i < ui.itemsList.Length; i++)
        {
            ui.itemsList[i]
                .transform.GetChild(1)
                .gameObject.SetActive(false);

            ui.itemsList[i]
                .transform.GetChild(2)
                .gameObject.SetActive(false);
        }

        ClearRandomCardHighlight();

        isPlayer1Choose = false;
        isPlayer2Choose = false;

        isChoosingRandomCard = false;
        isChoosingCardRoutine = false;

        ResetPlayer1Navigation();
        ResetPlayer2Navigation();
    }

    private void ClearRandomCardHighlight()
    {
        for (int i = 0; i < ui.itemCardRandomList.Length; i++)
        {
            ui.itemCardRandomList[i]
                .transform.GetChild(1)
                .gameObject.SetActive(false);
        }
    }

    #endregion

    #region Player Input

    private void HandlePlayerInput()
    {
        if (isPlayer1Choose)
        {
            HandlePlayer1Input();

            if (Player1ConfirmDown())
            {
                BuyPlayer1Item();
            }
            else if (Player1CancelDown())
            {
                SkipPlayer1();
            }
        }

        if (isPlayer2Choose)
        {
            HandlePlayer2Input();

            if (Player2ConfirmDown())
            {
                BuyPlayer2Item();
            }
            else if (Player2CancelDown())
            {
                SkipPlayer2();
            }
        }
    }

    private bool Player1ConfirmDown()
    {
        return Input.GetKeyDown(KeyCode.J) ||
               Input.GetKeyDown(KeyCode.Joystick1Button0);
    }

    private bool Player1CancelDown()
    {
        return Input.GetKeyDown(KeyCode.K) ||
               Input.GetKeyDown(KeyCode.Joystick1Button1);
    }

    private bool Player2ConfirmDown()
    {
        return Input.GetKeyDown(KeyCode.Keypad1) ||
               Input.GetKeyDown(KeyCode.Joystick2Button0);
    }

    private bool Player2CancelDown()
    {
        return Input.GetKeyDown(KeyCode.Keypad2) ||
               Input.GetKeyDown(KeyCode.Joystick2Button1);
    }

    #endregion

    #region Timeout

    public void TimeOutPlayer1()
    {
        if (!isPlayer1Choose)
            return;

        SkipPlayer1();
    }

    public void TimeOutPlayer2()
    {
        if (!isPlayer2Choose)
            return;

        SkipPlayer2();
    }

    public void TimeOutRandomCard()
    {
        if (!isChoosingRandomCard)
            return;

        StartChooseRandomCard();
    }

    #endregion

    #region Player 1 Navigation

    private void HandlePlayer1Input()
    {
        IndexItem current =
            ui.itemsList[player1Index].GetComponent<IndexItem>();

        if (current == null)
            return;

        float horizontal = Input.GetAxisRaw("HorizontalP1");
        float vertical = Input.GetAxisRaw("VerticalP1");

        UpdatePlayer1AxisReset(horizontal, vertical);

        // Chọn trục có giá trị lớn hơn để tránh đi chéo hai lần.
        if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
        {
            if (vertical > navigationThreshold &&
                player1VerticalReady)
            {
                player1VerticalReady = false;
                ChangePlayer1(current.top);
                return;
            }

            if (vertical < -navigationThreshold &&
                player1VerticalReady)
            {
                player1VerticalReady = false;
                ChangePlayer1(current.bottom);
                return;
            }
        }

        if (horizontal < -navigationThreshold &&
            player1HorizontalReady)
        {
            player1HorizontalReady = false;
            ChangePlayer1(current.left);
            return;
        }

        if (horizontal > navigationThreshold &&
            player1HorizontalReady)
        {
            player1HorizontalReady = false;
            ChangePlayer1(current.right);
        }
    }

    private void UpdatePlayer1AxisReset(
        float horizontal,
        float vertical)
    {
        if (Mathf.Abs(horizontal) < resetThreshold)
        {
            player1HorizontalReady = true;
        }

        if (Mathf.Abs(vertical) < resetThreshold)
        {
            player1VerticalReady = true;
        }
    }

    private void ResetPlayer1Navigation()
    {
        player1HorizontalReady = true;
        player1VerticalReady = true;
    }

    private void ChangePlayer1(int newIndex)
    {
        if (newIndex < 0 ||
            newIndex >= ui.itemsList.Length ||
            newIndex == player1Index)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.movechooseItemClip
        );

        ui.itemsList[player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(false);

        player1Index = newIndex;

        ui.itemsList[player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(true);
    }

    #endregion

    #region Player 1 Buy

    private void BuyPlayer1Item()
    {
        PriceItem item =
            ui.itemsList[player1Index].GetComponent<PriceItem>();

        if (item == null)
            return;

        // Random Card
        if (player1Index == 1)
        {
            if (!shopManager.BuyItem(
                    0,
                    player1Index,
                    item.price))
            {
                return;
            }

            shopManager.StopTurnTimer();

            isPlayer1Choose = false;

            ui.itemsList[player1Index]
                .transform.GetChild(1)
                .gameObject.SetActive(false);

            OpenRandomCard(0);
            return;
        }

        if (shopManager.BuyItem(
                0,
                player1Index,
                item.price))
        {
            shopManager.StopTurnTimer();

            isPlayer1Choose = false;

            ui.itemsList[player1Index]
                .transform.GetChild(1)
                .gameObject.SetActive(false);

            bool win1 =
                GameManager.Instance.CheckWinnerByPowerCoinP1();

            if (win1)
            {
                StartCoroutine(CloseShop());
                return;
            }

            StartCoroutine(ShowPlayer2TurnDelay());
        }
    }

    private void SkipPlayer1()
    {
        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.skipBuyClip
        );

        shopManager.StopTurnTimer();

        isPlayer1Choose = false;

        ui.itemsList[player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(false);

        StartCoroutine(ShowPlayer2TurnDelay());
    }

    #endregion

    #region Player 2 Navigation

    private void HandlePlayer2Input()
    {
        IndexItem current =
            ui.itemsList[player2Index].GetComponent<IndexItem>();

        if (current == null)
            return;

        float horizontal = Input.GetAxisRaw("HorizontalP2");
        float vertical = Input.GetAxisRaw("VerticalP2");

        UpdatePlayer2AxisReset(horizontal, vertical);

        if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
        {
            if (vertical > navigationThreshold &&
                player2VerticalReady)
            {
                player2VerticalReady = false;
                ChangePlayer2(current.top);
                return;
            }

            if (vertical < -navigationThreshold &&
                player2VerticalReady)
            {
                player2VerticalReady = false;
                ChangePlayer2(current.bottom);
                return;
            }
        }

        if (horizontal < -navigationThreshold &&
            player2HorizontalReady)
        {
            player2HorizontalReady = false;
            ChangePlayer2(current.left);
            return;
        }

        if (horizontal > navigationThreshold &&
            player2HorizontalReady)
        {
            player2HorizontalReady = false;
            ChangePlayer2(current.right);
        }
    }

    private void UpdatePlayer2AxisReset(
        float horizontal,
        float vertical)
    {
        if (Mathf.Abs(horizontal) < resetThreshold)
        {
            player2HorizontalReady = true;
        }

        if (Mathf.Abs(vertical) < resetThreshold)
        {
            player2VerticalReady = true;
        }
    }

    private void ResetPlayer2Navigation()
    {
        player2HorizontalReady = true;
        player2VerticalReady = true;
    }

    private void ChangePlayer2(int newIndex)
    {
        if (newIndex < 0 ||
            newIndex >= ui.itemsList.Length ||
            newIndex == player2Index)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.movechooseItemClip
        );

        ui.itemsList[player2Index]
            .transform.GetChild(2)
            .gameObject.SetActive(false);

        player2Index = newIndex;

        ui.itemsList[player2Index]
            .transform.GetChild(2)
            .gameObject.SetActive(true);
    }

    #endregion

    #region Player 2 Buy

    private void BuyPlayer2Item()
    {
        PriceItem item =
            ui.itemsList[player2Index].GetComponent<PriceItem>();

        if (item == null)
            return;

        // Random Card
        if (player2Index == 1)
        {
            if (!shopManager.BuyItem(
                    1,
                    player2Index,
                    item.price))
            {
                return;
            }

            shopManager.StopTurnTimer();

            isPlayer2Choose = false;

            ui.itemsList[player2Index]
                .transform.GetChild(2)
                .gameObject.SetActive(false);

            OpenRandomCard(1);
            return;
        }

        if (shopManager.BuyItem(
                1,
                player2Index,
                item.price))
        {
            shopManager.StopTurnTimer();

            isPlayer2Choose = false;

            ui.itemsList[player2Index]
                .transform.GetChild(2)
                .gameObject.SetActive(false);

            bool win2 =
                GameManager.Instance.CheckWinnerByPowerCoinP2();

            StartCoroutine(CloseShop());
        }
    }

    private void SkipPlayer2()
    {
        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.skipBuyClip
        );

        shopManager.StopTurnTimer();

        isPlayer2Choose = false;

        ui.itemsList[player2Index]
            .transform.GetChild(2)
            .gameObject.SetActive(false);

        CheckAllPlayersFinished();
    }

    #endregion

    #region Turn Delay

    private IEnumerator ShowPlayer2TurnDelay()
    {
        isPlayer1Choose = false;
        isPlayer2Choose = false;
        isChoosingRandomCard = false;

        ui.itemsList[player1Index]
            .transform.GetChild(1)
            .gameObject.SetActive(false);

        ui.itemsList[player2Index]
            .transform.GetChild(2)
            .gameObject.SetActive(false);

        shopManager.ShowPlayer2Turn();

        yield return new WaitForSeconds(0.6f);

        // Tránh P2 vừa vào lượt đã nhận input đang bị giữ.
        yield return StartCoroutine(WaitForPlayer2AxisRelease());

        ResetPlayer2Navigation();

        isPlayer2Choose = true;

        ui.itemsList[player2Index]
            .transform.GetChild(2)
            .gameObject.SetActive(true);

        shopManager.StartTurnTimer(TimeOutPlayer2);
    }

    private IEnumerator WaitForPlayer2AxisRelease()
    {
        while (true)
        {
            float horizontal =
                Input.GetAxisRaw("HorizontalP2");

            float vertical =
                Input.GetAxisRaw("VerticalP2");

            bool horizontalReleased =
                Mathf.Abs(horizontal) < resetThreshold;

            bool verticalReleased =
                Mathf.Abs(vertical) < resetThreshold;

            if (horizontalReleased && verticalReleased)
            {
                break;
            }

            yield return null;
        }
    }

    #endregion

    #region Random Card

    private void OpenRandomCard(int playerIndex)
    {
        var setting = SettingManager.Instance;

        if (setting != null)
        {
            setting.ResetGuide();
        }

        ui.openInstructBuyButton.SetActive(false);
        ui.canvasRandomCard.SetActive(true);

        randomCardPlayer = playerIndex;
        randomCardIndex = 0;

        isChoosingRandomCard = false;
        isPlayer1Choose = false;
        isPlayer2Choose = false;
        isChoosingCardRoutine = false;

        ResetPlayer1Navigation();
        ResetPlayer2Navigation();

        RandomizeCards();
        ClearRandomCardHighlight();

        if (randomCardCoroutine != null)
        {
            StopCoroutine(randomCardCoroutine);
        }

        randomCardCoroutine =
            StartCoroutine(WaitOpenAnimation());
    }

    private IEnumerator WaitOpenAnimation()
    {
        Animator animator =
            ui.canvasRandomCard.GetComponent<Animator>();

        yield return null;

        if (animator != null)
        {
            yield return new WaitForSeconds(
                animator
                    .GetCurrentAnimatorStateInfo(0)
                    .length + 0.1f
            );
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        // Chờ người chơi thả cần trước khi cho chọn.
        yield return StartCoroutine(
            WaitForRandomCardAxisRelease()
        );

        if (randomCardPlayer == 0)
        {
            ResetPlayer1Navigation();
        }
        else
        {
            ResetPlayer2Navigation();
        }

        isChoosingRandomCard = true;

        ui.itemCardRandomList[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);

        shopManager.StartTurnTimer(TimeOutRandomCard);

        randomCardCoroutine = null;
    }

    private IEnumerator WaitForRandomCardAxisRelease()
    {
        while (true)
        {
            float horizontal;

            if (randomCardPlayer == 0)
            {
                horizontal =
                    Input.GetAxisRaw("HorizontalP1");
            }
            else
            {
                horizontal =
                    Input.GetAxisRaw("HorizontalP2");
            }

            if (Mathf.Abs(horizontal) < resetThreshold)
            {
                break;
            }

            yield return null;
        }
    }

    private void RandomizeCards()
    {
        int[] randomItems = { 0, 2, 3, 5 };

        for (int i = 0; i < randomItems.Length; i++)
        {
            int randomIndex =
                Random.Range(i, randomItems.Length);

            (randomItems[i], randomItems[randomIndex]) =
                (randomItems[randomIndex], randomItems[i]);
        }

        int cardCount = Mathf.Min(
            ui.itemCardRandomList.Length,
            randomItems.Length
        );

        for (int i = 0; i < cardCount; i++)
        {
            RandomCard card =
                ui.itemCardRandomList[i]
                    .GetComponent<RandomCard>();

            if (card == null)
                continue;

            card.itemIndex = randomItems[i];
            card.image.sprite = card.spriteStar;
        }
    }

    private void HandleRandomCardInput()
    {
        IndexItem current =
            ui.itemCardRandomList[randomCardIndex]
                .GetComponent<IndexItem>();

        if (current == null)
            return;

        if (randomCardPlayer == 0)
        {
            HandlePlayer1RandomCardNavigation(current);

            if (Player1ConfirmDown())
            {
                StartChooseRandomCard();
            }
        }
        else if (randomCardPlayer == 1)
        {
            HandlePlayer2RandomCardNavigation(current);

            if (Player2ConfirmDown())
            {
                StartChooseRandomCard();
            }
        }
    }

    private void HandlePlayer1RandomCardNavigation(
        IndexItem current)
    {
        float horizontal =
            Input.GetAxisRaw("HorizontalP1");

        if (Mathf.Abs(horizontal) < resetThreshold)
        {
            player1HorizontalReady = true;
            return;
        }

        if (!player1HorizontalReady)
            return;

        if (horizontal < -navigationThreshold)
        {
            player1HorizontalReady = false;
            ChangeRandomCard(current.left);
        }
        else if (horizontal > navigationThreshold)
        {
            player1HorizontalReady = false;
            ChangeRandomCard(current.right);
        }
    }

    private void HandlePlayer2RandomCardNavigation(
        IndexItem current)
    {
        float horizontal =
            Input.GetAxisRaw("HorizontalP2");

        if (Mathf.Abs(horizontal) < resetThreshold)
        {
            player2HorizontalReady = true;
            return;
        }

        if (!player2HorizontalReady)
            return;

        if (horizontal < -navigationThreshold)
        {
            player2HorizontalReady = false;
            ChangeRandomCard(current.left);
        }
        else if (horizontal > navigationThreshold)
        {
            player2HorizontalReady = false;
            ChangeRandomCard(current.right);
        }
    }

    private void ChangeRandomCard(int newIndex)
    {
        if (newIndex < 0 ||
            newIndex >= ui.itemCardRandomList.Length ||
            newIndex == randomCardIndex)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.movechooseItemClip
        );

        ui.itemCardRandomList[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(false);

        randomCardIndex = newIndex;

        ui.itemCardRandomList[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);
    }

    private void StartChooseRandomCard()
    {
        if (isChoosingCardRoutine)
            return;

        StartCoroutine(ChooseRandomCard());
    }

    private IEnumerator ChooseRandomCard()
    {
        isChoosingCardRoutine = true;

        shopManager.StopTurnTimer();

        isChoosingRandomCard = false;

        RandomCard card =
            ui.itemCardRandomList[randomCardIndex]
                .GetComponent<RandomCard>();

        if (card == null)
        {
            isChoosingCardRoutine = false;
            yield break;
        }

        ui.itemCardRandomList[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);

        card.image.sprite =
            shopManager.itemSprites[card.itemIndex];

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.buyItemClip
        );

        shopManager.ShowPlayerItem(
            randomCardPlayer,
            card.itemIndex
        );

        yield return new WaitForSeconds(2f);

        ClearRandomCardHighlight();

        ui.canvasRandomCard.SetActive(false);
        ui.openInstructBuyButton.SetActive(true);

        isChoosingCardRoutine = false;

        if (randomCardPlayer == 0)
        {
            StartCoroutine(ShowPlayer2TurnDelay());
        }
        else
        {
            isPlayer2Choose = false;

            ui.itemsList[player2Index]
                .transform.GetChild(2)
                .gameObject.SetActive(false);

            CheckAllPlayersFinished();
        }
    }

    #endregion

    #region Close Shop

    private void CheckAllPlayersFinished()
    {
        if (!isPlayer1Choose &&
            !isPlayer2Choose &&
            !isChoosingRandomCard)
        {
            StartCoroutine(CloseShop());
        }
    }

    public IEnumerator CloseShop()
    {
        shopManager.StopTurnTimer();

        yield return new WaitForSeconds(1f);

        ShopManager.Instance.CloseShop();
    }

    #endregion
}