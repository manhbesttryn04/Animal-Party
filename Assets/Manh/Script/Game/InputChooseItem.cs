using UnityEngine;

public class InputChooseItem : MonoBehaviour
{
    public GameObject[] items;

    [Header("Player Index")]
    public int player1Index = 0;
    public int player2Index = 0;

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
        HandlePlayer1Input();
        HandlePlayer2Input();
    }

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

    private void ChangePlayer1(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length)
            return;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(false);

        player1Index = newIndex;

        items[player1Index].transform.GetChild(1).gameObject.SetActive(true);
    }

    private void ChangePlayer2(int newIndex)
    {
        if (newIndex < 0 || newIndex >= items.Length)
            return;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(false);

        player2Index = newIndex;

        items[player2Index].transform.GetChild(2).gameObject.SetActive(true);
    }
}