using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerAnimator playerAnimator;
    public PlayerMove playerMove;
    public PlayerType playerType;
    public PlayerDiceRoll playerDiceRoll;
    public PlayerMoveAI playerMoveAI;
    public PlayerCamera playerCamera;
    public PlayerRound playerRound;
    public PlayerNotifi playerNotifi;
    public PlayerInputDice playerInputDice;
    public PlayerCoin playerCoin;
    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerType = GetComponent<PlayerType>();
        playerMoveAI = GetComponent<PlayerMoveAI>();
        playerDiceRoll = GetComponent<PlayerDiceRoll>();
        playerCamera = GetComponent<PlayerCamera>();
        playerRound = GetComponent< PlayerRound>();
        playerNotifi = GetComponent<PlayerNotifi>();
        playerInputDice = GetComponent<PlayerInputDice>();
            playerCoin = GetComponent<PlayerCoin>();

    }
}
