using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTrapState : MonoBehaviour
{
    public PlayerManager manager;
    public bool isTrapActive = false;

    public void Start()
    {
        manager = GetComponent<PlayerManager>();
    }

    public IEnumerator CheckCurrentTile()
    {
        PlayerMoveAI a = manager.playerMoveAI;
        TrapAndCoin trap = a.pointCheck[a.currentIndex].GetComponentInChildren<TrapAndCoin>();

        if (trap == null)
            yield break;

        isTrapActive = true;

        try
        {
            //==========================
            // Bomb
            //==========================
            if (trap.hasBom)
            {
                trap.BomActivated();

                yield return new WaitForSeconds(1f);

                yield return StartCoroutine(a.BoomHitEffect(3));

                yield break;
            }

            //==========================
            // Teleport
            //==========================
            if (trap.hasTelep)
            {
                trap.TelepActivated();

                yield return StartCoroutine(trap.TeleportRoutine(manager.playerType.isPlayer2));

                yield break;
            }

            //==========================
            // Coin
            //==========================
            if (trap.hasCoin)
            {
                trap.CoinActivated(manager.playerType.isPlayer2 ? 1 : 0);
                manager.playerCoin.coinEndMiniGame += 100;

                yield return new WaitForSeconds(0.5f);
            }
        }
        finally
        {
            isTrapActive = false;
        }
    }
}