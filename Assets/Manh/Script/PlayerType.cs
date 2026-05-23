using UnityEngine;

public class PlayerType : MonoBehaviour
{
    public bool isPlayer2 = false;
    private void Awake()
    {
        if (!isPlayer2)
        {
            gameObject.tag = "Player 1";

        }
        else gameObject.tag = "Player 2";
    }
}
