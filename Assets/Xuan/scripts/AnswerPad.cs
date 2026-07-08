using UnityEngine;

public class AnswerPad : MonoBehaviour
{
    public MathManager mathManager;
    public int padIndex; // 0=A, 1=B, 2=C, 3=D

    // Sử dụng OnTriggerStay để liên tục cập nhật ô lựa chọn khi player đang đứng trong ô
    private void OnTriggerStay(Collider other)
    {
        if (mathManager == null) return;

        // Kiểm tra xem đối tượng va chạm có gắn script PlayerType không
        PlayerType playerType = other.GetComponent<PlayerType>();

        if (playerType != null)
        {
            // Liên tục gửi tín hiệu cập nhật: Player nào đang đứng trên ô index nào
            mathManager.OnPlayerStepOnPad(playerType.isPlayer2, padIndex);
        }
    }
}