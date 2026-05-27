using UnityEngine;
using TMPro;
using System.Collections;

public class MathManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts; // 4 ô chữ đáp án (A, B, C, D)
    public TextMeshProUGUI timerText;

    [Header("Player Status UI")]
    public TextMeshProUGUI p1StatusText;
    public TextMeshProUGUI p2StatusText;

    private int correctAnswer;
    private int correctPadIndex;

    // Lựa chọn hiện tại của 2 Player (-1 nghĩa là chưa chọn)
    private int p1Choice = -1;
    private int p2Choice = -1;
    private bool isAnsweringState = true;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            // ---- BƯỚC 1: TẠO CÂU HỎI MỚI ----
            GenerateQuestion();
            isAnsweringState = true;
            p1Choice = -1;
            p2Choice = -1;

            p1StatusText.text = "";
            p2StatusText.text = "";

            // ---- BƯỚC 2: ĐẾM NGƯỢC 10 GIÂY CHO PHÉP TRẢ LỜI ----
            float timeLeft = 5f;
            while (timeLeft > 0)
            {
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();
                yield return new WaitForSeconds(1.0f);
                timeLeft -= 1f;
            }

            // ---- BƯỚC 3: HẾT GIỜ CHỌN -> HIỂN THỊ KẾT QUẢ VÀ ĐỔI MÀU CHỮ ----
            isAnsweringState = false;
            CheckFinalResults();

            // ---- BƯỚC 4: ĐẾM NGƯỢC 10 GIÂY TRƯỚC KHI ĐỔI CÂU ----
            float waitTimeLeft = 5f;
            while (waitTimeLeft > 0)
            {
                timerText.text = Mathf.CeilToInt(waitTimeLeft).ToString();
                yield return new WaitForSeconds(1.0f);
                waitTimeLeft -= 1f;
            }
        }
    }

    void GenerateQuestion()
    {
        int num1 = Random.Range(1, 21);
        int num2 = Random.Range(1, 21);
        int operation = Random.Range(0, 4);
        string opSymbol = "";

        switch (operation)
        {
            case 0: opSymbol = "+"; correctAnswer = num1 + num2; break;
            case 1: opSymbol = "-"; correctAnswer = num1 - num2; break;
            case 2:
                num1 = Random.Range(1, 10); num2 = Random.Range(1, 10);
                opSymbol = "x"; correctAnswer = num1 * num2;
                break;
            case 3:
                int k = Random.Range(1, 10); num2 = Random.Range(1, 10);
                num1 = num2 * k; opSymbol = "/"; correctAnswer = num1 / num2;
                break;
        }

        questionText.text = num1 + " " + opSymbol + " " + num2 + " = ?";
        correctPadIndex = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            // Reset lại màu chữ thành màu TRẮNG cho tất cả 4 ô khi có câu hỏi mới
            answerTexts[i].color = Color.white;

            if (i == correctPadIndex)
            {
                answerTexts[i].text = correctAnswer.ToString();
            }
            else
            {
                int wrongAnswer = correctAnswer + Random.Range(-5, 6);
                if (wrongAnswer == correctAnswer) wrongAnswer += 1;
                answerTexts[i].text = wrongAnswer.ToString();
            }
        }
    }

    public void PlayerChoose(PlayerController.PlayerId pId, int padIndex)
    {
        if (!isAnsweringState) return;

        if (pId == PlayerController.PlayerId.Player1)
        {
            if (p1Choice != -1) return;
            p1Choice = padIndex;
            p1StatusText.text = "P1 Đã khóa đáp án!";
        }
        else if (pId == PlayerController.PlayerId.Player2)
        {
            if (p2Choice != -1) return;
            p2Choice = padIndex;
            p2StatusText.text = "P2 Đã khóa đáp án!";
        }
    }

    // Xử lý logic hiển thị màu sắc khi hết 10 giây chọn
    void CheckFinalResults()
    {
        // 1. Kiểm tra kết quả chữ thông báo cho Player 1
        if (p1Choice == -1) p1StatusText.text = "P1: Không trả lời!";
        else if (p1Choice == correctPadIndex) p1StatusText.text = "P1: ĐÚNG!";
        else p1StatusText.text = "P1: SAI!";

        // 2. Kiểm tra kết quả chữ thông báo cho Player 2
        if (p2Choice == -1) p2StatusText.text = "P2: Không trả lời!";
        else if (p2Choice == correctPadIndex) p2StatusText.text = "P2: ĐÚNG!";
        else p2StatusText.text = "P2: SAI!";

        // 3. ĐỔI MÀU CHỮ CỦA 4 Ô ĐÁP ÁN:
        for (int i = 0; i < 4; i++)
        {
            // Nếu là ô đáp án đúng -> Đổi sang màu VÀNG
            if (i == correctPadIndex)
            {
                answerTexts[i].color = Color.yellow;
            }
            // Nếu ô này có bất kỳ người chơi nào chọn mà chọn SAI -> Đổi sang màu ĐỎ
            else if (i == p1Choice || i == p2Choice)
            {
                answerTexts[i].color = Color.red;
            }
            // Những ô sai còn lại không ai dậm vào thì giữ nguyên màu trắng (hoặc bạn có thể đổi tùy ý)
        }

        questionText.text = "Đáp án đúng là: " + correctAnswer;
    }
}