using UnityEngine;
using TMPro;
using System.Collections;

public class MiniGame9 : MonoBehaviour
{
    [Header("Cấu hình 3 Trụ Xoay")]
    public Transform mainRoller;
    public Transform leftRoller;
    public Transform rightRoller;

    [Header("Cấu hình Tốc Độ Xoay Ngẫu Nhiên")]
    [Tooltip("Tốc độ quay tối thiểu")]
    public float minRotateSpeed = 20f;

    [Tooltip("Tốc độ quay tối đa")]
    public float maxRotateSpeed = 65f;

    [Tooltip("Thời gian tối thiểu để đổi tốc độ mới (giây)")]
    public float minChangeInterval = 4f;

    [Tooltip("Thời gian tối đa để đổi tốc độ mới (giây)")]
    public float maxChangeInterval = 8f;

    [Tooltip("Độ mượt khi chuyển đổi giữa tốc độ cũ và tốc độ mới")]
    public float speedLerpSmoothness = 2f;

    [Header("Cấu hình Lực Ma Sát Đẩy Player")]
    public float surfaceSlipForce = 4f;

    [Header("UI & Trạng Thái")]
    public float gameDuration = 60f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    // Biến lưu tốc độ thực tế (đang biến đổi mượt mà)
    private float currentMainSpeed;
    private float currentLeftSpeed;
    private float currentRightSpeed;

    // Biến lưu tốc độ mục tiêu (được Random ngẫu nhiên)
    private float targetMainSpeed;
    private float targetLeftSpeed;
    private float targetRightSpeed;

    private float timeLeft;
    private bool isPlaying = false;

    private GameObject player1Obj;
    private GameObject player2Obj;
    private CharacterController p1CC;
    private CharacterController p2CC;

    void Start()
    {
        FindPlayers();
        StartMinigame();
    }

    void FindPlayers()
    {
        player1Obj = GameObject.Find("Player Play 1");
        player2Obj = GameObject.Find("Player Play 2");

        if (player1Obj != null) p1CC = player1Obj.GetComponent<CharacterController>();
        if (player2Obj != null) p2CC = player2Obj.GetComponent<CharacterController>();
    }

    public void StartMinigame()
    {
        timeLeft = gameDuration;
        isPlaying = true;

        // Khởi tạo tốc độ ban đầu
        currentMainSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        currentLeftSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        currentRightSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        targetMainSpeed = currentMainSpeed;
        targetLeftSpeed = currentLeftSpeed;
        targetRightSpeed = currentRightSpeed;

        if (statusText != null) statusText.text = "CHÚ Ý! TỐC ĐỘ XOAY ĐỔI LIÊN TỤC!";

        StartCoroutine(GameTimerRoutine());
        StartCoroutine(RandomSpeedRoutine()); // Chạy bộ quản lý Random tốc độ
    }

    void Update()
    {
        if (!isPlaying) return;

        // 1. Biến đổi tốc độ hiện tại tiến dần về tốc độ mục tiêu để trụ quay mượt (không bị giật khựng)
        currentMainSpeed = Mathf.Lerp(currentMainSpeed, targetMainSpeed, Time.deltaTime * speedLerpSmoothness);
        currentLeftSpeed = Mathf.Lerp(currentLeftSpeed, targetLeftSpeed, Time.deltaTime * speedLerpSmoothness);
        currentRightSpeed = Mathf.Lerp(currentRightSpeed, targetRightSpeed, Time.deltaTime * speedLerpSmoothness);

        // 2. Xoay Trụ Giữa (MainRoller - Chiều thuận)
        if (mainRoller != null)
        {
            mainRoller.Rotate(Vector3.up * currentMainSpeed * Time.deltaTime, Space.Self);
        }

        // 3. Xoay Trụ Trái (LeftRoller - Ngược chiều với trụ giữa)
        if (leftRoller != null)
        {
            leftRoller.Rotate(Vector3.up * (-currentLeftSpeed) * Time.deltaTime, Space.Self);
        }

        // 4. Xoay Trụ Phải (RightRoller - Ngược chiều với trụ giữa)
        if (rightRoller != null)
        {
            rightRoller.Rotate(Vector3.up * (-currentRightSpeed) * Time.deltaTime, Space.Self);
        }

        // Tác động lực trượt cho Player
        ApplySurfaceDrag();

        // Kiểm tra điều kiện Rớt (Thua)
        CheckPlayerFall();
    }

    // Coroutine đổi tốc độ ngẫu nhiên độc lập cho từng trụ
    IEnumerator RandomSpeedRoutine()
    {
        while (isPlaying)
        {
            // Chọn thời gian chờ ngẫu nhiên trước khi đổi tốc độ lần tiếp theo
            float waitTime = Random.Range(minChangeInterval, maxChangeInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isPlaying) yield break;

            // Random tốc độ mới cho cả 3 trụ
            targetMainSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
            targetLeftSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
            targetRightSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        }
    }

    void ApplySurfaceDrag()
    {
        if (player1Obj != null && p1CC != null && p1CC.isGrounded)
        {
            ApplyDragToPlayer(player1Obj, p1CC);
        }

        if (player2Obj != null && p2CC != null && p2CC.isGrounded)
        {
            ApplyDragToPlayer(player2Obj, p2CC);
        }
    }

    void ApplyDragToPlayer(GameObject player, CharacterController cc)
    {
        Transform currentRoller = GetNearestRoller(player.transform.position);
        if (currentRoller == null) return;

        float currentSpeed = currentMainSpeed;
        float currentDir = 1f;

        if (currentRoller == mainRoller)
        {
            currentDir = 1f;
            currentSpeed = currentMainSpeed;
        }
        else if (currentRoller == leftRoller)
        {
            currentDir = -1f; // Ngược chiều
            currentSpeed = currentLeftSpeed;
        }
        else if (currentRoller == rightRoller)
        {
            currentDir = -1f; // Ngược chiều
            currentSpeed = currentRightSpeed;
        }

        // Lực trượt tỉ lệ theo tốc độ hiện tại của trụ đó
        Vector3 dragDirection = -currentRoller.forward * currentDir;
        float dynamicSlip = surfaceSlipForce * (currentSpeed / minRotateSpeed);

        cc.Move(dragDirection * dynamicSlip * Time.deltaTime);
    }

    Transform GetNearestRoller(Vector3 playerPos)
    {
        Transform nearest = mainRoller;
        float minDistance = float.MaxValue;

        Transform[] rollers = { mainRoller, leftRoller, rightRoller };
        foreach (Transform r in rollers)
        {
            if (r == null) continue;
            float dist = Vector3.Distance(playerPos, r.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = r;
            }
        }
        return nearest;
    }

    void CheckPlayerFall()
    {
        bool p1Fell = player1Obj != null && player1Obj.transform.position.y < -6f;
        bool p2Fell = player2Obj != null && player2Obj.transform.position.y < -6f;

        if (p1Fell && p2Fell)
        {
            EndGame("HÒA NHAU! CẢ HAI ĐỀU RỚT!");
        }
        else if (p1Fell)
        {
            EndGame("PLAYER 2 CHIẾN THẮNG!");
        }
        else if (p2Fell)
        {
            EndGame("PLAYER 1 CHIẾN THẮNG!");
        }
    }

    IEnumerator GameTimerRoutine()
    {
        while (timeLeft > 0 && isPlaying)
        {
            if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString() + "s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        if (timeLeft <= 0 && isPlaying)
        {
            EndGame("HẾT GIỜ! CẢ HAI CÙNG SỐNG SÓT!");
        }
    }

    public void EndGame(string message)
    {
        isPlaying = false;
        StopAllCoroutines();
        if (statusText != null) statusText.text = message;
    }
}