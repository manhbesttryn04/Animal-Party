using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;

    [Header("Attack Settings")]
    public BoxCollider kickCollider;
    public float attackCooldown = 2f;
    public float aimDistance = 3f;

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
        Transform target = FindTarget();

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // Nếu đối thủ đủ gần thì tự động xoay mặt về phía đối thủ
            if (distance <= aimDistance)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0f;

                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
        else Debug.Log("Ko");

            canAttack = false;

        if (playerManager != null &&
            playerManager.playerAnimator.playerAnimator != null)
        {
            playerManager.playerAnimator.playerAnimator.SetTrigger("Attack");
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private Transform FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player 1");

        Transform nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            // Bỏ qua chính mình
            if (p == gameObject)
                continue;

            float distance = Vector3.Distance(transform.position, p.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = p.transform;
            }
        }

        return nearest;
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