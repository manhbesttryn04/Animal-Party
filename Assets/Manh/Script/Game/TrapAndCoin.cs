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

    private void Start()
    {
        SetUpItems();
    }

    public void SetUpItems()
    {
        if (hasBom)
            bom.SetActive(true);
        else
            bom.SetActive(false);

        if (hasCoin)
            coin.SetActive(true);
        else
            coin.SetActive(false);

        if (hasTelep)
        {
            if (teleport == null) return;

            teleport.gameObject.SetActive(true);
            teleport.loop = true;
            teleport.Play();
        }
        else
        {
            if (teleport != null)
                teleport.gameObject.SetActive(false);
        }
    }

    public void BomActivated()
    {
        Bomb b = bom.GetComponent<Bomb>();

        if (b != null)
        {
            b.TriggerBomb();
            hasBom = false;
        }
    }

    public void CoinActivated(int i)
    {
        Coin c = coin.GetComponent<Coin>();

        if (c != null)
        {
            c.TriggerCoin(i);
            hasCoin = false;
        }
    }

    public void TelepActivated(bool isPlayer2)
    {
        if (!hasTelep || teleport == null)
            return;

        hasTelep = false;

        StartCoroutine(TeleportRoutine(isPlayer2));
    }

    private IEnumerator TeleportRoutine(bool isPlayer2)
    {
        TeleportAllPlayer teleportManager = teleport.GetComponent<TeleportAllPlayer>();

        if (teleportManager != null)
        {
            yield return StartCoroutine(teleportManager.TeleportPlayers(isPlayer2));
        }

        teleport.loop = false;

        yield return new WaitForSeconds(1f);

        teleport.gameObject.SetActive(false);
    }
}