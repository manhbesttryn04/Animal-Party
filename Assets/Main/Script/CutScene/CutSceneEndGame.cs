using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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

    [Header("Black Panels")]
    public GameObject blackPanel;
    public GameObject blackStopPanel;

    [Header("--- SUBTITLE NARRATOR ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Outline subtitleOutline;

    [Header("--- GRADIENT PRESET ---")]
    public TMP_ColorGradient normalGradient;
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE COLOR ---")]
    public Color normalOutlineColor =
        new Color(1f, 1f, 1f, 0.2f);

    public Color finalOutlineColor =
        new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    public AudioClip[] narratorVoices;

    [Range(0f, 1f)]
    public float narratorVolume = 0.9f;

    [Header("--- SUBTITLE TIMING ---")]
    public float typeSpeed = 0.04f;
    public float fadeOutDuration = 0.3f;
    public float betweenLineFade = 0.3f;

    private readonly string[] storyLines =
    {
        "A gateway to glory... it finally appears.",
        "The winner walks into legend.",
        "Behold... the island that legends are made of.",
        "Riches untold, claimed by only the worthy.",
        "This is what it was all for.",
        "The Animal Party has found its true champion!"
    };

    private Vector3 teleportOriginalScale;
    private AudioSource narratorSource;

    private void Awake()
    {
        FindPlayerWinner();

        if (teleport != null)
        {
            teleportOriginalScale = teleport.transform.localScale;
            teleport.transform.localScale = Vector3.zero;
        }
    }

    private void Start()
    {
        SetupNarrator();
        PlayCutScene();
    }

    //==================================================
    // SETUP NARRATOR
    //==================================================
    private void SetupNarrator()
    {
        narratorSource = gameObject.AddComponent<AudioSource>();

        narratorSource.playOnAwake = false;
        narratorSource.loop = false;
        narratorSource.volume = narratorVolume;

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.alpha = 1f;
        }
    }

    //==================================================
    // PLAY CUTSCENE
    //==================================================
    public void PlayCutScene()
    {
        StartCoroutine(CutSceneRoutine());
    }

    //==================================================
    // MAIN CUTSCENE
    //==================================================
    private IEnumerator CutSceneRoutine()
    {
        if (player == null || teleport == null)
        {
            yield break;
        }

        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.PlayEnvironment(audio.javaLoopClip);
        }

        //==================================================
        // 1. MỞ CỔNG + PHỤ ĐỀ CÂU 0
        //==================================================
        ShowSubtitleImmediate(
            storyLines[0],
            0,
            false
        );

        if (audio != null)
        {
            audio.PlaySFX(audio.openTeleportClip);
        }

        yield return StartCoroutine(
            ScaleTeleport(
                teleportOriginalScale,
                teleportOpenTime
            )
        );

        yield return new WaitForSeconds(2f);

        //==================================================
        // 2. PLAYER ĐI TỚI WALK 0 + PHỤ ĐỀ CÂU 1
        //==================================================
        if (transPlayerToWalk != null &&
            transPlayerToWalk.Length > 0 &&
            transPlayerToWalk[0] != null)
        {
           StartCoroutine(
                ShowSubtitleWithVoice(
                    storyLines[1],
                    1,
                    false
                )
            );

            PlayerVFX playerVFX =
                player.GetComponent<PlayerVFX>();

            if (playerVFX != null)
            {
                StartCoroutine(
                    playerVFX.DissolveInNoParticleRoutine()
                );
            }

            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(
                    player,
                    transPlayerToWalk[0].position,
                    playerMoveSpeed
                )
            );

            yield return new WaitForSeconds(3f);

            if (audio != null)
            {
                audio.PlaySFX(audio.closeTeleportClip);
            }

            yield return StartCoroutine(
                ScaleTeleport(
                    Vector3.zero,
                    1f
                )
            );

            yield return playerMoveRoutine;
        }

        //==================================================
        // 3. CAMERA CUT 0 -> 3 + PHỤ ĐỀ CÂU 2
        //==================================================
        SetCameraToPoint(0);

        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[2],
                2,
                false
            )
        );

        yield return StartCoroutine(
            MoveCameraToPoint(1)
        );

        yield return StartCoroutine(
            MoveCameraToPoint(2)
        );

        yield return StartCoroutine(
            MoveCameraToPoint(3)
        );

        //==================================================
        // CAMERA CUT 4 -> 7 + PHỤ ĐỀ CÂU 3
        //==================================================
        SetCameraToPoint(4);

         StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[3],
                3,
                false
            )
        );

        yield return StartCoroutine(
            MoveCameraToPoint(5)
        );

        yield return new WaitForSeconds(waitTime);

        SetCameraToPoint(6);

        yield return StartCoroutine(
            MoveCameraToPoint(7)
        );

        //==================================================
        // CAMERA CUT 8 -> 12 + PHỤ ĐỀ CÂU 4
        //==================================================
        SetCameraToPoint(8);

       StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[4],
                4,
                false
            )
        );

        yield return StartCoroutine(
            MoveCameraToPoint(9)
        );

        SetCameraToPoint(10);

        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(
            MoveCameraToPoint(11)
        );

        yield return new WaitForSeconds(waitTime);

        SetCameraToPoint(12);

        //==================================================
        // 4. PLAYER ĐI CHẬM + CÂU CUỐI
        //==================================================
        if (transPlayerToWalk != null &&
            transPlayerToWalk.Length > 1 &&
            transPlayerToWalk[1] != null)
        {
            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(
                    player,
                    transPlayerToWalk[1].position,
                    playerSlowMoveSpeed
                )
            );
          
            // Camera 12 -> 13
            yield return StartCoroutine(
                MoveCameraToPoint(13)
            );

            // Dịch chuyển tới camera 14
            SetCameraToPoint(14);

            // Hiện câu cuối
           

            // Camera 14 -> 15
            yield return StartCoroutine(
                MoveCameraToPoint(15)
            );

            yield return new WaitForSeconds(0.2f);

            // Dịch chuyển tới camera 16
            SetCameraToPoint(16);

            // Đợi player đi xong
            yield return playerMoveRoutine;
           yield return StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[5],
                5,
                true
            )
        );

            HideSubtitleImmediate();

            // Hiện panel đen cuối game
            if (blackStopPanel != null)
            {
                blackStopPanel.SetActive(true);
            }

            // Fade toàn bộ âm thanh
            if (audio != null)
            {
                audio.FadeOutAllAudio(4.18f);
            }

            yield return new WaitForSeconds(7f);

            // Chuyển scene
            SceneManager.LoadScene(6);
        }
        else
        {
            yield return StartCoroutine(
                MoveCameraToPoint(13)
            );

            SetCameraToPoint(14);

            yield return StartCoroutine(
                ShowSubtitleWithVoice(
                    storyLines[5],
                    5,
                    true
                )
            );

            yield return StartCoroutine(
                MoveCameraToPoint(15)
            );

            HideSubtitleImmediate();
        }
    }

    //==================================================
    // HIỆN PHỤ ĐỀ + GIỌNG KỂ
    //==================================================
    private IEnumerator ShowSubtitleWithVoice(
        string line,
        int voiceIndex,
        bool isFinal
    )
    {
        if (narratorSource != null)
        {
            narratorSource.Stop();
        }

        // Nếu câu cũ còn hiện thì fade ra
        if (subtitleText != null &&
            subtitleText.text != "")
        {
            yield return StartCoroutine(
                FadeOutLine()
            );

            yield return new WaitForSeconds(
                betweenLineFade
            );
        }

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
        }

        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = "";
        }

        ApplyGradient(isFinal);

        PlayVoice(voiceIndex);

        // Hiệu ứng chữ chạy
        foreach (char character in line)
        {
            if (subtitleText != null)
            {
                subtitleText.text += character;
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        // Đợi giọng kể chạy xong
        if (narratorSource != null &&
            narratorSource.isPlaying)
        {
            yield return new WaitWhile(
                () => narratorSource.isPlaying
            );
        }
    }

    //==================================================
    // HIỆN PHỤ ĐỀ NGAY
    //==================================================
    private void ShowSubtitleImmediate(
        string line,
        int voiceIndex,
        bool isFinal
    )
    {
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
        }

        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = line;
        }

        ApplyGradient(isFinal);

        PlayVoice(voiceIndex);
    }

    //==================================================
    // ÁP DỤNG GRADIENT VÀ OUTLINE
    //==================================================
    private void ApplyGradient(bool isFinal)
    {
        if (subtitleText != null)
        {
            subtitleText.enableVertexGradient = true;

            subtitleText.colorGradientPreset =
                isFinal
                    ? finalGradient
                    : normalGradient;
        }

        if (subtitleOutline != null)
        {
            subtitleOutline.effectColor =
                isFinal
                    ? finalOutlineColor
                    : normalOutlineColor;
        }
    }

    //==================================================
    // PHÁT GIỌNG KỂ
    //==================================================
    private void PlayVoice(int index)
    {
        if (narratorSource == null)
        {
            return;
        }

        if (narratorVoices == null)
        {
            return;
        }

        if (index < 0 ||
            index >= narratorVoices.Length)
        {
            return;
        }

        if (narratorVoices[index] == null)
        {
            return;
        }

        narratorSource.PlayOneShot(
            narratorVoices[index],
            narratorVolume
        );
    }

    //==================================================
    // FADE OUT PHỤ ĐỀ
    //==================================================
    private IEnumerator FadeOutLine()
    {
        if (subtitleText == null)
        {
            yield break;
        }

        float startAlpha = subtitleText.alpha;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;

            subtitleText.alpha = Mathf.Lerp(
                startAlpha,
                0f,
                time / fadeOutDuration
            );

            yield return null;
        }

        subtitleText.alpha = 1f;
        subtitleText.text = "";

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }

    //==================================================
    // ẨN PHỤ ĐỀ NGAY
    //==================================================
    private void HideSubtitleImmediate()
    {
        if (narratorSource != null)
        {
            narratorSource.Stop();
        }

        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = "";
        }

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }

    //==================================================
    // TELEPORT SCALE
    //==================================================
    private IEnumerator ScaleTeleport(
        Vector3 targetScale,
        float duration
    )
    {
        if (teleport == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            teleport.transform.localScale = targetScale;
            yield break;
        }

        Vector3 startScale =
            teleport.transform.localScale;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            teleport.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    time / duration
                );

            yield return null;
        }

        teleport.transform.localScale = targetScale;
    }

    //==================================================
    // PLAYER MOVE
    //==================================================
    private IEnumerator MovePlayerToPoint(
        GameObject playerObj,
        Vector3 targetPos,
        float speed
    )
    {
        if (playerObj == null)
        {
            yield break;
        }

        PlayerManager playerManager =
            playerObj.GetComponent<PlayerManager>();

        NavMeshAgent agent =
            playerObj.GetComponent<NavMeshAgent>();

        // Giữ nguyên Y của player
        targetPos.y =
            playerObj.transform.position.y;

        SetPlayerWalk(playerManager, 1f);

        if (agent != null)
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            agent.isStopped = false;
            agent.speed = speed;

            agent.SetDestination(targetPos);

            while (agent.pathPending ||
                   agent.remainingDistance >
                   agent.stoppingDistance)
            {
                SetPlayerWalk(
                    playerManager,
                    agent.velocity.magnitude
                );

                yield return null;
            }

            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            while (Vector3.Distance(
                       playerObj.transform.position,
                       targetPos
                   ) > 0.05f)
            {
                Vector3 direction =
                    targetPos -
                    playerObj.transform.position;

                direction.y = 0f;

                if (direction != Vector3.zero)
                {
                    direction.Normalize();

                    playerObj.transform.position +=
                        direction *
                        speed *
                        Time.deltaTime;

                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    playerObj.transform.rotation =
                        Quaternion.Slerp(
                            playerObj.transform.rotation,
                            targetRotation,
                            Time.deltaTime * 5f
                        );
                }

                yield return null;
            }

            playerObj.transform.position =
                new Vector3(
                    targetPos.x,
                    playerObj.transform.position.y,
                    targetPos.z
                );
        }

        SetPlayerWalk(playerManager, 0f);
    }

    //==================================================
    // PLAYER WALK ANIMATION
    //==================================================
    private void SetPlayerWalk(
        PlayerManager playerManager,
        float value
    )
    {
        if (playerManager != null &&
            playerManager.playerAnimator != null &&
            playerManager.playerAnimator.playerAnimator != null)
        {
            playerManager.playerAnimator
                .playerAnimator
                .SetFloat(
                    "Walk",
                    value
                );
        }
    }

    //==================================================
    // SET CAMERA NGAY
    //==================================================
    private void SetCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index))
        {
            return;
        }

        StartCoroutine(
            ShowBlackPanelRoutine()
        );

        Camera.main.transform.position =
            transCameraCutSceneList[index].position;

        Camera.main.transform.rotation =
            transCameraCutSceneList[index].rotation;
    }

    //==================================================
    // CAMERA DI CHUYỂN MƯỢT
    //==================================================
    private IEnumerator MoveCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index))
        {
            yield break;
        }

        Transform cameraTransform =
            Camera.main.transform;

        Transform target =
            transCameraCutSceneList[index];

        Vector3 startPosition =
            cameraTransform.position;

        Quaternion startRotation =
            cameraTransform.rotation;

        if (cameraMoveTime <= 0f)
        {
            cameraTransform.position =
                target.position;

            cameraTransform.rotation =
                target.rotation;

            yield break;
        }

        float time = 0f;

        while (time < cameraMoveTime)
        {
            time += Time.deltaTime;

            float percent =
                time / cameraMoveTime;

            cameraTransform.position =
                Vector3.Lerp(
                    startPosition,
                    target.position,
                    percent
                );

            cameraTransform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    target.rotation,
                    percent
                );

            yield return null;
        }

        cameraTransform.position =
            target.position;

        cameraTransform.rotation =
            target.rotation;
    }

    //==================================================
    // KIỂM TRA CAMERA POINT
    //==================================================
    private bool IsValidCameraPoint(int index)
    {
        return transCameraCutSceneList != null &&
               index >= 0 &&
               index < transCameraCutSceneList.Length &&
               transCameraCutSceneList[index] != null &&
               Camera.main != null;
    }

    //==================================================
    // BLACK PANEL CAMERA CUT
    //==================================================
    private IEnumerator ShowBlackPanelRoutine()
    {
        if (blackPanel == null)
        {
            yield break;
        }

        blackPanel.SetActive(true);

        yield return new WaitForSeconds(1.6f);

        blackPanel.SetActive(false);
    }

    //==================================================
    // FIND PLAYER WINNER
    //==================================================
    public void FindPlayerWinner()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
}