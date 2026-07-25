using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutSceneShip : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> transVideoList;

    [Header("Camera")]
    public Camera cam;

    [Header("Speed")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 3f;

    [Header("Panels")]
    public GameObject blackPanel;
    public GameObject blackFlastPanel;

    [Header("--- SUBTITLE ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Image subtitlePanelImage;
    public Outline subtitleOutline;

    [Header("--- SKIP UI ---")]
    public GameObject skipHintObject;
    public TMP_Text skipHintText;

    [Header("--- GRADIENT PRESET ---")]
    [Tooltip("NormalGradient: trái #5C2E00, phải #1A0A00")]
    public TMP_ColorGradient normalGradient;

    [Tooltip("FinalGradient: 4 góc đều #7A0000")]
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE ---")]
    public Color normalOutlineColor =
        new Color(1f, 1f, 1f, 0.2f);

    public Color finalOutlineColor =
        new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    [Tooltip("Kéo 5 file voice vào đây theo thứ tự câu 1 → 5")]
    public AudioClip[] narratorVoices = new AudioClip[5];

    [Header("--- TIMING ---")]
    public float typeSpeed = 0.035f;
    public float fadeOutDuration = 0.3f;
    public float betweenLineFade = 0.2f;

    private bool isSkipped;

    private readonly string[] storyLines =
    {
        "Somewhere in the vast ocean, a ship sails toward an unknown island...",
        "On board: five animals, each dreaming of glory and adventure.",
        "The island holds ancient mini-games, forgotten by time.",
        "Only the cleverest and bravest will claim the ultimate prize.",
        "Let the Animal Party begin!"
    };

    private void Start()
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.alpha = 1f;
        }

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        StartCoroutine(CutScene());
        StartCoroutine(ShowSkipHint());
        var audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.PlayMusic(audio.musicCutScene1Clip);
        }
        var setting = SettingManager.Instance;
        if(setting != null)
        {
            setting.canOpenSettingByEsc = true;
        }
    }

    private void Update()
    {
        if (isSkipped)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            SkipCutscene();
        }
    }

    private void SkipCutscene()
    {
        if (isSkipped)
            return;

        isSkipped = true;


        // Dừng toàn bộ coroutine của CutSceneShip.
        StopAllCoroutines();

        var setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.ResetEscSetting();
            setting.canOpenSettingByEsc = false;
        }
        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.ZeroAllAudio();
            audio.PauseAudio();

            if (audio.UISource != null)
                audio.UISource.Stop();
        }

        HideSubtitleImmediate();

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        StartCoroutine(LoadScene("CutScene 2"));
    }

    private IEnumerator ShowSkipHint()
    {
        yield return new WaitForSeconds(1f);

        if (isSkipped)
            yield break;

        if (skipHintObject != null)
            skipHintObject.SetActive(true);

        if (skipHintText == null)
            yield break;

        skipHintText.alpha = 0f;

        float time = 0f;
        const float duration = 0.5f;

        while (time < duration)
        {
            if (isSkipped)
                yield break;

            time += Time.deltaTime;

            skipHintText.alpha = Mathf.Lerp(
                0f,
                0.7f,
                time / duration
            );

            yield return null;
        }

        skipHintText.alpha = 0.7f;
    }

    private IEnumerator CutScene()
    {
        if (!CheckReferences())
            yield break;

        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.PlaySFX(audio.shipVoiceClip);
            audio.PlaySFX(audio.shipMoveClip);
            audio.PlaySFX(audio.seaGullClip);
        }

        // Point 1
        StartCoroutine(ShowSubtitleWithVoice(0, false));

        yield return StartCoroutine(
            MoveAndRotate(transVideoList[0])
        );

        yield return new WaitForSeconds(1f);

        // Point 2
        StartCoroutine(ShowSubtitleWithVoice(1, false));

        yield return StartCoroutine(
            MoveAndRotate(transVideoList[1])
        );

        yield return new WaitForSeconds(3f);

        // Point 3
        StartCoroutine(ShowSubtitleWithVoice(2, false));

        yield return StartCoroutine(
            MoveAndRotate(transVideoList[2])
        );

        yield return new WaitForSeconds(1f);

        // Point 4
        StartCoroutine(ShowSubtitleWithVoice(3, false));

        yield return StartCoroutine(
            MoveAndRotate(transVideoList[3])
        );

        yield return new WaitForSeconds(2f);

        // Point 5: dừng narrator, flash đen và dịch chuyển camera.
        StopNarratorVoice();
        HideSubtitleImmediate();

        if (blackFlastPanel != null)
            blackFlastPanel.SetActive(true);

        cam.transform.rotation =
            Quaternion.Euler(0f, 90f, 0f);

        cam.transform.position =
            transVideoList[4].position;

        yield return null;

        // Point 6
        StartCoroutine(
            MoveAndRotate(transVideoList[5])
        );
        var setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.ResetEscSetting();
            setting.canOpenSettingByEsc = false;
        }

        yield return new WaitForSeconds(1.5f);

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        StartCoroutine(
            ShowSubtitleWithVoice(4, true)
        );

        yield return new WaitForSeconds(3f);
        
        yield return StartCoroutine(
            ShowBlackPanel(5f)
        );
    }

    private IEnumerator ShowSubtitleWithVoice(
        int index,
        bool isFinal)
    {
        if (isSkipped)
            yield break;

        if (index < 0 ||
            index >= storyLines.Length ||
            index >= narratorVoices.Length)
        {
            Debug.LogWarning(
                "Narrator index không hợp lệ: " + index
            );

            yield break;
        }

        // Dừng câu narrator trước đó.
        StopNarratorVoice();

        // Fade câu chữ cũ.
        if (subtitleText != null &&
            !string.IsNullOrEmpty(subtitleText.text))
        {
            yield return StartCoroutine(FadeOutLine());
            yield return new WaitForSeconds(betweenLineFade);
        }

        if (isSkipped)
            yield break;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = "";

            subtitleText.enableVertexGradient = true;
            subtitleText.colorGradientPreset =
                isFinal ? finalGradient : normalGradient;
        }

        if (subtitleOutline != null)
        {
            subtitleOutline.effectColor =
                isFinal
                    ? finalOutlineColor
                    : normalOutlineColor;
        }

        // Phát giọng người dẫn chuyện bằng UISource.
        AudioManager audio = AudioManager.Instance;

        if (audio != null &&
            narratorVoices[index] != null)
        {
            audio.PlayUI(narratorVoices[index]);
        }

        // Hiệu ứng gõ từng chữ.
        string line = storyLines[index];

        foreach (char character in line)
        {
            if (isSkipped)
                yield break;

            if (subtitleText != null)
                subtitleText.text += character;

            yield return new WaitForSeconds(typeSpeed);
        }

        // Chờ giọng người dẫn chuyện phát xong.
        if (audio != null && audio.UISource != null)
        {
            yield return new WaitWhile(
                () =>
                    !isSkipped &&
                    audio != null &&
                    audio.UISource != null &&
                    audio.UISource.isPlaying
            );
        }
    }

    public void HideSubtitle()
    {
        StopNarratorVoice();
        StartCoroutine(FadeOutLine());
    }

    private void StopNarratorVoice()
    {
        AudioManager audio = AudioManager.Instance;

        if (audio != null && audio.UISource != null)
            audio.UISource.Stop();
    }

    private void HideSubtitleImmediate()
    {
        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = "";
        }

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    private IEnumerator FadeOutLine()
    {
        if (subtitleText == null)
            yield break;

        float duration = Mathf.Max(0.01f, fadeOutDuration);
        float startAlpha = subtitleText.alpha;
        float time = 0f;

        while (time < duration)
        {
            if (isSkipped)
                yield break;

            time += Time.deltaTime;

            subtitleText.alpha = Mathf.Lerp(
                startAlpha,
                0f,
                time / duration
            );

            yield return null;
        }

        subtitleText.alpha = 1f;
        subtitleText.text = "";

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    private IEnumerator MoveAndRotate(Transform target)
    {
        if (cam == null || target == null)
            yield break;

        while (
            Vector3.Distance(
                cam.transform.position,
                target.position
            ) > 0.05f ||
            Quaternion.Angle(
                cam.transform.rotation,
                target.rotation
            ) > 0.1f)
        {
            if (isSkipped)
                yield break;

            cam.transform.position =
                Vector3.MoveTowards(
                    cam.transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );

            cam.transform.rotation =
                Quaternion.Slerp(
                    cam.transform.rotation,
                    target.rotation,
                    rotateSpeed * Time.deltaTime
                );

            yield return null;
        }

        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    private IEnumerator ShowBlackPanel(float waitTime)
    {
        if (blackPanel != null)
            blackPanel.SetActive(true);

        HideSubtitleImmediate();
        StopNarratorVoice();

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutAllAudio(5f);
        }

        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(
            LoadScene("CutScene 2")
        );
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }

    private bool CheckReferences()
    {
        if (cam == null)
        {
            Debug.LogError(
                "CutSceneShip chưa được gán Camera."
            );

            return false;
        }

        if (transVideoList == null ||
            transVideoList.Count < 6)
        {
            Debug.LogError(
                "CutSceneShip cần ít nhất 6 waypoint."
            );

            return false;
        }

        for (int i = 0; i < 6; i++)
        {
            if (transVideoList[i] == null)
            {
                Debug.LogError(
                    "Waypoint " + i + " đang bị thiếu."
                );

                return false;
            }
        }

        return true;
    }
}