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

    public bool isPlayer1Choose = true;
    public bool isPlayer2Choose = true;

    #endregion

    #region Random Card State

    private bool isChoosingRandomCard = false;
    private int randomCardIndex = 0;
    private int randomCardPlayer = -1; // 0 = P1, 1 = P2

    #endregion

    #region Unity

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

    #endregion

    #region Init

    private void InitHighlight()
    {
        for (int i = 1; i < items.Length; i++)
        {
            items[i].transform.GetChild(1).gameObject.SetActive(false);
            items[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        items[player1Index].transform.GetChild(1).gameObject.SetActive(true);
        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }

    #endregion

    #region Player Input Router

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

            OpenRandomCard(0);
            return;
        }

        if (shopManager.BuyItem(0, player1Index, item.price))
        {
            isPlayer1Choose = false;
            isPlayer2Choose = true;

            shopManager.ShowPlayer2Turn();

            items[player1Index].transform.GetChild(1).gameObject.SetActive(false);
            CheckAllPlayersFinished();
        }
    }
    private void SkipPlayer1()
    {
        isPlayer1Choose = false;
        isPlayer2Choose = true;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

        shopManager.ShowPlayer2Turn();

        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }

    private void SkipPlayer2()
    {
        isPlayer2Choose = false;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        CheckAllPlayersFinished();
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
            OpenRandomCard(1);
            return;
        }

        if (shopManager.BuyItem(1, player2Index, item.price))
        {
            isPlayer2Choose = false;
            items[player2Index].transform.GetChild(2).gameObject.SetActive(false);
            CheckAllPlayersFinished();
        }
    }

    #endregion

    #region Random Card

    private void OpenRandomCard(int playerIndex)
    {
        shopManager.canvasRandomCard.SetActive(true);
      

        randomCardPlayer = playerIndex;
        randomCardIndex = 0;

        isChoosingRandomCard = true;
        isPlayer1Choose = false;
        isPlayer2Choose = false;

        RandomizeCards();

        int child = playerIndex == 0 ? 1 : 2;

        itemCardRandom[0].transform.GetChild(child).gameObject.SetActive(true);
    }

    private void RandomizeCards()
    {
        int[] randomItems = { 0, 2, 3, 5 };

        for (int i = 0; i < randomItems.Length; i++)
        {
            int rand = Random.Range(i, randomItems.Length);

            (randomItems[i], randomItems[rand]) = (randomItems[rand], randomItems[i]);
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
            if (Input.GetKeyDown(KeyCode.J)) StartCoroutine(ChooseRandomCard());
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeRandomCard(current.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeRandomCard(current.right);
            if (Input.GetKeyDown(KeyCode.Keypad1)) StartCoroutine(ChooseRandomCard());
        }
    }

    private void ChangeRandomCard(int newIndex)
    {
        if (newIndex < 0 || newIndex >= itemCardRandom.Length) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);
        itemCardRandom[randomCardIndex].transform.GetChild(1).gameObject.SetActive(false);

        randomCardIndex = newIndex;

        itemCardRandom[randomCardIndex].transform.GetChild(1).gameObject.SetActive(true);
    }

    private IEnumerator ChooseRandomCard()
    {
        isChoosingRandomCard = false;

        RandomCard card = itemCardRandom[randomCardIndex].GetComponent<RandomCard>();

        itemCardRandom[randomCardIndex].transform.GetChild(2).gameObject.SetActive(true);

        card.image.sprite = shopManager.itemSprites[card.itemIndex];
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItemClip);

        shopManager.ShowPlayerItem(randomCardPlayer, card.itemIndex);

        yield return new WaitForSeconds(2f);

        shopManager.canvasRandomCard.SetActive(false);

        if (randomCardPlayer == 0)
        {
            isPlayer1Choose = false;
            isPlayer2Choose = true;

            shopManager.ShowPlayer2Turn();

            items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            isPlayer2Choose = false;
            CheckAllPlayersFinished();
        }
    }
    private void CheckAllPlayersFinished()
    {
        if (!isPlayer1Choose && !isPlayer2Choose)
        {

            StartCoroutine(CloseShop());
        }
    }
    public IEnumerator CloseShop()
    {
        yield return new WaitForSeconds(2f);
        ShopManager.Instance.CloseShop();
    }

    #endregion
}