using UnityEngine;

public class CheckWinPlayer : MonoBehaviour
{
    public static CheckWinPlayer Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CheckWinnerByIndex(int index)
    {
        if (index >= 33)
        {
            Debug.Log("Player Win");
            return true;
        }

        else
        {
            Debug.Log("Chua du 33 buoc");
            return false;
        }

    }
    public bool CheckWinnerByCoinPower(int coin)
    {
        if (coin >= 4)
        {
            Debug.Log("Nguoi choi da du 4 dong coin");
            return true;
        }
        else
        {
            Debug.Log("Chua du 4 coin");
            return false;
        }

    }
}