using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum PadHazardType { None, Bomb, Freeze }

public class MiniGame5 : MonoBehaviour
{
    [Header("Trạng thái quản lý Minigame")]
    public bool isPlaying = false;

    [Header("Danh sách các ô màu (Kéo các ô sàn vào đây)")]
    public List<GameObject> allPadRenderers = new List<GameObject>();

    private List<PaintPadData> allPads = new List<PaintPadData>();

    [Header("CẤU HÌNH MATERIAL CHỨA HÌNH ICON (MỚI)")]
    public Material defaultMaterial;   // Ô trống mặc định
    public Material player1Material;   // Chứa hình của Player 1
    public Material player2Material;   // Chứa hình của Player 2
    public Material bombMaterial;      // Chứa hình quả Bom
    public Material freezeMaterial;    // Chứa hình khối Băng

    [Header("Cấu hình màu nhấp nháy cho Bom")]
    public Color bombFlashColor = new Color(1f, 0.5f, 0f);
    public float bombFlashSpeed = 8f;
    public float hazardResetInterval = 5f;

    [Header("Cấu hình Vật phẩm Phóng To (PREFAB)")]
    public GameObject growItemPrefab;
    public float growDuration = 5f;
    public float growMultiplier = 3f;
    public float itemSpawnHeight = 0.5f;

    [Header("UI Giao diện")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float gameDuration = 57f;

    [Header("Audio")]
    public AudioSource source;

    private bool isPlayer1Frozen = false;
    private bool isPlayer2Frozen = false;

    private Vector3 player1OriginalScale = Vector3.one;
    private Vector3 player2OriginalScale = Vector3.one;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Awake()
    {
        InitializeManualPads();
    }

    void Update()
    {
        if (isPlaying)
        {
            foreach (PaintPadData pad in allPads)
            {
                pad.UpdateDetection();

                if (pad.hazardType == PadHazardType.Bomb)
                {
                    pad.UpdateBombFlashing(Color.white, bombFlashColor, bombFlashSpeed);
                }
            }
        }
    }

    void InitializeManualPads()
    {
        allPads.Clear();
        foreach (GameObject padObj in allPadRenderers)
        {
            if (padObj != null)
            {
                BoxCollider col = padObj.GetComponent<BoxCollider>();
                if (col != null) col.isTrigger = false;

                MeshRenderer renderer = padObj.GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    PaintPadData data = new PaintPadData(padObj, renderer, this);
                    allPads.Add(data);
                    data.ResetColor();
                }
            }
        }
    }

    public void StartMiniGame()
    {
        if (source != null) source.Play();

        if (isPlaying) return;
        if (resultText != null) resultText.gameObject.SetActive(true);
        isPlaying = true;
        isPlayer1Frozen = false;
        isPlayer2Frozen = false;

        if (resultText != null) resultText.text = "Minigame Start";

        foreach (PaintPadData pad in allPads)
        {
            pad.ResetColor();
        }

        ClearAllSpawnedItems();

        StartCoroutine(PaintGameRoutine());
        StartCoroutine(SpawnHazardsRoutine());
        StartCoroutine(SpawnGrowItemPrefabRoutine());
    }

    public void StopMiniGame()
    {
        if (source != null) source.Stop();
        isPlaying = false;
        StopAllCoroutines();
        ClearAllSpawnedItems();
        if (timerText != null) timerText.text = "-";
        if (resultText != null) resultText.gameObject.SetActive(false);
    }

    IEnumerator PaintGameRoutine()
    {
        float timeLeft = gameDuration;

        while (timeLeft > 0 && isPlaying)
        {
            if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        isPlaying = false;
        if (timerText != null) timerText.text = "HẾT GIỜ!";

        CalculateFinalScore();
    }

    IEnumerator SpawnHazardsRoutine()
    {
        while (isPlaying)
        {
            foreach (PaintPadData pad in allPads)
            {
                if (pad.hazardType == PadHazardType.Bomb || pad.hazardType == PadHazardType.Freeze)
                {
                    pad.hazardType = PadHazardType.None;
                    pad.RestoreVisualAfterHazard();
                }
            }

            float timeLeft = gameDuration;
            if (timerText != null && float.TryParse(timerText.text, out float parsedTime))
            {
                timeLeft = parsedTime;
            }

            int bombCount = (timeLeft > 30f) ? Random.Range(2, 11) : Random.Range(11, 21);
            int freezeCount = Random.Range(1, 6);

            List<PaintPadData> availablePads = new List<PaintPadData>();
            foreach (var pad in allPads)
            {
                if (pad.hazardType == PadHazardType.None) availablePads.Add(pad);
            }

            int totalHazards = Mathf.Min(bombCount + freezeCount, availablePads.Count);

            for (int i = 0; i < availablePads.Count; i++)
            {
                PaintPadData temp = availablePads[i];
                int randomIndex = Random.Range(i, availablePads.Count);
                availablePads[i] = availablePads[randomIndex];
                availablePads[randomIndex] = temp;
            }

            int currentIndex = 0;
            for (int i = 0; i < bombCount; i++)
            {
                if (currentIndex >= totalHazards) break;
                availablePads[currentIndex].hazardType = PadHazardType.Bomb;
                availablePads[currentIndex].ApplyHazardVisual();
                currentIndex++;
            }

            for (int i = 0; i < freezeCount; i++)
            {
                if (currentIndex >= totalHazards) break;
                availablePads[currentIndex].hazardType = PadHazardType.Freeze;
                availablePads[currentIndex].ApplyHazardVisual();
                currentIndex++;
            }

            yield return new WaitForSeconds(hazardResetInterval);
        }
    }

    IEnumerator SpawnGrowItemPrefabRoutine()
    {
        yield return new WaitForSeconds(5f);

        while (isPlaying)
        {
            if (growItemPrefab != null && allPads.Count > 0)
            {
                PaintPadData randomPad = allPads[Random.Range(0, allPads.Count)];
                Vector3 spawnPos = randomPad.padObject.transform.position + new Vector3(0f, itemSpawnHeight, 0f);
                GameObject newItem = Instantiate(growItemPrefab, spawnPos, Quaternion.identity);

                GrowItem itemScript = newItem.GetComponent<GrowItem>();
                if (itemScript != null)
                {
                    itemScript.Setup(this);
                }

                spawnedItems.Add(newItem);
            }

            yield return new WaitForSeconds(10f);
        }
    }

    public void OnPadTriggered(PaintPadData padData, GameObject playerObj)
    {
        if (!isPlaying) return;

        PlayerType pType = playerObj.GetComponent<PlayerType>();
        if (pType == null) return;

        bool isP2 = pType.isPlayer2;

        if (!isP2 && isPlayer1Frozen) return;
        if (isP2 && isPlayer2Frozen) return;

        if (padData.hazardType == PadHazardType.Bomb)
        {
            TriggerBombExplosion(padData);
            return;
        }
        else if (padData.hazardType == PadHazardType.Freeze)
        {
            TriggerFreezeStatus(isP2, playerObj);
            padData.ResetColor();
            return;
        }

        if (!isP2)
        {
            padData.SetOwner("Player 1", player1Material);
        }
        else
        {
            padData.SetOwner("Player 2", player2Material);
        }
    }

    void TriggerFreezeStatus(bool isPlayer2, GameObject playerObj)
    {
        if (!isPlayer2)
        {
            if (!isPlayer1Frozen) StartCoroutine(FreezePlayerRoutine(1, playerObj));
        }
        else
        {
            if (!isPlayer2Frozen) StartCoroutine(FreezePlayerRoutine(2, playerObj));
        }
    }

    IEnumerator FreezePlayerRoutine(int playerNumber, GameObject playerObj)
    {
        if (playerNumber == 1) isPlayer1Frozen = true;
        else isPlayer2Frozen = true;

        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        Vector3 originalVelocity = Vector3.zero;
        if (rb != null)
        {
            originalVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        MonoBehaviour[] scripts = playerObj.GetComponents<MonoBehaviour>();
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && script.GetType() != typeof(PlayerType) &&
               (script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Input")))
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        yield return new WaitForSeconds(2f);

        if (cc != null) cc.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = originalVelocity;
        }

        foreach (var script in disabledScripts)
        {
            if (script != null) script.enabled = true;
        }

        if (playerNumber == 1) isPlayer1Frozen = false;
        else if (playerNumber == 2) isPlayer2Frozen = false;
    }

    public void OnGrowItemPickedUp(bool isPlayer2, GameObject playerObj)
    {
        if (!isPlaying) return;
        StartCoroutine(GrowPlayerRoutine(isPlayer2, playerObj));
    }

    IEnumerator GrowPlayerRoutine(bool isPlayer2, GameObject playerObj)
    {
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (!isPlayer2) player1OriginalScale = playerObj.transform.localScale;
        else player2OriginalScale = playerObj.transform.localScale;

        float liftOffset = 1f;
        Collider playerCollider = playerObj.GetComponent<Collider>();
        if (playerCollider != null)
        {
            liftOffset = playerCollider.bounds.size.y;
        }

        Vector3 targetScale = (!isPlayer2 ? player1OriginalScale : player2OriginalScale) * growMultiplier;
        playerObj.transform.localScale = targetScale;
        playerObj.transform.position += new Vector3(0f, liftOffset * (growMultiplier - 1f) * 0.5f, 0f);

        if (cc != null) cc.enabled = true;

        float originalSpeed = 5f;
        PlayerMove movementScript = playerObj.GetComponent<PlayerMove>();
        if (movementScript != null)
        {
            originalSpeed = movementScript.speed;
            movementScript.speed = originalSpeed * 0.5f;
        }

        yield return new WaitForSeconds(growDuration);

        if (playerObj != null)
        {
            if (cc != null) cc.enabled = false;
            playerObj.transform.localScale = !isPlayer2 ? player1OriginalScale : player2OriginalScale;
            if (cc != null) cc.enabled = true;

            if (movementScript != null)
            {
                movementScript.speed = originalSpeed;
            }
        }
    }

    void TriggerBombExplosion(PaintPadData explodedPad)
    {
        explodedPad.hazardType = PadHazardType.None;
        foreach (PaintPadData pad in allPads)
        {
            float distance = Vector3.Distance(explodedPad.padObject.transform.position, pad.padObject.transform.position);
            if (distance <= 2.5f) pad.ResetColor();
        }
    }

    void ClearAllSpawnedItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedItems.Clear();
    }

    void CalculateFinalScore()
    {
        int p1Count = 0; int p2Count = 0;
        foreach (PaintPadData pad in allPads)
        {
            if (pad.ownerTag == "Player 1") p1Count++;
            else if (pad.ownerTag == "Player 2") p2Count++;
        }
        if (resultText != null)
        {
            // Thiết lập đổi màu chữ kết quả theo màu đặc trưng của Material Player
            if (p1Count > p2Count) { resultText.text = $"P1 win! ({p1Count} vs {p2Count})"; resultText.color = Color.blue; }
            else if (p2Count > p1Count) { resultText.text = $"P2 win! ({p2Count} vs {p1Count})"; resultText.color = Color.red; }
            else { resultText.text = $"Draw! ({p1Count} vs {p2Count})"; resultText.color = Color.white; }
        }
    }
}

public class PaintPadData
{
    public GameObject padObject;
    public MeshRenderer renderer;
    public string ownerTag = "";
    public PadHazardType hazardType = PadHazardType.None;

    private MiniGame5 manager;

    public PaintPadData(GameObject obj, MeshRenderer meshRenderer, MiniGame5 gameManager)
    {
        padObject = obj; renderer = meshRenderer; manager = gameManager;
    }

    public void UpdateDetection()
    {
        Vector3 centerPosition = padObject.transform.position + new Vector3(0f, 0.6f, 0f);
        Vector3 checkSize = new Vector3(1.1f, 0.5f, 1.1f);

        Collider[] hitColliders = Physics.OverlapBox(centerPosition, checkSize, padObject.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            GameObject pObj = col.gameObject;
            if (pObj.GetComponent<PlayerType>() != null)
            {
                Collider playerCollider = pObj.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    Vector3 padCenter = padObject.transform.position;
                    Vector3 closestPoint = playerCollider.ClosestPoint(padCenter);

                    float distanceX = Mathf.Abs(closestPoint.x - padCenter.x);
                    float distanceZ = Mathf.Abs(closestPoint.z - padCenter.z);

                    float targetRadius = 0.42f;

                    if (pObj.transform.localScale.x > 1.5f)
                    {
                        targetRadius = 0.42f * pObj.transform.localScale.x;
                    }

                    if (distanceX < targetRadius && distanceZ < targetRadius)
                    {
                        manager.OnPadTriggered(this, pObj);
                    }
                }
            }
        }
    }

    // Nhấp nháy màu nhuộm (Tint Color) trực tiếp trên Material của quả Bom
    public void UpdateBombFlashing(Color c1, Color c2, float speed)
    {
        if (renderer == null) return;
        float lerpFactor = Mathf.PingPong(Time.time * speed, 1f);
        renderer.material.color = Color.Lerp(c1, c2, lerpFactor);
    }

    // Thay đổi Material sang dạng chứa hình của Player
    public void SetOwner(string tag, Material playerMat)
    {
        if (hazardType != PadHazardType.None) return;
        ownerTag = tag;
        if (renderer != null)
        {
            renderer.material = playerMat;
            renderer.material.color = Color.white; // Đảm bảo giữ nguyên màu sắc gốc của ảnh texture
        }
    }

    // Thay đổi Material sang dạng chứa hình bẫy Bom hoặc Băng
    public void ApplyHazardVisual()
    {
        if (renderer == null) return;
        renderer.material.color = Color.white;

        if (hazardType == PadHazardType.Bomb) renderer.material = manager.bombMaterial;
        else if (hazardType == PadHazardType.Freeze) renderer.material = manager.freezeMaterial;
    }

    public void RestoreVisualAfterHazard()
    {
        if (renderer == null) return;
        renderer.material.color = Color.white;

        if (ownerTag == "Player 1") renderer.material = manager.player1Material;
        else if (ownerTag == "Player 2") renderer.material = manager.player2Material;
        else renderer.material = manager.defaultMaterial;
    }

    public void ResetColor()
    {
        ownerTag = ""; hazardType = PadHazardType.None;
        if (renderer != null)
        {
            renderer.material = manager.defaultMaterial;
            renderer.material.color = Color.white;
        }
    }
}