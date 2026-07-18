using UnityEngine;

public class SendIndexCharacter : MonoBehaviour
{
    public static SendIndexCharacter Instance;

    public int player1Index;
    public int player2Index;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

   
}