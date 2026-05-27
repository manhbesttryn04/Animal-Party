using UnityEngine;

public class AnswerPad : MonoBehaviour
{
    public MathManager mathManager;
    public int padIndex; // 0=A, 1=B, 2=C, 3=D

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm xem đối tượng va chạm là Player 1 hay Player 2 dựa vào script PlayerController gắn trên nó
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                // Gửi thông tin: Ai dậm (Player1/Player2) và Dậm ô nào (padIndex)
                mathManager.PlayerChoose(pc.playerId, padIndex);
            }
        }
    }
}