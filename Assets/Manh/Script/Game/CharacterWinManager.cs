using UnityEngine;

public class CharacterWinManager : MonoBehaviour
{
    public GameObject seaGullCharacter;
    public GameObject batCharacter;
    public GameObject rabbitCharacter;
    public GameObject leoPardCharacter;
    public GameObject slothCharacter;
    public bool isSeaGullUnlocked;
    public bool isLeoPardUnlocked;
    public bool isSlothUnlocked;
    public bool isBatUnlocked;
    public bool isRabbitUnlocked;



    public void Awake()
    {
        UnLockedCharacterWinner();
    }

    public void UnLockedCharacterWinner()
    {
        if(isSeaGullUnlocked)
        {
            seaGullCharacter.SetActive(true);
        }
        if(isBatUnlocked)
        {
            batCharacter.SetActive(true);
        }
        if(isRabbitUnlocked)
        {
            rabbitCharacter.SetActive(true);
        }
        if(isLeoPardUnlocked)
        {
            leoPardCharacter.SetActive(true);
        }
        if(isSlothUnlocked)
        {
            slothCharacter.SetActive(true);
        }
    }
}
