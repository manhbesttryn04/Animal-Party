using System.Collections;
using UnityEngine;

public class PlayerNotifi : MonoBehaviour
{
    public PlayerManager manager;
    public GameObject NotifiPlayer;
    public GameObject DicePlayer;

    public void Start()
    {
        manager = GetComponent<PlayerManager>();
        if (!manager.playerType.isPlayer2)
        {
            NotifiPlayer = GameObject.Find("Not Player 1");
            DicePlayer = GameObject.Find("Dice Player 1");
        }else
        {
            NotifiPlayer = GameObject.Find("Not Player 2");
            DicePlayer = GameObject.Find("Dice Player 2");
        }
        NotifiPlayer.SetActive(false);
        DicePlayer.SetActive(false);
    }

  public IEnumerator SetNotifi()
    {
        NotifiPlayer.SetActive(true);
        yield return new WaitForSeconds(1);
        DicePlayer.SetActive(true);
        NotifiPlayer.SetActive(false);
    }
    public void SetDice()
    {
        DicePlayer.SetActive(false);
    }

}
