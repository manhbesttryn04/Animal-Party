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

    public bool isPlayer2 = false;
    public bool isUseManager = false;



    public void Awake()
    {
        if(isUseManager)
        {
            string characterName = SendPlayerWinner.Instance.characterName;
            bool isPlayer2 = SendPlayerWinner.Instance.isPlayer2;
            SetCharacterWinner(characterName, isPlayer2);
        }
       

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
   
   public void SetCharacterWinner(string name, bool isPlayer2)
   {
       switch (name)
       {
           case "Seagull":
               isSeaGullUnlocked = true;
               break;
           case "Bat":
               isBatUnlocked = true;
               break;
           case "Rabbit":
               isRabbitUnlocked = true;
               break;
           case "Leopard":
               isLeoPardUnlocked = true;
               break;
           case "Sloth":
               isSlothUnlocked = true;
               break;
           default:
             
               break;
       }
       this.isPlayer2 = isPlayer2;
   }
}