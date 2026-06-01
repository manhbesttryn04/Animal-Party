using UnityEngine;

public class AnswerPad : MonoBehaviour
{
    public MathManager mathManager;
    public int padIndex; // 0=A, 1=B, 2=C, 3=D

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có gắn script PlayerType của bạn không
        PlayerType playerType = other.GetComponent<PlayerType>();

        if (playerType != null)
        {
            // Truyền thông tin: là Player 2 (true) hay Player 1 (false), kèm theo chỉ số ô dậm
            mathManager.OnPlayerStepOnPad(playerType.isPlayer2, padIndex);
        }
    }
}