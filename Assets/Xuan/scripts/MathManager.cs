using UnityEngine;
using TMPro;
using System.Collections;

public class MathManager : MonoBehaviour
{
    [Header("UI Màn Hình Chính")]
    public TextMeshProUGUI questionText;    // Hiện phép tính (VD: 1 + 1 = ?)
    public TextMeshProUGUI timerText;       // Chỉ hiện con số thời gian đếm ngược

    [Header("UI Thông Báo Riêng Biệt")]
    public TextMeshProUGUI p1StatusText;    // Hiện trạng thái/kết quả riêng của P1
    public TextMeshProUGUI p2StatusText;    // Hiện trạng thái/kết quả riêng của P2

    [Header("UI Đáp Án Dưới Mặt Đất")]
    public TextMeshProUGUI[] answerTexts;   // Kéo thả 4 TMP nằm trên bề mặt 4 ô chọn vào đây

    private int correctAnswer;
    private int correctPadIndex;

    // Biến lưu trữ đáp án đã khóa (-1 có nghĩa là chưa chọn)
    private int p1Choice = -1;
    private int p2Choice = -1;
    private bool isAnsweringState = true;   // Trạng thái kiểm soát thời gian bấm chọn

    void Start()
    {
        if (questionText == null || timerText == null || p1StatusText == null || p2StatusText == null || answerTexts.Length < 4)
        {
            Debug.LogError("Vui lòng kéo đầy đủ các thành phần UI vào MathManager trong Inspector!");
            return;
        }
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            // ---- BƯỚC 1: KHỞI TẠO LƯỢT CHƠI & PHÉP TOÁN MỚI ----
            GenerateQuestion();
            isAnsweringState = true;
            p1Choice = -1;
            p2Choice = -1;

            // Xóa rỗng nội dung thông báo lượt cũ
            p1StatusText.text = "";
            p2StatusText.text = "";

            // ---- BƯỚC 2: 10 GIÂY ĐẾM NGƯỢC CHO PHÉP CHỌN ĐÁP ÁN ----
            float timeLeft = 3f;
            while (timeLeft > 0)
            {
                // Chỉ hiển thị số thời gian nguyên, không kèm chữ
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();
                yield return new WaitForSeconds(1.0f);
                timeLeft -= 1f;
            }

            // ---- BƯỚC 3: HẾT GIỜ -> KHÓA NHẬN LỆNH & KIỂM TRA ĐÚNG SAI ----
            isAnsweringState = false;
            CheckFinalResults();

            // ---- BƯỚC 4: 10 GIÂY ĐÓNG BĂNG ĐỢI XEM KẾT QUẢ TRƯỚC KHI ĐỔI CÂU ----
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
            // Thiết lập lại màu chữ đáp án về màu trắng mặc định ban đầu
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

    // Hàm nhận tín hiệu xử lý dậm chân được kích hoạt từ AnswerPad
    public void OnPlayerStepOnPad(bool isPlayer2, int padIndex)
    {
        // Nếu không nằm trong 10s thời gian trả lời thì không ghi nhận
        if (!isAnsweringState) return;

        if (!isPlayer2) // Xử lý Player 1
        {
            // KHÓA ĐÁP ÁN: Nếu đã chọn rồi (khác -1) thì không cho đổi ô khác
            if (p1Choice != -1) return;

            p1Choice = padIndex;
            p1StatusText.text = "P1: Đã khóa!";
            Debug.Log("Player 1 đã chốt ô số: " + padIndex);
        }
        else // Xử lý Player 2
        {
            // KHÓA ĐÁP ÁN: Nếu đã chọn rồi thì không cho đổi ô khác
            if (p2Choice != -1) return;

            p2Choice = padIndex;
            p2StatusText.text = "P2: Đã khóa!";
            Debug.Log("Player 2 đã chốt ô số: " + padIndex);
        }
    }

    void CheckFinalResults()
    {
        // 1. Phân tích & Thông báo trạng thái lên TMP riêng của Player 1
        if (p1Choice == -1) p1StatusText.text = "P1: Không trả lời!";
        else if (p1Choice == correctPadIndex) p1StatusText.text = "P1: CHÍNH XÁC!";
        else p1StatusText.text = "P1: SAI RỒI!";

        // 2. Phân tích & Thông báo trạng thái lên TMP riêng của Player 2
        if (p2Choice == -1) p2StatusText.text = "P2: Không trả lời!";
        else if (p2Choice == correctPadIndex) p2StatusText.text = "P2: CHÍNH XÁC!";
        else p2StatusText.text = "P2: SAI RỒI!";

        // 3. Xử lý logic nhuộm màu chữ đáp án dưới mặt đất
        for (int i = 0; i < 4; i++)
        {
            if (i == correctPadIndex)
            {
                answerTexts[i].color = Color.yellow; // Ô đúng chuyển thành màu VÀNG
            }
            else if (i == p1Choice || i == p2Choice)
            {
                answerTexts[i].color = Color.red;    // Ô bị chọn sai chuyển thành màu ĐỎ
            }
        }

        // Hiện kết quả chính xác lên màn hình đề bài chính
        questionText.text = "Đáp án đúng: " + correctAnswer;
    }
}