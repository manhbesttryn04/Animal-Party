using UnityEngine;

public class InputChooseItem : MonoBehaviour
{
    public GameObject[] items;

    [Header("References")]
    public ShopManager shopManager;

    [Header("Player Index")]
    public int player1Index = 0;
    public int player2Index = 0;

    [Header("Can Choose")]
    public bool isPlayer1Choose = true;
    public bool isPlayer2Choose = true;

    private void Start()
    {
        // Tắt toàn bộ highlight
        for (int i = 1; i < items.Length; i++)
        {
            items[i].transform.GetChild(1).gameObject.SetActive(false); // P1
            items[i].transform.GetChild(2).gameObject.SetActive(false); // P2
        }

        // Bật highlight mặc định
        items[player1Index].transform.GetChild(1).gameObject.SetActive(true);
        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }

    private void Update()
    {
        // =========================
        // PLAYER 1
        // =========================
        if (isPlayer1Choose)
        {
            HandlePlayer1Input();

            if (Input.GetKeyDown(KeyCode.J))
            {
                BuyPlayer1Item();
            }
        }

        // =========================
        // PLAYER 2
        // =========================
        if (isPlayer2Choose)
        {
            HandlePlayer2Input();

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                BuyPlayer2Item();
            }
        }
    }

    // =========================
    // PLAYER 1 INPUT
    // =========================
    private void HandlePlayer1Input()
    {
        IndexItem current = items[player1Index].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.W))
            ChangePlayer1(current.top);

        if (Input.GetKeyDown(KeyCode.S))
            ChangePlayer1(current.bottom);

        if (Input.GetKeyDown(KeyCode.A))
            ChangePlayer1(current.left);

        if (Input.GetKeyDown(KeyCode.D))
            ChangePlayer1(current.right);
    }

    // =========================
    // PLAYER 2 INPUT
    // =========================
    private void HandlePlayer2Input()
    {
        IndexItem current = items[player2Index].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangePlayer2(current.top);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangePlayer2(current.bottom);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ChangePlayer2(current.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ChangePlayer2(current.right);
    }

    // =========================
    // CHANGE PLAYER 1
    // =========================
    private void ChangePlayer1(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length)
            return;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

        player1Index = newIndex;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(true);
    }

    // =========================
    // CHANGE PLAYER 2
    // =========================
    private void ChangePlayer2(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length)
            return;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        player2Index = newIndex;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }

    // =========================
    // BUY PLAYER 1 ITEM
    // =========================
    private void BuyPlayer1Item()
    {
        PriceItem item = items[player1Index].GetComponent<PriceItem>();

        if (item == null)
        {
            Debug.LogError("Item chưa có script PriceItem");
            return;
        }

        bool success = shopManager.BuyItem(
            0,                  // Player 1
            player1Index -1,       // Item Index
            item.price          // Giá
        );

        if (success)
        {
            isPlayer1Choose = false;

            items[player1Index]
                .transform.GetChild(1)
                .gameObject.SetActive(false);
        }
    }

    // =========================
    // BUY PLAYER 2 ITEM
    // =========================
    private void BuyPlayer2Item()
    {
        PriceItem item = items[player2Index].GetComponent<PriceItem>();

        if (item == null)
        {
            Debug.LogError("Item chưa có script PriceItem");
            return;
        }

        bool success = shopManager.BuyItem(
            1,                  // Player 2
            player2Index-1,       // Item Index
            item.price          // Giá
        );

        if (success)
        {
            isPlayer2Choose = false;

            items[player2Index]
                .transform.GetChild(2)
                .gameObject.SetActive(false);
        }
    }
}