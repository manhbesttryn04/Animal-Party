using UnityEngine;

public class PlayerMiniGame: MonoBehaviour
{
    public Transform checkPoint;

    public float waterHeight = -5f;

  

    public void Respawn()
    {
        CharacterController cc =
            GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        transform.position =
            checkPoint.position;

        if (cc != null)
            cc.enabled = true;
        PlayerCoin coin = GetComponent<PlayerCoin>();
        coin.TakeCoin(5);
    }
}