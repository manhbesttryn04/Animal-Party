using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerAnimator playerAnimator;
    public PlayerMove playerMove;
    public PlayerType playerType;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerType = GetComponent<PlayerType>();

    }
}
