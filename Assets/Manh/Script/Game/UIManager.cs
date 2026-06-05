using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance { get; private set; }
    public GameObject resultPanel;
    public TextMeshProUGUI coinTextP1;
    public TextMeshProUGUI coinTextP2;
    public GameObject player1ResultUI;
    public GameObject player2ResultUI;

    public GameObject notifiPanel;
    public TextMeshProUGUI textNotifi;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateResultPanel(int coinP1, int coinP2)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (coinTextP1 != null)
                coinTextP1.text = $"{coinP1}";
            if (coinTextP2 != null)
                coinTextP2.text = $"{coinP2}";
            if (coinP1 > coinP2)
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(true);

            }
            else if (coinP1 < coinP2)
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                player1ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player1ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                player2ResultUI.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                player2ResultUI.gameObject.transform.GetChild(1).gameObject.SetActive(false);

            }
        }
    }
    public void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    public void SendNotifi(string message)
    {
        if (notifiPanel != null && textNotifi != null)
        {
            notifiPanel.SetActive(true);
            textNotifi.text = message;
           
            Invoke(nameof(HideNotifi), 2f);
        }
    }

    private void HideNotifi()
    {
        if (notifiPanel != null)
        {
            notifiPanel.SetActive(false);
        }
    }
}