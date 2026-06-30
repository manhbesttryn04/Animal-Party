using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum PadHazardType { None, Bomb, Freeze }

public class MiniGame5 : MonoBehaviour
{
    [Header("Minigame Manager")]
    public MiniGameManager manager;
    public bool isPlaying = false;

    [Header("Danh sách các ô")]
    public List<GameObject> allPadRenderers = new List<GameObject>();
    private List<PaintPadData> allPads = new List<PaintPadData>();

    [Header("Material")]
    public Material defaultMaterial;
    public Material player1Material;
    public Material player2Material;
    public Material bombMaterial;
    public Material freezeMaterial;

    [Header("Bomb Prefab GIẢ")]
    public GameObject bombPrefab;
    public float bombSpawnHeight = 0.6f;

    [Header("Bomb Flash")]
    public Color bombFlashColor = new Color(1f, 0.5f, 0f);
    public float bombFlashSpeed = 8f;
    public float hazardResetInterval = 5f;

    [Header("Grow Item")]
    public GameObject growItemPrefab;
    public float growDuration = 5f;
    public float itemExistDuration = 5f;
    public float growMultiplier = 3f;
    public float itemSpawnHeight = 0.5f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float gameDuration = 57f;

    private bool isPlayer1Frozen = false;
    private bool isPlayer2Frozen = false;

    private Vector3 player1OriginalScale = Vector3.one;
    private Vector3 player2OriginalScale = Vector3.one;

    private GameObject currentSpawnedItem = null;
    private Coroutine itemDestroyCoroutine = null;

    void Awake()
    {
        InitializeManualPads();
    }

    void Update()
    {
        if (!isPlaying) return;

        foreach (PaintPadData pad in allPads)
        {
            pad.UpdateDetection();

            if (pad.hazardType == PadHazardType.Bomb)
            {
                pad.UpdateBombFlashing(Color.white, bombFlashColor, bombFlashSpeed);
            }
        }
    }

    void InitializeManualPads()
    {
        allPads.Clear();

        foreach (GameObject padObj in allPadRenderers)
        {
            if (padObj == null) continue;

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

    public void StartMiniGame()
    {
        AudioManager.Instance.PlayEnvironment(AudioManager.Instance.snowFallClip);

        if (isPlaying) return;

        if (resultText != null) resultText.gameObject.SetActive(true);

        isPlaying = true;
        isPlayer1Frozen = false;
        isPlayer2Frozen = false;
        resultText.text = "";
        // if (resultText != null) resultText.text = "Minigame Start";

        foreach (PaintPadData pad in allPads)
        {
            pad.ResetColor();
        }

        ClearCurrentSpawnedItem();

        StartCoroutine(PaintGameRoutine());
        StartCoroutine(SpawnHazardsRoutine());
        StartCoroutine(SpawnGrowItemPrefabRoutine());
    }

    public void StopMiniGame()
    {
        AudioManager.Instance.StopEnvironment();

        isPlaying = false;
        StopAllCoroutines();

        ClearCurrentSpawnedItem();

        foreach (PaintPadData pad in allPads)
        {
            pad.RemoveSpawnedBomb();
        }

        if (timerText != null) timerText.text = "-";
        if (resultText != null) resultText.gameObject.SetActive(false);
    }

    IEnumerator PaintGameRoutine()
    {
        float timeLeft = gameDuration;

        while (timeLeft > 0 && isPlaying)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();

            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        isPlaying = false;

       // if (timerText != null) timerText.text = "End Time";

        ClearCurrentSpawnedItem();

        CalculateFinalScore();

        StartCoroutine(ResetPadsAfterResult());
    }
    IEnumerator ResetPadsAfterResult()
    {
        yield return new WaitForSeconds(3.5f);

        ResetAllPadsToDefault();
    }
    void ResetAllPadsToDefault()    
    {
        foreach (PaintPadData pad in allPads)
        {
            pad.ResetColor();
            pad.RemoveSpawnedBomb();
        }

        ClearCurrentSpawnedItem();

        isPlayer1Frozen = false;
        isPlayer2Frozen = false;
       
    }

    IEnumerator SpawnHazardsRoutine()
    {
        while (isPlaying)
        {
            foreach (PaintPadData pad in allPads)
            {
                if (pad.hazardType == PadHazardType.Bomb ||
                    pad.hazardType == PadHazardType.Freeze)
                {
                    pad.hazardType = PadHazardType.None;
                    pad.RestoreVisualAfterHazard();
                    pad.RemoveSpawnedBomb();
                }
            }

            float timeLeft = gameDuration;

            if (timerText != null &&
                float.TryParse(timerText.text, out float parsedTime))
            {
                timeLeft = parsedTime;
            }

            int bombCount =
                timeLeft > 30f ? Random.Range(2, 11) : Random.Range(11, 21);

            int freezeCount = Random.Range(1, 6);

            List<PaintPadData> availablePads = new List<PaintPadData>();

            foreach (PaintPadData pad in allPads)
            {
                if (pad.hazardType == PadHazardType.None)
                    availablePads.Add(pad);
            }

            int totalHazards =
                Mathf.Min(bombCount + freezeCount, availablePads.Count);

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
                ClearCurrentSpawnedItem();

                PaintPadData randomPad =
                    allPads[Random.Range(0, allPads.Count)];

                Vector3 spawnPos =
                    randomPad.padObject.transform.position +
                    Vector3.up * itemSpawnHeight;

                currentSpawnedItem =
                    Instantiate(growItemPrefab, spawnPos, Quaternion.identity);

                GrowItem itemScript =
                    currentSpawnedItem.GetComponent<GrowItem>();

                if (itemScript != null)
                    itemScript.Setup(this);

                itemDestroyCoroutine =
                    StartCoroutine(DestroyItemAfterDelay(
                        currentSpawnedItem,
                        itemExistDuration
                    ));
            }

            yield return new WaitForSeconds(11f);
        }
    }

    IEnumerator DestroyItemAfterDelay(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item != null && item == currentSpawnedItem)
        {
            Destroy(item);
            currentSpawnedItem = null;
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

        if (padData.hazardType == PadHazardType.Freeze)
        {
            TriggerFreezeStatus(isP2, playerObj);
            padData.ResetColor();
            return;
        }

        if (!isP2)
            padData.SetOwner("Player 1", player1Material);
        else
            padData.SetOwner("Player 2", player2Material);
    }

    void TriggerBombExplosion(PaintPadData explodedPad)
    {
        GameObject bombObj = explodedPad.spawnedBomb;

        explodedPad.spawnedBomb = null;
        explodedPad.hazardType = PadHazardType.None;

        if (bombObj != null)
        {
            Bomb bomb = bombObj.GetComponent<Bomb>();

            if (bomb != null)
            {
                bomb.TriggerBomb();
                Destroy(bombObj, bomb.explodeDelay + 1.5f);
            }
            else
            {
                Destroy(bombObj);
            }
        }

        foreach (PaintPadData pad in allPads)
        {
            float distance = Vector3.Distance(
                explodedPad.padObject.transform.position,
                pad.padObject.transform.position
            );

            if (distance <= 2.5f)
            {
                pad.ResetColor();
            }
        }
    }

    void TriggerFreezeStatus(bool isPlayer2, GameObject playerObj)
    {
        if (!isPlayer2)
        {
            if (!isPlayer1Frozen)
                StartCoroutine(FreezePlayerRoutine(1, playerObj));
        }
        else
        {
            if (!isPlayer2Frozen)
                StartCoroutine(FreezePlayerRoutine(2, playerObj));
        }
    }

    IEnumerator FreezePlayerRoutine(int playerNumber, GameObject playerObj)
    {
        if (playerNumber == 1)
            isPlayer1Frozen = true;
        else
            isPlayer2Frozen = true;

        // Tìm IceBlock trong Player (kể cả đang tắt)
        Transform iceBlock = null;

        foreach (Transform t in playerObj.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "IceBlock")
            {
                iceBlock = t;
                break;
            }
        }

        // Lấy PlayerMove và PlayerManager
        PlayerMove move = playerObj.GetComponent<PlayerMove>();
        PlayerAnimator manager = playerObj.GetComponent<PlayerAnimator>();

        // Hiện khối băng
        if (iceBlock != null)
            iceBlock.gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.bebuffRockMagicClip);

        // Đóng băng người chơi
        if (move != null)
            move.isJumpAndMove = false;

        if (manager != null)
            manager.playerAnimator.speed = 0f;

        yield return new WaitForSeconds(2f);

        // Bỏ đóng băng
        if (move != null)
            move.isJumpAndMove = true;

        if (manager != null)
            manager.playerAnimator.speed = 1f;

        // Ẩn khối băng
        if (iceBlock != null)
            iceBlock.gameObject.SetActive(false);

        if (playerNumber == 1)
            isPlayer1Frozen = false;
        else
            isPlayer2Frozen = false;
    }

    public void OnGrowItemPickedUp(bool isPlayer2, GameObject playerObj)
    {
        if (!isPlaying) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buffBigClip);
        if (itemDestroyCoroutine != null)
        {
            StopCoroutine(itemDestroyCoroutine);
            itemDestroyCoroutine = null;
        }

        currentSpawnedItem = null;

        StartCoroutine(GrowPlayerRoutine(isPlayer2, playerObj));
    }

    IEnumerator GrowPlayerRoutine(bool isPlayer2, GameObject playerObj)
    {
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (!isPlayer2)
            player1OriginalScale = playerObj.transform.localScale;
        else
            player2OriginalScale = playerObj.transform.localScale;

        float liftOffset = 1f;

        Collider playerCollider = playerObj.GetComponent<Collider>();
        if (playerCollider != null)
            liftOffset = playerCollider.bounds.size.y;

        Vector3 originalScale =
            !isPlayer2 ? player1OriginalScale : player2OriginalScale;

        Vector3 targetScale = originalScale * growMultiplier;

        playerObj.transform.localScale = targetScale;

        playerObj.transform.position +=
            Vector3.up * liftOffset * (growMultiplier - 1f) * 0.5f;

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

            playerObj.transform.localScale = originalScale;

            if (cc != null) cc.enabled = true;

            if (movementScript != null)
                movementScript.speed = originalSpeed;
        }
    }

    void ClearCurrentSpawnedItem()
    {
        if (itemDestroyCoroutine != null)
        {
            StopCoroutine(itemDestroyCoroutine);
            itemDestroyCoroutine = null;
        }

        if (currentSpawnedItem != null)
        {
            Destroy(currentSpawnedItem);
            currentSpawnedItem = null;
        }
    }

    void CalculateFinalScore()
    {
        int p1Count = 0;
        int p2Count = 0;

        foreach (PaintPadData pad in allPads)
        {
            if (pad.hazardType == PadHazardType.Bomb ||
                pad.hazardType == PadHazardType.Freeze)
            {
                pad.hazardType = PadHazardType.None;
                pad.RestoreVisualAfterHazard();
                pad.RemoveSpawnedBomb();
            }

            if (pad.ownerTag == "Player 1")
                p1Count++;
            else if (pad.ownerTag == "Player 2")
                p2Count++;
        }

        if (resultText != null)
        {
            if (p1Count > p2Count)
            {
                resultText.text = $"P1 win! ({p1Count} vs {p2Count})";
                resultText.color = Color.blue;
            }
            else if (p2Count > p1Count)
            {
                resultText.text = $"P2 win! ({p2Count} vs {p1Count})";
                resultText.color = Color.red;
            }
            else
            {
                resultText.text = $"Draw! ({p1Count} vs {p2Count})";
                resultText.color = Color.yellow;
            }
        }

     //   Debug.Log("Điểm P1: " + p1Count + " | P2: " + p2Count);

        ExitResultAllPlayer(p1Count, p2Count);
    }

    public void ExitResultAllPlayer(int countPadP1, int countPadP2)
    {
        PlayerMiniGame p1 =
            manager.currentPlayer1.GetComponent<PlayerMiniGame>();

        PlayerMiniGame p2 =
            manager.currentPlayer2.GetComponent<PlayerMiniGame>();

        if (p1 == null || p2 == null) return;

        if (countPadP1 > countPadP2)
            p1.UpCoin(1, 100);
        else if (countPadP2 > countPadP1)
            p2.UpCoin(1, 100);
    }
}

public class PaintPadData
{
    public GameObject padObject;
    public MeshRenderer renderer;
    public string ownerTag = "";
    public PadHazardType hazardType = PadHazardType.None;

    public GameObject spawnedBomb;

    private MiniGame5 manager;

    public PaintPadData(
        GameObject obj,
        MeshRenderer meshRenderer,
        MiniGame5 gameManager
    )
    {
        padObject = obj;
        renderer = meshRenderer;
        manager = gameManager;
    }

    public void UpdateDetection()
    {
        Vector3 centerPosition =
            padObject.transform.position + new Vector3(0f, 0.6f, 0f);

        Vector3 checkSize = new Vector3(1.1f, 0.5f, 1.1f);

        Collider[] hitColliders =
            Physics.OverlapBox(
                centerPosition,
                checkSize,
                padObject.transform.rotation
            );

        foreach (Collider col in hitColliders)
        {
            GameObject pObj = col.gameObject;

            if (pObj.GetComponent<PlayerType>() != null)
            {
                Collider playerCollider = pObj.GetComponent<Collider>();
                if (playerCollider == null) continue;

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

    public void UpdateBombFlashing(Color c1, Color c2, float speed)
    {
        if (renderer == null) return;

        float lerpFactor = Mathf.PingPong(Time.time * speed, 1f);
        renderer.material.color = Color.Lerp(c1, c2, lerpFactor);
    }

    public void SetOwner(string tag, Material playerMat)
    {
        if (hazardType != PadHazardType.None) return;

        ownerTag = tag;

        if (renderer != null)
        {
            renderer.material = playerMat;
            renderer.material.color = Color.white;
        }
    }

    public void ApplyHazardVisual()
    {
        if (renderer == null) return;

        renderer.material.color = Color.white;

        if (hazardType == PadHazardType.Bomb)
        {
            renderer.material = manager.bombMaterial;

            SpawnFakeBomb();
        }
        else if (hazardType == PadHazardType.Freeze)
        {
            renderer.material = manager.freezeMaterial;
            RemoveSpawnedBomb();
        }
    }

    void SpawnFakeBomb()
    {
        if (manager.bombPrefab == null) return;
        if (spawnedBomb != null) return;

        Vector3 spawnPos =
            padObject.transform.position +
            Vector3.up * manager.bombSpawnHeight;

        spawnedBomb =
            GameObject.Instantiate(
                manager.bombPrefab,
                spawnPos,
                Quaternion.identity
            );

        Collider[] bombColliders =
            spawnedBomb.GetComponentsInChildren<Collider>();

        foreach (Collider col in bombColliders)
        {
            col.enabled = false;
        }
    }

    public void RestoreVisualAfterHazard()
    {
        if (renderer == null) return;

        renderer.material.color = Color.white;

        if (ownerTag == "Player 1")
            renderer.material = manager.player1Material;
        else if (ownerTag == "Player 2")
            renderer.material = manager.player2Material;
        else
            renderer.material = manager.defaultMaterial;
    }

    public void ResetColor()
    {
        ownerTag = "";
        hazardType = PadHazardType.None;

        RemoveSpawnedBomb();

        if (renderer != null)
        {
            renderer.material = manager.defaultMaterial;
            renderer.material.color = Color.white;
        }
    }

    public void RemoveSpawnedBomb()
    {
        if (spawnedBomb != null)
        {
            GameObject.Destroy(spawnedBomb);
            spawnedBomb = null;
        }
    }
}