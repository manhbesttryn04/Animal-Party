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
        if (index >= 32)
        {
          
            return true;
        }

        else
        {
          
            return false;
        }

    }
    public bool CheckWinnerByCoinPower(int coin)
    {
        if (coin >= 4)
        {
           
            return true;
        }
        else
        {
           
            return false;
        }

    }
}