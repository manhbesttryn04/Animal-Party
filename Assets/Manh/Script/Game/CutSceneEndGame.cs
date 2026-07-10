using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public class CutSceneEndGame : MonoBehaviour
{
    [Header("Teleport")]
    public GameObject teleport;

    [Header("Player")]
    public GameObject player;
    public Transform[] transPlayerToWalk;

    [Header("Camera Cut Scene Points")]
    public Transform[] transCameraCutSceneList;

    [Header("Settings")]
    public float teleportOpenTime = 0.5f;
    public float waitTime = 0.5f;
    public float cameraMoveTime = 1f;

    [Header("Player Move")]
    public float playerMoveSpeed = 1f;
    public float playerSlowMoveSpeed = 0.4f;

    public GameObject blackPanel;

    [Header("--- SUBTITLE NARRATOR ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Outline subtitleOutline;

    [Header("--- GRADIENT PRESET ---")]
    public TMP_ColorGradient normalGradient;
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE COLOR ---")]
    public Color normalOutlineColor = new Color(1f, 1f, 1f, 0.2f);
    public Color finalOutlineColor = new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    public AudioClip[] narratorVoices;
    [Range(0f, 1f)] public float narratorVolume = 0.9f;

    [Header("--- TIMING ---")]
    public float typeSpeed = 0.04f;
    public float fadeOutDuration = 0.3f;
    public float betweenLineFade = 0.3f;

    private string[] storyLines = new string[]
    {
        "A gateway to glory... it finally appears.",           // [0] mở cổng
        "The winner walks into legend.",                       // [1] player bước vào
        "Behold... the island that legends are made of.",      // [2] camera khám phá
        "Riches untold, claimed by only the worthy.",          // [3] camera tiếp tục
        "This is what it was all for.",                        // [4] camera cut 8->11
        "The Animal Party has found its true champion!"        // [5] câu cuối đỏ son
    };

    private AudioSource narratorSource;
    private Vector3 teleportOriginalScale;

    private void Awake()
    {
        if (teleport != null)
        {
            teleportOriginalScale = teleport.transform.localScale;
            teleport.transform.localScale = Vector3.zero;
        }
    }

    private void Start()
    {
        narratorSource = gameObject.AddComponent<AudioSource>();
        narratorSource.playOnAwake = false;
        narratorSource.loop = false;
        narratorSource.volume = narratorVolume;

        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (subtitleText) { subtitleText.text = ""; subtitleText.alpha = 1f; }

        PlayCutScene();
    }

    public void PlayCutScene()
    {
        StartCoroutine(CutSceneRoutine());
    }

    private IEnumerator CutSceneRoutine()
    {
        if (player == null || teleport == null) yield break;

        AudioManager.Instance.PlayEnvironment(AudioManager.Instance.javaLoopClip);

        // 1. Mở cổng + câu 0
        ShowSubtitleImmediate(storyLines[0], 0, false);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openTeleportClip);
        yield return StartCoroutine(ScaleTeleport(teleportOriginalScale, teleportOpenTime));
        yield return new WaitForSeconds(waitTime);

        // 2. Player đi tới Walk 0 + câu 1
        if (transPlayerToWalk.Length > 0 && transPlayerToWalk[0] != null)
        {
            yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[1], 1, false));

            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            if (playerVFX != null)
                StartCoroutine(playerVFX.DissolveInNoParticleRoutine());

            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(player, transPlayerToWalk[0].position, playerMoveSpeed));

            yield return new WaitForSeconds(3f);

            AudioManager.Instance.PlaySFX(AudioManager.Instance.closeTeleportClip);
            yield return StartCoroutine(ScaleTeleport(Vector3.zero, 1));
            yield return playerMoveRoutine;
        }

        // 3. Camera 0->3 + câu 2
        SetCameraToPoint(0);
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[2], 2, false));
        yield return StartCoroutine(MoveCameraToPoint(1));
        yield return StartCoroutine(MoveCameraToPoint(2));
        yield return StartCoroutine(MoveCameraToPoint(3));

        // Camera 4->7 + câu 3
        SetCameraToPoint(4);
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[3], 3, false));
        yield return StartCoroutine(MoveCameraToPoint(5));
        yield return new WaitForSeconds(waitTime);
        SetCameraToPoint(6);
        yield return StartCoroutine(MoveCameraToPoint(7));

        // Camera 8->11 + câu 4
        SetCameraToPoint(8);
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[4], 4, false));
        yield return StartCoroutine(MoveCameraToPoint(9));
        SetCameraToPoint(10);
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(MoveCameraToPoint(11));
        yield return new WaitForSeconds(waitTime);
        SetCameraToPoint(12);

        // 4. Player đi chậm + câu cuối đỏ son
        if (transPlayerToWalk.Length > 1 && transPlayerToWalk[1] != null)
        {
            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(player, transPlayerToWalk[1].position, playerSlowMoveSpeed));

            yield return StartCoroutine(MoveCameraToPoint(13));
            SetCameraToPoint(14);

            yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[5], 5, true));

            yield return StartCoroutine(MoveCameraToPoint(15));
            yield return new WaitForSeconds(0.2f);
            SetCameraToPoint(16);
            yield return playerMoveRoutine;
        }
        else
        {
            yield return StartCoroutine(MoveCameraToPoint(13));
            SetCameraToPoint(14);
            yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[5], 5, true));
            yield return StartCoroutine(MoveCameraToPoint(15));
        }

        HideSubtitleImmediate();
    }

    // ====== SUBTITLE ======
    IEnumerator ShowSubtitleWithVoice(string line, int voiceIndex, bool isFinal)
    {
        if (narratorSource) narratorSource.Stop();

        if (subtitleText && subtitleText.text != "")
        {
            yield return StartCoroutine(FadeOutLine());
            yield return new WaitForSeconds(betweenLineFade);
        }

        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (subtitleText) subtitleText.alpha = 1f;

        ApplyGradient(isFinal);
        PlayVoice(voiceIndex);

        if (subtitleText) subtitleText.text = "";
        foreach (char c in line)
        {
            if (subtitleText) subtitleText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        if (narratorSource && narratorSource.isPlaying)
            yield return new WaitWhile(() => narratorSource.isPlaying);
    }

    void ShowSubtitleImmediate(string line, int voiceIndex, bool isFinal)
    {
        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (subtitleText) { subtitleText.alpha = 1f; subtitleText.text = line; }
        ApplyGradient(isFinal);
        PlayVoice(voiceIndex);
    }

    void ApplyGradient(bool isFinal)
    {
        if (subtitleText)
        {
            subtitleText.enableVertexGradient = true;
            subtitleText.colorGradientPreset = isFinal ? finalGradient : normalGradient;
        }
        if (subtitleOutline)
            subtitleOutline.effectColor = isFinal ? finalOutlineColor : normalOutlineColor;
    }

    void PlayVoice(int index)
    {
        if (narratorSource && narratorVoices != null &&
            narratorVoices.Length > index && narratorVoices[index] != null)
            narratorSource.PlayOneShot(narratorVoices[index], narratorVolume);
    }

    void HideSubtitleImmediate()
    {
        if (narratorSource) narratorSource.Stop();
        if (subtitleText) { subtitleText.alpha = 1f; subtitleText.text = ""; }
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }

    IEnumerator FadeOutLine()
    {
        if (subtitleText == null) yield break;
        float startAlpha = subtitleText.alpha;
        float time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            if (subtitleText) subtitleText.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            yield return null;
        }
        if (subtitleText) { subtitleText.alpha = 1f; subtitleText.text = ""; }
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }

    // ====== TELEPORT ======
    private IEnumerator ScaleTeleport(Vector3 targetScale, float duration)
    {
        Vector3 startScale = teleport.transform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            teleport.transform.localScale = Vector3.Lerp(startScale, targetScale, t / duration);
            yield return null;
        }
        teleport.transform.localScale = targetScale;
    }

    // ====== PLAYER MOVE ======
    private IEnumerator MovePlayerToPoint(GameObject playerObj, Vector3 targetPos, float speed)
    {
        if (playerObj == null) yield break;

        PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();
        NavMeshAgent agent = playerObj.GetComponent<NavMeshAgent>();

        targetPos.y = playerObj.transform.position.y;
        SetPlayerWalk(playerManager, 1f);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(targetPos);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                SetPlayerWalk(playerManager, agent.velocity.magnitude);
                yield return null;
            }

            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            while (Vector3.Distance(playerObj.transform.position, targetPos) > 0.05f)
            {
                Vector3 dir = targetPos - playerObj.transform.position;
                dir.y = 0f;
                if (dir != Vector3.zero)
                {
                    dir.Normalize();
                    playerObj.transform.position += dir * speed * Time.deltaTime;
                    playerObj.transform.rotation = Quaternion.Slerp(
                        playerObj.transform.rotation,
                        Quaternion.LookRotation(dir),
                        Time.deltaTime * 5f);
                }
                yield return null;
            }
            playerObj.transform.position = new Vector3(targetPos.x, playerObj.transform.position.y, targetPos.z);
        }

        SetPlayerWalk(playerManager, 0f);
    }

    private void SetPlayerWalk(PlayerManager playerManager, float value)
    {
        if (playerManager != null &&
            playerManager.playerAnimator != null &&
            playerManager.playerAnimator.playerAnimator != null)
            playerManager.playerAnimator.playerAnimator.SetFloat("Walk", value);
    }

    // ====== CAMERA ======
    private void SetCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index)) return;
        StartCoroutine(ShowBlackPanelRoutine());
        Camera.main.transform.position = transCameraCutSceneList[index].position;
        Camera.main.transform.rotation = transCameraCutSceneList[index].rotation;
    }

    private IEnumerator MoveCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index)) yield break;

        Transform cam = Camera.main.transform;
        Transform target = transCameraCutSceneList[index];
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;
        float t = 0f;

        while (t < cameraMoveTime)
        {
            t += Time.deltaTime;
            float lerp = t / cameraMoveTime;
            cam.position = Vector3.Lerp(startPos, target.position, lerp);
            cam.rotation = Quaternion.Slerp(startRot, target.rotation, lerp);
            yield return null;
        }

        cam.position = target.position;
        cam.rotation = target.rotation;
    }

    private bool IsValidCameraPoint(int index)
    {
        return transCameraCutSceneList != null &&
               index >= 0 &&
               index < transCameraCutSceneList.Length &&
               transCameraCutSceneList[index] != null &&
               Camera.main != null;
    }

    // ====== BLACK PANEL ======
    private IEnumerator ShowBlackPanelRoutine()
    {
        if (blackPanel == null) yield break;
        blackPanel.SetActive(true);
        yield return new WaitForSeconds(1.6f);
        blackPanel.SetActive(false);
    }
}