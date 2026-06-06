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
        AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMainClip);
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
        player1.GetComponent<PlayerManager>().playerCamera.isFllow2= true;
         yield return new WaitForSeconds(2f);
        player1.GetComponent<PlayerManager>().playerCamera.isFllow2 = false;
        yield return new WaitForSeconds(0.5f);
     yield return  (StartCoroutine( player1.GetComponent<PlayerManager>().playerNotifi.SetNotifi()));
        player1.GetComponent<PlayerManager>().playerInputDice.isClick = false;
    }
    public IEnumerator FistRoundPlayer2()
    {  
        yield return new WaitForSeconds(0.5f);
        player2.GetComponent<PlayerManager>().playerCamera.isFllow2 = true;
        yield return new WaitForSeconds(0.5f);
        player2.GetComponent<PlayerManager>().playerCamera.isFllow2 = false;
       yield return (StartCoroutine(player2.GetComponent<PlayerManager>().playerNotifi.SetNotifi()));
        player2.GetComponent<PlayerManager>().playerInputDice.isClick = false;

    }
}

  
