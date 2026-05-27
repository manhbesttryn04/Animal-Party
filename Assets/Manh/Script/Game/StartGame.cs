using System.Collections;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    bool isFistRoundSussce = false;
    void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player 1");
        player2 = GameObject.FindGameObjectWithTag("Player 2");
        StartCoroutine(FistRoundPlayer1());
    }
    private void Update()
    {
        if(!isFistRoundSussce && player1.GetComponent<PlayerManager>().playerRound.isRound1)
        {
            StartCoroutine(FistRoundPlayer2());
            isFistRoundSussce=true;
        }
    }

    // Update is called once per frame
    public IEnumerator FistRoundPlayer1()
    {
        yield return new WaitForSeconds(2f);
        player1.GetComponent<PlayerManager>().playerAnimator.playerAnimator.SetTrigger("Dice");
    }
    public IEnumerator FistRoundPlayer2()
    {
        yield return new WaitForSeconds(2f);
        player2.GetComponent<PlayerManager>().playerAnimator.playerAnimator.SetTrigger("Dice");

    }
}

  
