using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputDice : MonoBehaviour
{ 
    public PlayerManager manager;
    public bool isClick = true;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
       
    }
    public void Update()
    {
        ExitsInput();
    }
    public void ExitsInput()
    {
        if (!manager.playerType.isPlayer2 && Input.GetKey(KeyCode.E) && !isClick)
        {
            manager.playerAnimator.playerAnimator.SetTrigger("Dice");
            isClick = true;
        }
        else if (manager.playerType.isPlayer2 && Input.GetKey(KeyCode.Keypad1) && !isClick)
        {

            manager.playerAnimator.playerAnimator.SetTrigger("Dice");
            isClick = true;
        }
    }

}
