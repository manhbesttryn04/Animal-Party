using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerAnimator playerAnimator;
    public PlayerMove playerMove;
    public PlayerAttack playerAttack;
    public PlayerType playerType;
    public PlayerDiceRoll playerDiceRoll;
    public PlayerMoveAI playerMoveAI;
    public PlayerCamera playerCamera;
    public PlayerRound playerRound;
    public PlayerNotifi playerNotifi;
    public PlayerInputDice playerInputDice;
    public PlayerCoin playerCoin;
    public PlayerBuff playerBuff;
    public PlayerDebuff playerDebuff;
    public PlayerFunnyItem playerFunnyItem;
    public PlayerMiniGame playerMiniGame;
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
        playerBuff = GetComponent<PlayerBuff>();
        playerDebuff= GetComponent<PlayerDebuff>();
        playerAttack = GetComponent<PlayerAttack>();
        playerFunnyItem = GetComponent<PlayerFunnyItem>();
        playerMiniGame = GetComponent<PlayerMiniGame>();

    }
}
