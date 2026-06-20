using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CutSceneShip : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> transVideoList;

    [Header("Camera")]
    public Camera cam;

    [Header("Speed")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 3f;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip shipVoiceClip;
    public AudioClip shipMoveClip;
    public List<AudioSource> audioSources;

    [Header("Panels")]
    public GameObject blackPanel;
    public GameObject blackFlastPanel;

    [Header("--- SUBTITLE ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Image subtitlePanelImage;
    public Outline subtitleOutline;

    [Header("--- SKIP UI ---")]
    [Tooltip("GameObject chứa chữ 'Press Space to Skip' góc màn hình")]
    public GameObject skipHintObject;
    [Tooltip("TMP Text của skip hint (để fade in/out)")]
    public TMP_Text skipHintText;

    [Header("--- MÀU SUBTITLE ---")]
    public Color normalTextColor = Color.white;
    public Color normalOutlineColor = new Color(1f, 1f, 1f, 0.2f);
    public Color finalTextColor = new Color(0.96f, 0.90f, 0.66f, 1f);
    public Color finalOutlineColor = new Color(0.79f, 0.64f, 0.15f, 1f);

    [Header("--- TIMING ---")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;
    public float betweenLineFade = 0.3f; // thời gian fade out câu cũ trước khi hiện câu mới

    private Coroutine typingCoroutine;
    private bool isSkipped = false;

    private string[] storyLines = new string[]
    {
        "Somewhere in the vast ocean, a ship sails toward an unknown island...",
        "On board: five animals, each dreaming of glory and adventure.",
        "The island holds ancient mini-games, forgotten by time.",
        "Only the cleverest and bravest will claim the ultimate prize.",
        "Let the Animal Party begin!"
    };

    private void Start()
    {
        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (subtitleText) { subtitleText.text = ""; subtitleText.alpha = 0f; }

        // Hiện skip hint mờ dần sau 1s
        if (skipHintObject) skipHintObject.SetActive(false);

        StartCoroutine(CutScene());
        StartCoroutine(ShowSkipHint());
    }

    private void Update()
    {
        // Bấm Space hoặc Enter để skip
        if (!isSkipped && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            SkipCutscene();
        }
    }

    private void SkipCutscene()
    {
        isSkipped = true;
        StopAllCoroutines();
        HideSubtitleImmediate();
        if (skipHintObject) skipHintObject.SetActive(false);
        StartCoroutine(LoadScene("CutScene 2"));
    }

    IEnumerator ShowSkipHint()
    {
        yield return new WaitForSeconds(1f);
        if (skipHintObject) skipHintObject.SetActive(true);

        // Fade in skip hint
        if (skipHintText)
        {
            skipHintText.alpha = 0f;
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                skipHintText.alpha = Mathf.Lerp(0f, 0.7f, t / 0.5f); // mờ nhạt thôi, không cần nổi bật
                yield return null;
            }
        }
    }

    IEnumerator CutScene()
    {
        source.PlayOneShot(shipVoiceClip);
        source.PlayOneShot(shipMoveClip);

        // Point 1
        yield return StartCoroutine(ShowSubtitleWithFade(storyLines[0], false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[0]));
        yield return new WaitForSeconds(1f);

        // Point 2
        yield return StartCoroutine(ShowSubtitleWithFade(storyLines[1], false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[1]));
        yield return new WaitForSeconds(1f);

        // Point 3
        yield return StartCoroutine(ShowSubtitleWithFade(storyLines[2], false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[2]));
        yield return new WaitForSeconds(1f);

        // Point 4
        yield return StartCoroutine(ShowSubtitleWithFade(storyLines[3], false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[3]));
        yield return new WaitForSeconds(1f);

        // Point 5 — flash đen + teleport (không subtitle)
        HideSubtitle();
        blackFlastPanel.SetActive(true);
        cam.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cam.transform.position = transVideoList[4].position;
        yield return new WaitForSeconds(0.5f);

        // Point 6 — câu cuối vàng
        StartCoroutine(MoveAndRotate(transVideoList[5]));
        yield return new WaitForSeconds(1.5f);

        // Ẩn skip hint trước câu cuối
        if (skipHintObject) skipHintObject.SetActive(false);

        yield return StartCoroutine(ShowSubtitleWithFade(storyLines[4], true));
        yield return new WaitForSeconds(3f);
        StartCoroutine(ShowBlackPanel(4f));
    }

    // Fade out câu cũ → đổi màu → fade in câu mới
    IEnumerator ShowSubtitleWithFade(string line, bool isFinal)
    {
        // Nếu đang có chữ → fade out trước
        if (subtitleText && subtitleText.alpha > 0f)
        {
            yield return StartCoroutine(FadeOutLine());
            yield return new WaitForSeconds(betweenLineFade);
        }

        // Đổi màu theo loại câu
        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (subtitleText) subtitleText.color = isFinal ? finalTextColor : normalTextColor;
        if (subtitleOutline) subtitleOutline.effectColor = isFinal ? finalOutlineColor : normalOutlineColor;

        // Fade in câu mới
        yield return StartCoroutine(FadeInLine(line));
    }

    public void HideSubtitle()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(FadeOutLine());
    }

    private void HideSubtitleImmediate()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (subtitleText) { subtitleText.alpha = 0f; subtitleText.text = ""; }
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }

    IEnumerator FadeInLine(string line)
    {
        if (subtitleText) { subtitleText.text = line; subtitleText.alpha = 0f; }

        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            if (subtitleText) subtitleText.alpha = Mathf.Lerp(0f, 1f, time / fadeInDuration);
            yield return null;
        }
        if (subtitleText) subtitleText.alpha = 1f;
    }

    IEnumerator FadeOutLine()
    {
        float startAlpha = subtitleText != null ? subtitleText.alpha : 1f;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            if (subtitleText) subtitleText.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            yield return null;
        }

        if (subtitleText) { subtitleText.alpha = 0f; subtitleText.text = ""; }
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }

    IEnumerator MoveAndRotate(Transform target)
    {
        while (
            Vector3.Distance(cam.transform.position, target.position) > 0.05f ||
            Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.1f
        )
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, target.position, moveSpeed * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    IEnumerator ShowBlackPanel(float time)
    {
        blackPanel.SetActive(true);
        HideSubtitle();
        StartCoroutine(FadeOutAudio(6f));
        yield return new WaitForSeconds(time);
        StartCoroutine(LoadScene("CutScene 2"));
    }

    IEnumerator LoadScene(string name)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(name);
    }

    IEnumerator FadeOutAudio(float duration)
    {
        List<float> startVolumes = new List<float>();
        foreach (AudioSource audio in audioSources) startVolumes.Add(audio.volume);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            for (int i = 0; i < audioSources.Count; i++)
                audioSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, time / duration);
            yield return null;
        }
        foreach (AudioSource audio in audioSources) audio.volume = 0f;
    }
}