using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    public int indexPlayer1;
    public int indexPlayer2;
    public List<GameObject> player1List;
    public List<GameObject> player2List;
    public List<GameObject> playerPlaylist;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
        }
        
      //  indexPlayer1 = SendIndexCharacter.Instance.player1Index;
       // indexPlayer2 = SendIndexCharacter.Instance.player2Index;
        SetUpPlayer();
        
    }

    public void SetUpPlayer()
    {
        player1List[indexPlayer1].SetActive(true);
        player2List[indexPlayer2].SetActive(true);
    }
}
