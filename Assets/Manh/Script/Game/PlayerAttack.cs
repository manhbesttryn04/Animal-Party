using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;

    [Header("Attack Settings")]
    public BoxCollider kickCollider;
    public float attackCooldown = 2f;
  

    private bool canAttack = true;

    private void Start()
    {
        if (kickCollider != null)
            kickCollider.enabled = false;
    }

    private void Update()
    {
        bool attackPressed = false;

        // Player 1
        if (!playerManager.playerType.isPlayer2)
        {
            attackPressed = Input.GetKeyDown(KeyCode.J);
        }
        // Player 2
        else
        {
            attackPressed = Input.GetKeyDown(KeyCode.Keypad1);
        }

        if (attackPressed && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        canAttack = false;

        if (playerManager != null &&
            playerManager.playerAnimator.playerAnimator != null)
        {
            playerManager.playerAnimator.playerAnimator.SetTrigger("Attack");
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    // Animation Event
    public void EnableKickCollider()
    {
        if (kickCollider != null)
            kickCollider.enabled = true;
    }

    // Animation Event
    public void DisableKickCollider()
    {
        if (kickCollider != null)
            kickCollider.enabled = false;
    }

   
}