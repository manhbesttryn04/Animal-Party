using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDiceRoll : MonoBehaviour
{
    public PlayerManager manager;
    public List<GameObject> dices = new List<GameObject>();

    public GameObject diceRandom;

    public int currentDiceNumber;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
    }
    public void SetDiceRandom(int i) { if (i == 0) { diceRandom.SetActive(false); } else diceRandom.SetActive(true); }
   

    public void RandomDice()
    {
       
        int ran = Random.Range(0, 6);

        // Tắt hết
        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].SetActive(false);
        }

        // Hiện mặt xúc xắc random
        dices[ran].SetActive(true);

        currentDiceNumber = ran + 1;

        Debug.Log("Dice Number: " + currentDiceNumber);

        // Sau 3 giây tắt hết
        StartCoroutine(HideDiceAfterTime(ran));
    }

    IEnumerator HideDiceAfterTime(int a)
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        manager.playerMoveAI.isMoving = true;
     StartCoroutine(manager.playerMoveAI.AIToPoint(a));
      

    }
}