using System.Collections;
using UnityEngine;

public class InputChooseItem : MonoBehaviour
{
    #region References

    public GameObject[] items;
    public GameObject[] itemCardRandom;
    public ShopManager shopManager;

    #endregion

    #region Player State

    public int player1Index = 0;
    public int player2Index = 0;

    public bool isPlayer1Choose = false;
    public bool isPlayer2Choose = false;

    #endregion

    #region Random Card State

    private bool isChoosingRandomCard = false;
    private int randomCardIndex = 0;
    private int randomCardPlayer = -1; // 0 = P1, 1 = P2

    private Coroutine randomCardCoroutine;
    private bool isChoosingCardRoutine = false;

    #endregion

    private void Start()
    {
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

    private void InitHighlight()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].transform.GetChild(1).gameObject.SetActive(false);
            items[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        ClearRandomCardHighlight();

        isPlayer1Choose = false;
        isPlayer2Choose = false;
        isChoosingRandomCard = false;
        isChoosingCardRoutine = false;
    }

    private void ClearRandomCardHighlight()
    {
        for (int i = 0; i < itemCardRandom.Length; i++)
        {
            itemCardRandom[i].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void HandlePlayerInput()
    {
        if (isPlayer1Choose)
        {
            HandlePlayer1Input();

            if (Input.GetKeyDown(KeyCode.J))
                BuyPlayer1Item();

            if (Input.GetKeyDown(KeyCode.K))
                SkipPlayer1();
        }

        if (isPlayer2Choose)
        {
            HandlePlayer2Input();

            if (Input.GetKeyDown(KeyCode.Keypad1))
                BuyPlayer2Item();

            if (Input.GetKeyDown(KeyCode.Keypad2))
                SkipPlayer2();
        }
    }

    #region Timeout

    public void TimeOutPlayer1()
    {
        if (!isPlayer1Choose) return;

        SkipPlayer1();
    }

    public void TimeOutPlayer2()
    {
        if (!isPlayer2Choose) return;

        SkipPlayer2();
    }

    public void TimeOutRandomCard()
    {
        if (!isChoosingRandomCard) return;

        StartChooseRandomCard();
    }

    #endregion

    #region Player 1

    private void HandlePlayer1Input()
    {
        IndexItem current = items[player1Index].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.W)) ChangePlayer1(current.top);
        if (Input.GetKeyDown(KeyCode.S)) ChangePlayer1(current.bottom);
        if (Input.GetKeyDown(KeyCode.A)) ChangePlayer1(current.left);
        if (Input.GetKeyDown(KeyCode.D)) ChangePlayer1(current.right);
    }

    private void ChangePlayer1(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length) return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);

        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

        player1Index = newIndex;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(true);
    }

    private void BuyPlayer1Item()
    {
        PriceItem item = items[player1Index].GetComponent<PriceItem>();
        if (item == null) return;

        if (player1Index == 1)
        {
            if (!shopManager.BuyItem(0, player1Index, item.price))
                return;

            shopManager.StopTurnTimer();

            isPlayer1Choose = false;
            items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

            OpenRandomCard(0);
            return;
        }

        if (shopManager.BuyItem(0, player1Index, item.price))
        {
            shopManager.StopTurnTimer();

            isPlayer1Choose = false;
            items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

            StartCoroutine(ShowPlayer2TurnDelay());
        }
    }

    private void SkipPlayer1()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.skipBuyClip);
        shopManager.StopTurnTimer();

        isPlayer1Choose = false;
        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

        StartCoroutine(ShowPlayer2TurnDelay());
    }

    #endregion

    #region Player 2

    private void HandlePlayer2Input()
    {
        IndexItem current = items[player2Index].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangePlayer2(current.top);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangePlayer2(current.bottom);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangePlayer2(current.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangePlayer2(current.right);
    }

    private void ChangePlayer2(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length) return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);

        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        player2Index = newIndex;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }

    private void BuyPlayer2Item()
    {
        PriceItem item = items[player2Index].GetComponent<PriceItem>();
        if (item == null) return;

        if (player2Index == 1)
        {
            if (!shopManager.BuyItem(1, player2Index, item.price))
                return;

            shopManager.StopTurnTimer();

            isPlayer2Choose = false;
            items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

            OpenRandomCard(1);
            return;
        }

        if (shopManager.BuyItem(1, player2Index, item.price))
        {
            shopManager.StopTurnTimer();

            isPlayer2Choose = false;
            items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

            CheckAllPlayersFinished();
        }
    }

    private void SkipPlayer2()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.skipBuyClip);
        shopManager.StopTurnTimer();

        isPlayer2Choose = false;
        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        CheckAllPlayersFinished();
    }

    #endregion

    #region Turn Delay

    private IEnumerator ShowPlayer2TurnDelay()
    {
        isPlayer1Choose = false;
        isPlayer2Choose = false;
        isChoosingRandomCard = false;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);
        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        shopManager.ShowPlayer2Turn();

        yield return new WaitForSeconds(0.6f);

        isPlayer2Choose = true;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);

        shopManager.StartTurnTimer(TimeOutPlayer2);
    }

    #endregion

    #region Random Card

    private void OpenRandomCard(int playerIndex)
    {
        shopManager.canvasRandomCard.SetActive(true);

        randomCardPlayer = playerIndex;
        randomCardIndex = 0;

        isChoosingRandomCard = false;
        isPlayer1Choose = false;
        isPlayer2Choose = false;
        isChoosingCardRoutine = false;

        RandomizeCards();
        ClearRandomCardHighlight();

        if (randomCardCoroutine != null)
            StopCoroutine(randomCardCoroutine);

        randomCardCoroutine = StartCoroutine(WaitOpenAnimation());
    }

    private IEnumerator WaitOpenAnimation()
    {
        Animator animator = shopManager.canvasRandomCard.GetComponent<Animator>();

        yield return null;

        if (animator != null)
        {
            yield return new WaitForSeconds(
                animator.GetCurrentAnimatorStateInfo(0).length + 0.1f
            );
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        isChoosingRandomCard = true;

        itemCardRandom[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);

        shopManager.StartTurnTimer(TimeOutRandomCard);
    }

    private void RandomizeCards()
    {
        int[] randomItems = { 0, 2, 3, 5 };

        for (int i = 0; i < randomItems.Length; i++)
        {
            int rand = Random.Range(i, randomItems.Length);

            (randomItems[i], randomItems[rand]) =
            (randomItems[rand], randomItems[i]);
        }

        for (int i = 0; i < itemCardRandom.Length; i++)
        {
            RandomCard card = itemCardRandom[i].GetComponent<RandomCard>();

            card.itemIndex = randomItems[i];
            card.image.sprite = card.spriteStar;
        }
    }

    private void HandleRandomCardInput()
    {
        IndexItem current = itemCardRandom[randomCardIndex].GetComponent<IndexItem>();

        if (randomCardPlayer == 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) ChangeRandomCard(current.left);
            if (Input.GetKeyDown(KeyCode.D)) ChangeRandomCard(current.right);

            if (Input.GetKeyDown(KeyCode.J))
                StartChooseRandomCard();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeRandomCard(current.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeRandomCard(current.right);

            if (Input.GetKeyDown(KeyCode.Keypad1))
                StartChooseRandomCard();
        }
    }

    private void ChangeRandomCard(int newIndex)
    {
        if (newIndex < 0 || newIndex >= itemCardRandom.Length) return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);

        itemCardRandom[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(false);

        randomCardIndex = newIndex;

        itemCardRandom[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);
    }

    private void StartChooseRandomCard()
    {
        if (isChoosingCardRoutine) return;

        StartCoroutine(ChooseRandomCard());
    }

    private IEnumerator ChooseRandomCard()
    {
        isChoosingCardRoutine = true;

        shopManager.StopTurnTimer();

        isChoosingRandomCard = false;

        RandomCard card = itemCardRandom[randomCardIndex].GetComponent<RandomCard>();

        itemCardRandom[randomCardIndex]
            .transform.GetChild(1)
            .gameObject.SetActive(true);

        card.image.sprite = shopManager.itemSprites[card.itemIndex];

        AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItemClip);

        shopManager.ShowPlayerItem(randomCardPlayer, card.itemIndex);

        yield return new WaitForSeconds(2f);

        ClearRandomCardHighlight();

        shopManager.canvasRandomCard.SetActive(false);

        isChoosingCardRoutine = false;

        if (randomCardPlayer == 0)
        {
            StartCoroutine(ShowPlayer2TurnDelay());
        }
        else
        {
            isPlayer2Choose = false;
            items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

            CheckAllPlayersFinished();
        }
    }

    #endregion

    #region Close Shop

    private void CheckAllPlayersFinished()
    {
        if (!isPlayer1Choose && !isPlayer2Choose && !isChoosingRandomCard)
        {
            StartCoroutine(CloseShop());
        }
    }

    public IEnumerator CloseShop()
    {
        shopManager.StopTurnTimer();

        yield return new WaitForSeconds(2f);

        ShopManager.Instance.CloseShop();
    }

    #endregion
}