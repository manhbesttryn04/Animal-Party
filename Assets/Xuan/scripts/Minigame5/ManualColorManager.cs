using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ManualColorManager : MonoBehaviour
{
    [Header("Danh sách ô màu (Kéo thả đủ 40 ô vào đây)")]
    public List<ColorPad> allPads = new List<ColorPad>();

    [Header("UI Giao diện")]
    public Image targetColorImage;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;

    // Danh sách các nhóm màu để phân phối theo vòng chơi
    private List<Color> easyColors = new List<Color>();
    private List<Color> mediumColors = new List<Color>();
    private List<Color> hardColors = new List<Color>();

    private List<Color> activeColorPool = new List<Color>(); // Bảng màu sẽ dùng cho lượt hiện tại
    private Color targetColor;
    private int currentRound = 1;

    void Awake()
    {
        InitializeColorPools();
    }

    void Start()
    {
        if (allPads.Count == 0)
        {
            Debug.LogError("Bạn chưa kéo các ô vào danh sách allPads!");
            return;
        }
        StartCoroutine(ColorGameLoop());
    }

    // Khởi tạo các dải màu gây lú
    void InitializeColorPools()
    {
        // 1. Nhóm màu CƠ BẢN (6 màu - Dễ nhìn phân biệt)
        easyColors.Add(Color.red);          // Đỏ thuần
        easyColors.Add(Color.blue);         // Xanh dương
        easyColors.Add(Color.yellow);       // Vàng tươi
        easyColors.Add(Color.green);        // Xanh lá
        easyColors.Add(Color.magenta);      // Hồng cánh sen
        easyColors.Add(Color.cyan);         // Xanh ngọc/Xanh thẫm

        // 2. Nhóm màu TRUNG BÌNH (Thêm 5 màu biến thể nhạt/đậm)
        mediumColors.AddRange(easyColors);
        mediumColors.Add(new Color(1f, 0.5f, 0f));       // Cam thuần
        mediumColors.Add(new Color(0.5f, 0f, 0.5f));     // Tím đậm (Purple)
        mediumColors.Add(new Color(0f, 0.5f, 0f));       // Xanh lá cây đậm (Dark Green)
        mediumColors.Add(new Color(0.6f, 0.8f, 1f));     // Xanh da trời nhạt (Light Blue)
        mediumColors.Add(new Color(1f, 0.75f, 0.8f));    // Hồng phấn nhạt (Light Pink)

        // 3. Nhóm màu KHÓ (Thêm 6 màu Neon, Pastel và màu độc lạ để gây nhầm lẫn cực đại)
        hardColors.AddRange(mediumColors);
        hardColors.Add(new Color(0.75f, 1f, 0f));        // Xanh lá chuối/Xanh Neon (Lime) -> Dễ lộn với Vàng/Xanh lá
        hardColors.Add(new Color(0f, 1f, 0.5f));         // Xanh lục bảo (Mint/Spring Green) -> Dễ lộn với Cyan/Green
        hardColors.Add(new Color(1f, 0.3f, 0.5f));       // Hồng dâu đậm (Hot Pink) -> Dễ lộn với Magenta/Đỏ
        hardColors.Add(new Color(0.5f, 0.25f, 0f));      // Màu Nâu đất (Brown)
        hardColors.Add(new Color(0.4f, 0.4f, 0.4f));     // Xám đậm (Dark Gray)
        hardColors.Add(new Color(0.85f, 0.85f, 0.85f));  // Xám khói nhạt (Light Gray) -> Đi chung với trắng/xám đậm rất khó nhìn
    }

    IEnumerator ColorGameLoop()
    {
        while (true)
        {
            if (roundText != null)
            {
                roundText.text = "Lượt: " + currentRound;
            }

            // ---- BƯỚC 1: LỰA CHỌN BẢNG MÀU VÀ ĐỘ KHÓ THEO VÒNG ----
            int safePadsCount = 8;
            float maxTimeForChoice = 5f;

            if (currentRound >= 1 && currentRound <= 5)
            {
                // LƯỢT DỄ: Màu cơ bản rõ ràng, thời gian 5s, nhiều ô đúng
                activeColorPool = easyColors;
                safePadsCount = Random.Range(8, 12);
                maxTimeForChoice = 5f;
            }
            else if (currentRound >= 6 && currentRound <= 10)
            {
                // LƯỢT TRUNG BÌNH: Thêm cam, tím, hồng nhạt, thời gian 4s, ô đúng ít lại
                activeColorPool = mediumColors;
                safePadsCount = Random.Range(4, 7);
                maxTimeForChoice = 4f;
            }
            else
            {
                // LƯỢT KHÓ CỰC HẠN: Kích hoạt toàn bộ 17 màu gây lú, thời gian chớp nhoáng 2s, chỉ 1-2 ô đúng
                activeColorPool = hardColors;
                safePadsCount = Random.Range(1, 3);
                maxTimeForChoice = 3f;
            }

            // ---- BƯỚC 2: CHỌN MÀU MỤC TIÊU TỪ BẢNG MÀU ĐÃ LỌC ----
            targetColor = activeColorPool[Random.Range(0, activeColorPool.Count)];
            targetColorImage.color = targetColor;

            // ---- BƯỚC 3: TRỘN NGẪU NHIÊN DANH SÁCH 40 Ô ----
            List<ColorPad> shuffledPads = new List<ColorPad>(allPads);
            for (int i = 0; i < shuffledPads.Count; i++)
            {
                ColorPad temp = shuffledPads[i];
                int randomIndex = Random.Range(i, shuffledPads.Count);
                shuffledPads[i] = shuffledPads[randomIndex];
                shuffledPads[randomIndex] = temp;
            }

            // Phân bổ màu lên sàn đấu
            for (int i = 0; i < shuffledPads.Count; i++)
            {
                if (i < safePadsCount)
                {
                    shuffledPads[i].SetPadColor(targetColor);
                    shuffledPads[i].isSafe = true;
                }
                else
                {
                    // Lấy màu sai ngẫu nhiên trong danh sách màu đang kích hoạt (phải khác màu đúng)
                    Color randomColor;
                    do
                    {
                        randomColor = activeColorPool[Random.Range(0, activeColorPool.Count)];
                    } while (randomColor == targetColor);

                    shuffledPads[i].SetPadColor(randomColor);
                    shuffledPads[i].isSafe = false;
                }
            }

            // ---- BƯỚC 4: ĐẾM NGƯỢC THỜI GIAN CHƠI ----
            float timeLeft = maxTimeForChoice;
            while (timeLeft > 0)
            {
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }

            // ---- BƯỚC 5: HẾT GIỜ -> SẬP CÁC Ô SAI ----
            timerText.text = "??";
            foreach (ColorPad pad in allPads) pad.CheckSurvival();

            // ---- BƯỚC 6: ĐỢI 3 GIÂY ĐỂ PHỤC HỒI LẠI SÀN ĐẤU ----
            yield return new WaitForSeconds(1f);
            foreach (ColorPad pad in allPads) pad.ResetPad();

            currentRound++;
            yield return new WaitForSeconds(1f);
        }
    }
}