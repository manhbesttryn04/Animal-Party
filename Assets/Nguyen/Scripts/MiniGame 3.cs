using AnimalParty.Audio;
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
    public float totalGameTime = 120f;
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

    private float timeBetweenWaves;
    private int lasersPerWave;
    private float delayBetweenLasers;
    private float currentLaserSpeed;
    private SpawnMode currentMode;

    private float survivalTime;
    private float waveTimer;
    private int spawnCounter;
    private int lastSpawnIndex;
    private int currentPhase;
    private bool isGameOver;
    private bool isSpawningWave;

    private void Start()
    {
        ResetMiniGameState();

        if (centralHub != null)
            centralHub.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        StopAllCoroutines();
        ClearAllLasers();
        ResetMiniGameState();

        isRunning = true;
        isGameOver = false;

        if (centralHub != null)
        {
            centralHub.gameObject.SetActive(true);
            centralHub.ResetHub();
        }

        UpdateDifficultyPhase();

        Debug.Log("MiniGame3 START");
    }

    public void StopMiniGame()
    {
        if (!isRunning && isGameOver) return;

        isRunning = false;
        isGameOver = true;

        StopAllCoroutines();
        ClearAllLasers();

        if (centralHub != null && centralHub.gameObject.activeInHierarchy)
            centralHub.EndMinigameAndSink();

        Debug.Log("MiniGame3 STOP");
    }

    private void Update()
    {
        if (!isRunning || isGameOver) return;

        survivalTime += Time.deltaTime;

        if (survivalTime >= totalGameTime)
        {
            CompleteMiniGame();
            return;
        }

        UpdateDifficultyPhase();

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f && !isSpawningWave)
        {
            StartCoroutine(SpawnSpamWave());
            waveTimer = timeBetweenWaves;
        }
    }

    private void ResetMiniGameState()
    {
        survivalTime = 0f;
        waveTimer = 3f;

        currentPhase = 0;
        spawnCounter = 0;
        lastSpawnIndex = 0;
        isSpawningWave = false;

        timeBetweenWaves = 5f;
        lasersPerWave = 0;
        delayBetweenLasers = 1.2f;
        currentLaserSpeed = 4f;
        currentMode = SpawnMode.Alternating;

        isRunning = false;
        isGameOver = false;
    }

    private void CompleteMiniGame()
    {
        isRunning = false;
        isGameOver = true;

        StopAllCoroutines();
        ClearAllLasers();

        if (centralHub != null && centralHub.gameObject.activeInHierarchy)
            centralHub.EndMinigameAndSink();

      //  Debug.Log("MiniGame3 COMPLETE");
    }

    private void UpdateDifficultyPhase()
    {
        if (survivalTime < timeToPhase2 && currentPhase != 1)
        {
            currentPhase = 1;

            lasersPerWave = 0;
            waveTimer = timeBetweenWaves;

           // Debug.Log("Phase 1: Central Hub");
        }
        else if (survivalTime >= timeToPhase2 && survivalTime < timeToPhase3 && currentPhase != 2)
        {
            currentPhase = 2;

            if (centralHub != null && centralHub.gameObject.activeInHierarchy)
                centralHub.EndMinigameAndSink();

            currentMode = SpawnMode.Alternating;
            currentLaserSpeed = p2_LaserSpeed;
            lasersPerWave = p2_LasersPerWave;
            timeBetweenWaves = p2_WaveDelay;
            delayBetweenLasers = 1.2f;
            waveTimer = timeBetweenWaves;

            ///Debug.Log("Phase 2");
        }
        else if (survivalTime >= timeToPhase3 && survivalTime < timeToPhase4 && currentPhase != 3)
        {
            currentPhase = 3;

            currentMode = SpawnMode.Random;
            currentLaserSpeed = p3_LaserSpeed;
            lasersPerWave = p3_LasersPerWave;
            timeBetweenWaves = p3_WaveDelay;
            delayBetweenLasers = 1.0f;
            waveTimer = timeBetweenWaves;

          //  Debug.Log("Phase 3");
        }
        else if (survivalTime >= timeToPhase4 && currentPhase != 4)
        {
            currentPhase = 4;

            currentMode = SpawnMode.AutoMixed;
            currentLaserSpeed = p4_LaserSpeed;
            lasersPerWave = p4_LasersPerWave;
            timeBetweenWaves = p4_WaveDelay;
            delayBetweenLasers = 0.8f;
            waveTimer = timeBetweenWaves;

            //Debug.Log("Phase 4");
        }
    }

    private IEnumerator SpawnSpamWave()
    {
        isSpawningWave = true;

        if (spawnPoints.Count == 0 || spamLaserPrefab == null || lasersPerWave <= 0)
        {
            isSpawningWave = false;
            yield break;
        }

        SpawnMode waveMode =
            currentMode == SpawnMode.AutoMixed
                ? (SpawnMode)Random.Range(0, 6)
                : currentMode;

        spawnCounter = 0;

        for (int i = 0; i < lasersPerWave; i++)
        {
            if (isGameOver)
            {
                isSpawningWave = false;
                yield break;
            }

            if (waveMode == SpawnMode.BothAtSameTime)
            {
                foreach (Transform sp in spawnPoints)
                    SpawnSingleLaser(sp);
            }
            else
            {
                Transform selectedPoint = GetSpawnPoint(waveMode);

                if (selectedPoint != null)
                    SpawnSingleLaser(selectedPoint);
            }

            float actualDelay =
                waveMode == SpawnMode.Burst
                    ? delayBetweenLasers * 0.5f
                    : delayBetweenLasers;

            yield return new WaitForSeconds(actualDelay);
        }

        if (waveMode == SpawnMode.Burst)
            lastSpawnIndex = (lastSpawnIndex + 1) % spawnPoints.Count;

        isSpawningWave = false;
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

                if (mode == SpawnMode.Alternating)
                    lastSpawnIndex = (lastSpawnIndex + 1) % spawnPoints.Count;

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

            if (sp.name.Contains("Left"))
                finalRotation = Quaternion.LookRotation(camRight);
            else if (sp.name.Contains("Right"))
                finalRotation = Quaternion.LookRotation(-camRight);
        }

        GameObject newLaser = Instantiate(spamLaserPrefab, sp.position, finalRotation);

        if (newLaser.TryGetComponent<LaserSpamObject>(out var laserScript))
            laserScript.speed = currentLaserSpeed;

        if (MiniGameAudioManager.Instance != null)
            MiniGameAudioManager.Instance.PlayLaserSound();

        // Debug.Log("Spawn Laser: " + newLaser.name);
    }

    private void ClearAllLasers()
    {
        LaserSpamObject[] remainingLasers =
            FindObjectsByType<LaserSpamObject>(FindObjectsSortMode.None);

        foreach (LaserSpamObject laser in remainingLasers)
            Destroy(laser.gameObject);
    }
}