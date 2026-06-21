using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class TrapAndCoin : MonoBehaviour
{
    public GameObject bom;
    public GameObject coin;
    public bool hasBom;
    public bool hasCoin;

    public void Start()
    {
        SetUpItems();
    }


    public void SetUpItems()
    {
        if (hasBom)
        {
            bom.SetActive(true);
        }else bom.SetActive(false);

        if (hasCoin)
        {
            coin.SetActive(true);
        }else coin.SetActive(false);
    }

    public void BomActivated()
    {
        Bomb b = bom.GetComponent<Bomb>();
        if(b != null)
        {
            b.TriggerBomb();
            hasBom = false;
        }
    }
    public void CoinActivated(int i)
    {
       Coin c = coin.GetComponent<Coin>();
        if(c != null)
        {
            c.TriggerCoin(i);
            hasCoin = false;
        }
    }
}
