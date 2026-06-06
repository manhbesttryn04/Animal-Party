using UnityEngine;
using System.Collections.Generic;

public class PummelLaserHub : MonoBehaviour
{
    [Header("Cài đặt Tốc độ & Đổi hướng")]
    public float currentSpeed = 60f;
    public float minSpeed = 40f;
    public float maxSpeed = 150f;
    public float minChangeTime = 3f;
    public float maxChangeTime = 6f;

    [Header("Cơ chế 1: Tăng tốc theo thời gian")]
    public bool useProgression = true;
    public float speedIncreasePerSecond = 1.5f; 
    public float absoluteMaxSpeed = 300f;       

    [Header("Cơ chế 2: Nhấp nháy mượt mà (Flicker)")]
    public bool useFlicker = true;
    public float visibleDuration = 4f;         
    public float invisibleDuration = 2f;       

    [Header("Danh sách các tia Laser con")]
    public List<AutoFitLaser> laserBeams = new List<AutoFitLaser>(); // Đã đổi sang kiểu AutoFitLaser

    private float direction = 1f;
    private float changeTimer = 0f;
    private float timeUntilNextChange = 0f;

    private float flickerTimer = 0f;
    private bool isLaserActive = true;

    void Start()
    {
        SetNextChangeTime();
        flickerTimer = visibleDuration;
    }

    void Update()
    {
        // 1. TĂNG TỐC THEO THỜI GIAN
        if (useProgression)
        {
            currentSpeed += speedIncreasePerSecond * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, absoluteMaxSpeed);
        }

        transform.Rotate(Vector3.up * currentSpeed * direction * Time.deltaTime);

        // 2. XOAY NGẪU NHIÊN VÀ ĐẢO CHIỀU
        changeTimer += Time.deltaTime;
        if (changeTimer >= timeUntilNextChange)
        {
            RandomizeMovement();
            SetNextChangeTime();
            changeTimer = 0f;
        }

        // 3. CƠ CHẾ ẨN HIỆN MƯỢT MÀ
        if (useFlicker && laserBeams.Count > 0)
        {
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0f)
            {
                isLaserActive = !isLaserActive;

                // Gọi hàm làm mượt thay vì SetActive
                foreach (AutoFitLaser laser in laserBeams)
                {
                    if (laser != null) laser.SetLaserActive(isLaserActive);
                }

                flickerTimer = isLaserActive ? visibleDuration : invisibleDuration;
            }
        }
    }

    void RandomizeMovement()
    {
        if (Random.value > 0.6f) 
        {
            direction *= -1f;
        }
        currentSpeed = Mathf.Clamp(currentSpeed + Random.Range(-15f, 15f), minSpeed, absoluteMaxSpeed);
    }

    void SetNextChangeTime()
    {
        timeUntilNextChange = Random.Range(minChangeTime, maxChangeTime);
    }
}