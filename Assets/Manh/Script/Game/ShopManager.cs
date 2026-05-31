using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Item Data")]
    public Sprite[] itemSprites;

    [Header("Players")]
    public PlayerManager[] players;

    [Header("UI")]
    public TextMeshProUGUI[] playerCoinTexts;
    public Image[] playerItemImages;

    [Header("Shop Timer")]
    public int timeToBuy = 99;
    public int startTime = 0;

    private void Start()
    {
        // Đảm bảo mảng đủ 2 player
        if (players == null || players.Length < 2)
            players = new PlayerManager[2];

        // Tìm Player 1
        GameObject p1 = GameObject.FindGameObjectWithTag("Player 1");
        if (p1 != null)
        {
            players[0] = p1.GetComponent<PlayerManager>();
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject có tag 'Player 1'");
        }

        // Tìm Player 2
        GameObject p2 = GameObject.FindGameObjectWithTag("Player 2");
        if (p2 != null)
        {
            players[1] = p2.GetComponent<PlayerManager>();
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject có tag 'Player 2'");
        }

        // Cập nhật coin ban đầu
        UpdateCoin();

        // Ẩn toàn bộ icon item
        if (playerItemImages != null)
        {
            foreach (Image img in playerItemImages)
            {
                if (img != null)
                    img.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateCoin()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            if (playerCoinTexts[i] == null)
                continue;

            playerCoinTexts[i].text =
                players[i].playerCoin.coinEndMiniGame.ToString();
        }
    }

    public void ShowPlayerItem(int playerIndex, int itemIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerItemImages.Length)
            return;

        if (itemIndex < 0 || itemIndex >= itemSprites.Length)
            return;

        playerItemImages[playerIndex].sprite = itemSprites[itemIndex];
        playerItemImages[playerIndex].gameObject.SetActive(true);
    }

    private IEnumerator FlashCoinText(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerCoinTexts.Length)
            yield break;

        TextMeshProUGUI text = playerCoinTexts[playerIndex];

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

        text.color = originalColor;
    }

    public bool BuyItem(int playerIndex, int itemIndex, int price)
    {
        if (playerIndex < 0 || playerIndex >= players.Length)
            return false;

        PlayerManager player = players[playerIndex];

        if (player == null)
            return false;

        // Không đủ tiền
        if (player.playerCoin.coinEndMiniGame < price)
        {
            StartCoroutine(FlashCoinText(playerIndex));
            return false;
        }

        // Trừ tiền
        player.playerCoin.coinEndMiniGame -= price;

        // Cập nhật UI
        UpdateCoin();

        // Hiển thị item đã mua
        ShowPlayerItem(playerIndex, itemIndex);

        return true;
    }
}