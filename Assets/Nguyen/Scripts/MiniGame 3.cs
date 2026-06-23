using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame3 : MonoBehaviour
{
    public enum SpawnMode 
    { 
        Random, Alternating, DoubleAlternating, Burst, BothAtSameTime, AutoMixed 
    }

    [Header("--- Phase & Game Timings ---")]
    [Tooltip("Tổng thời gian màn chơi (giây). Hết giờ này sẽ thắng/hết màn.")]
    public float totalGameTime = 120f; // Mặc định 2 phút
    
    public float timeToPhase2 = 30f;
    public float timeToPhase3 = 60f;
    public float timeToPhase4 = 90f;

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

    void Start()
    {
        if (centralHub != null) centralHub.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        if (isRunning) return;

        isRunning = true;
        isGameOver = false;
        survivalTime = 0f; 
        currentPhase = 0;
        
        UpdateDifficultyPhase(); 
    }

    public void StopMiniGame()
    {
        isRunning = false;
        TriggerGameOver(); 
        StopAllCoroutines(); 
    }

    void Update()
    {
        if (!isRunning || isGameOver) return; 

        survivalTime += Time.deltaTime;

        // --- CƠ CHẾ MỚI: KIỂM TRA HẾT GIỜ ĐỂ KẾT THÚC MÀN CHƠI ---
        if (survivalTime >= totalGameTime)
        {
            CompleteMiniGame(); // Gọi hàm chiến thắng/kết thúc
            return; // Dừng Update ngay lập tức
        }

        UpdateDifficultyPhase();

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            StartCoroutine(SpawnSpamWave());
            waveTimer = timeBetweenWaves;
        }
    }

    // --- HÀM MỚI: KẾT THÚC KHI HẾT GIỜ ---
    private void CompleteMiniGame()
    {
        //Debug.Log("🎉 HẾT GIỜ! NGƯỜI CHƠI ĐÃ SỐNG SÓT THÀNH CÔNG!");
        isRunning = false;
        isGameOver = true;
        
        if (centralHub != null && centralHub.gameObject.activeInHierarchy) 
        {
            centralHub.EndMinigameAndSink();
        }
        
        // Dọn dẹp Lazer rác trên sân
        LaserSpamObject[] remainingLasers = FindObjectsByType<LaserSpamObject>(FindObjectsSortMode.None);
        foreach (LaserSpamObject laser in remainingLasers) Destroy(laser.gameObject);

        // TODO: Chèn code gọi giao diện màn hình Win hoặc chuyển cảnh ở đây!
    }

    private void TriggerGameOver()
    {
        isGameOver = true; 
        
        if (centralHub != null && centralHub.gameObject.activeInHierarchy) 
        {
            centralHub.EndMinigameAndSink();
        }
        
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
            
            if (centralHub != null) centralHub.EndMinigameAndSink();            
            
            currentMode = SpawnMode.Alternating; 
            currentLaserSpeed = p2_LaserSpeed;
            lasersPerWave = p2_LasersPerWave; 
            timeBetweenWaves = p2_WaveDelay;
            delayBetweenLasers = 1.2f;
        }
        else if (survivalTime >= timeToPhase3 && survivalTime < timeToPhase4 && currentPhase != 3)
        {
            currentPhase = 3;
            
            if (centralHub != null && centralHub.gameObject.activeInHierarchy) 
            {
                centralHub.EndMinigameAndSink(); 
            }
            
            currentMode = SpawnMode.Random; 
            currentLaserSpeed = p3_LaserSpeed;
            lasersPerWave = p3_LasersPerWave;
            timeBetweenWaves = p3_WaveDelay;
            delayBetweenLasers = 1.0f; 
        }
        else if (survivalTime >= timeToPhase4 && currentPhase != 4)
        {
            currentPhase = 4;
            
            if (centralHub != null && centralHub.gameObject.activeInHierarchy) 
            {
                centralHub.EndMinigameAndSink(); 
            }
            
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