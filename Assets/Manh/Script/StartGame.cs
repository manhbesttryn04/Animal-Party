using System.Collections;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player 1");
        player2 = GameObject.FindGameObjectWithTag("Player 2");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

  
