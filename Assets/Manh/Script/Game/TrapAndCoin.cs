using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using System.Collections;
using UnityEngine;

public class TrapAndCoin : MonoBehaviour
{
    public GameObject bom;
    public GameObject coin;
    public ParticleSystem teleport;
    public bool hasBom;
    public bool hasCoin;
    public bool hasTelep;

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

        if (hasTelep)
        { 
            if (teleport == null) return;
            teleport.gameObject.SetActive(true);
            teleport.Play();
        }else if(teleport != null) teleport.gameObject.SetActive(false);
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
    public void TelepActivated()
    {
        if (!hasTelep || teleport == null) return;

        hasTelep = false;

        StartCoroutine(TeleportOff());
    }

    private IEnumerator TeleportOff()
    {
        // Không phát lại vòng lặp
        teleport.loop = false;

        // Chờ 1 giây
        yield return new WaitForSeconds(1f);

        // Tắt GameObject
        teleport.gameObject.SetActive(false);
    }
}
