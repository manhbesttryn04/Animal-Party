using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LaserSpamManager : MonoBehaviour
{
    public enum SpawnMode 
    { 
        Random, Alternating, DoubleAlternating, Burst, BothAtSameTime, AutoMixed 
    }

    [Header("UI Settings")]
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI countdownText; 
    public float totalGameTime = 300f; 

    [Header("Phase Timings")]
    public float timeToPhase2 = 15f;
    public float timeToPhase3 = 30f;
    public float timeToPhase4 = 45f;

    [Header("Phase 2 Difficulty")]
    public float p2_LaserSpeed = 4f;       
    public int p2_LasersPerWave = 3;       
    public float p2_WaveDelay = 4f;        

    [Header("Phase 3 Difficulty")]
    public float p3_LaserSpeed = 6f;       
    public int p3_LasersPerWave = 4;
    public float p3_WaveDelay = 3.5f;

    [Header("Phase 4 Difficulty")]
    public float p4_LaserSpeed = 8.5f;     
    public int p4_LasersPerWave = 5;
    public float p4_WaveDelay = 3f;

    [Header("References")]
    public PummelLaserHub centralHub; 
    public GameObject spamLaserPrefab;
    public List<Transform> spawnPoints = new List<Transform>(); 
    
    // ==========================================
    // BIẾN QUẢN LÝ THEO CHUẨN CỦA TEAM DỰ ÁN
    // ==========================================
    [Header("Team Integration")]
    public bool isRunning = false; 
    
    private float timeBetweenWaves = 5f;
    private int lasersPerWave = 0; 
    private float delayBetweenLasers = 1.2f;
    private float currentLaserSpeed = 4f;
    private SpawnMode currentMode = SpawnMode.Alternating;

    private float survivalTime = 0f;
    private float waveTimer = 3f;
    private int spawnCounter = 0; 
    private int lastSpawnIndex = 0; 
    private int currentPhase = 0; 
    private bool isGameOver = false; 
    private bool isCountdownActive = false; 

    void Start()
    {
        if (centralHub != null) centralHub.gameObject.SetActive(false);
        
        // Đã khôi phục lại lệnh này để game tự động chạy đếm ngược khi test độc lập!
        //StartMiniGame(); 
    }

    public void StartMiniGame()
    {
        // Nếu game đang chạy thì không start nữa
        if (isRunning) return;

        isRunning = true;
        isCountdownActive = true;
        isGameOver = false;
        
        // Chạy đếm ngược 3-2-1 rồi mới vào trận
        StartCoroutine(PlayStartCountdown());
    }

    public void StopMiniGame()
    {
        isRunning = false;
        TriggerGameOver(); // Gọi hàm dọn dẹp laser của chúng ta
        StopAllCoroutines(); 
    }
    // ==========================================

    void Update()
    {
        // Kiểm tra xem game có đang được cho phép chạy không (Chuẩn của team)
        if (!isRunning || isGameOver || isCountdownActive) return; 

        survivalTime += Time.deltaTime;
        float remainingTime = totalGameTime - survivalTime;

        if (remainingTime <= 0f)
        {
            StopMiniGame(); // Hết giờ tự động gọi hàm Stop của team
            return;
        }

        UpdateUI(remainingTime);
        UpdateDifficultyPhase();

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            StartCoroutine(SpawnSpamWave());
            waveTimer = timeBetweenWaves;
        }
    }

    private IEnumerator PlayStartCountdown()
    {
        string[] countdownTokens = { "3", "2", "1", "GO!" };
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        foreach (string token in countdownTokens)
        {
            if (countdownText != null) countdownText.text = token;
            
            float elapsed = 0f;
            float slamDuration = 0.15f; 
            
            while (elapsed < slamDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slamDuration;
                float easeOut = 1f - Mathf.Pow(1f - t, 4); 
                float currentScale = Mathf.Lerp(4f, 1f, easeOut);
                
                if (countdownText != null) countdownText.transform.localScale = new Vector3(currentScale, currentScale, 1f);
                yield return null;
            }
            
            if (countdownText != null) countdownText.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.85f); 
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        isCountdownActive = false; 
        UpdateDifficultyPhase(); 
    }

    private void UpdateUI(float remainingTime)
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    private void TriggerGameOver()
    {
        isGameOver = true; 
        if (timerText != null) timerText.text = "0";
        
        if (centralHub != null) centralHub.gameObject.SetActive(false);
        
        LaserSpamObject[] remainingLasers = FindObjectsByType<LaserSpamObject>(FindObjectsSortMode.None);
        foreach (LaserSpamObject laser in remainingLasers) Destroy(laser.gameObject);
    }

    private void UpdateDifficultyPhase()
    {
        if (survivalTime < timeToPhase2 && currentPhase != 1)
        {
            currentPhase = 1;
            if (centralHub != null) centralHub.gameObject.SetActive(true); 
            lasersPerWave = 0; 
        }
        else if (survivalTime >= timeToPhase2 && survivalTime < timeToPhase3 && currentPhase != 2)
        {
            currentPhase = 2;
            if (centralHub != null) centralHub.gameObject.SetActive(false); 
            
            currentMode = SpawnMode.Alternating; 
            currentLaserSpeed = p2_LaserSpeed;
            lasersPerWave = p2_LasersPerWave; 
            timeBetweenWaves = p2_WaveDelay;
            delayBetweenLasers = 1.2f;
        }
        else if (survivalTime >= timeToPhase3 && survivalTime < timeToPhase4 && currentPhase != 3)
        {
            currentPhase = 3;
            if (centralHub != null) centralHub.gameObject.SetActive(false); 
            
            currentMode = SpawnMode.Random; 
            currentLaserSpeed = p3_LaserSpeed;
            lasersPerWave = p3_LasersPerWave;
            timeBetweenWaves = p3_WaveDelay;
            delayBetweenLasers = 1.0f; 
        }
        else if (survivalTime >= timeToPhase4 && currentPhase != 4)
        {
            currentPhase = 4;
            if (centralHub != null) centralHub.gameObject.SetActive(false); 
            
            currentMode = SpawnMode.AutoMixed; 
            currentLaserSpeed = p4_LaserSpeed;
            lasersPerWave = p4_LasersPerWave;
            timeBetweenWaves = p4_WaveDelay;
            delayBetweenLasers = 0.8f;
        }
    }

    private IEnumerator SpawnSpamWave()
    {
        if (spawnPoints.Count == 0 || spamLaserPrefab == null || lasersPerWave <= 0) yield break;

        SpawnMode waveMode = currentMode == SpawnMode.AutoMixed ? (SpawnMode)Random.Range(0, 6) : currentMode;
        spawnCounter = 0; 

        for (int i = 0; i < lasersPerWave; i++)
        {
            if (isGameOver) yield break; 

            if (waveMode == SpawnMode.BothAtSameTime)
            {
                foreach (Transform sp in spawnPoints) SpawnSingleLaser(sp);
            }
            else
            {
                Transform selectedPoint = GetSpawnPoint(waveMode);
                if (selectedPoint != null) SpawnSingleLaser(selectedPoint);
            }

            float actualDelay = (waveMode == SpawnMode.Burst) ? delayBetweenLasers * 0.5f : delayBetweenLasers;
            yield return new WaitForSeconds(actualDelay);
        }
        
        if (waveMode == SpawnMode.Burst) lastSpawnIndex = (lastSpawnIndex + 1) % spawnPoints.Count;
    }

    private Transform GetSpawnPoint(SpawnMode mode)
    {
        switch (mode)
        {
            case SpawnMode.Random:
                return spawnPoints[Random.Range(0, spawnPoints.Count)];
            
            case SpawnMode.Alternating:
            case SpawnMode.Burst:
                Transform pt = spawnPoints[lastSpawnIndex];
                if (mode == SpawnMode.Alternating) lastSpawnIndex = (lastSpawnIndex + 1) % spawnPoints.Count;
                return pt;
            
            case SpawnMode.DoubleAlternating:
                Transform dPt = spawnPoints[lastSpawnIndex];
                spawnCounter++;
                if (spawnCounter >= 2) 
                {
                    lastSpawnIndex = (lastSpawnIndex + 1) % spawnPoints.Count;
                    spawnCounter = 0;
                }
                return dPt;
                
            default:
                return spawnPoints[0];
        }
    }

    private void SpawnSingleLaser(Transform sp)
    {
        Quaternion finalRotation = sp.rotation; 

        if (Camera.main != null)
        {
            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            if (sp.name.Contains("Left")) finalRotation = Quaternion.LookRotation(camRight);
            else if (sp.name.Contains("Right")) finalRotation = Quaternion.LookRotation(-camRight);
        }

        GameObject newLaser = Instantiate(spamLaserPrefab, sp.position, finalRotation);
        if (newLaser.TryGetComponent<LaserSpamObject>(out var laserScript))
        {
            laserScript.speed = currentLaserSpeed;
        }
    }
}