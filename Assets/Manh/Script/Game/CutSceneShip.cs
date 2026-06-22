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
    public GameObject skipHintObject;
    public TMP_Text skipHintText;

    [Header("--- GRADIENT PRESET ---")]
    [Tooltip("NormalGradient: trái #5C2E00, phải #1A0A00")]
    public TMP_ColorGradient normalGradient;
    [Tooltip("FinalGradient: 4 góc đều #7A0000")]
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE ---")]
    public Color normalOutlineColor = new Color(1f, 1f, 1f, 0.2f);
    public Color finalOutlineColor = new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    [Tooltip("Kéo 5 file voice vào đây theo thứ tự câu 1→5")]
    public AudioClip[] narratorVoices = new AudioClip[5];
    [Range(0f, 1f)] public float narratorVolume = 0.9f;

    [Header("--- TIMING ---")]
    public float typeSpeed = 0.035f;
    public float fadeOutDuration = 0.3f;
    public float betweenLineFade = 0.2f;

    private AudioSource narratorSource;
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
        // Tạo AudioSource riêng cho narrator
        narratorSource = gameObject.AddComponent<AudioSource>();
        narratorSource.playOnAwake = false;
        narratorSource.loop = false;
        narratorSource.volume = narratorVolume;

        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (subtitleText) { subtitleText.text = ""; subtitleText.alpha = 1f; }
        if (skipHintObject) skipHintObject.SetActive(false);

        StartCoroutine(CutScene());
        StartCoroutine(ShowSkipHint());
    }

    private void Update()
    {
        if (!isSkipped && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            SkipCutscene();
    }

    private void SkipCutscene()
    {
        isSkipped = true;
        StopAllCoroutines();
        if (narratorSource) narratorSource.Stop();
        HideSubtitleImmediate();
        if (skipHintObject) skipHintObject.SetActive(false);
        StartCoroutine(LoadScene("CutScene 2"));
    }

    IEnumerator ShowSkipHint()
    {
        yield return new WaitForSeconds(1f);
        if (skipHintObject) skipHintObject.SetActive(true);
        if (skipHintText)
        {
            skipHintText.alpha = 0f;
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                skipHintText.alpha = Mathf.Lerp(0f, 0.7f, t / 0.5f);
                yield return null;
            }
        }
    }

    IEnumerator CutScene()
    {
        source.PlayOneShot(shipVoiceClip);
        source.PlayOneShot(shipMoveClip);

        // Point 1
        StartCoroutine(ShowSubtitleWithVoice(0, false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[0]));
        yield return new WaitForSeconds(1f);

        // Point 2
        StartCoroutine(ShowSubtitleWithVoice(1, false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[1]));
        yield return new WaitForSeconds(1f);

        // Point 3
        StartCoroutine(ShowSubtitleWithVoice(2, false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[2]));
        yield return new WaitForSeconds(1f);

        // Point 4
        StartCoroutine(ShowSubtitleWithVoice(3, false));
        yield return StartCoroutine(MoveAndRotate(transVideoList[3]));
        yield return new WaitForSeconds(1f);

        // Point 5 — flash đen + teleport
        if (narratorSource) narratorSource.Stop();
        HideSubtitleImmediate();
        blackFlastPanel.SetActive(true);
        cam.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cam.transform.position = transVideoList[4].position;
        yield return new WaitForSeconds(0.5f);

        // Point 6 — câu cuối đỏ son
        StartCoroutine(MoveAndRotate(transVideoList[5]));
        yield return new WaitForSeconds(1.5f);
        if (skipHintObject) skipHintObject.SetActive(false);

        StartCoroutine(ShowSubtitleWithVoice(4, true));
        yield return new WaitForSeconds(3f);
        StartCoroutine(ShowBlackPanel(4f));
    }

    IEnumerator ShowSubtitleWithVoice(int index, bool isFinal)
    {
        if (narratorSource) narratorSource.Stop();

        // Fade out câu cũ
        if (subtitleText && subtitleText.text != "")
        {
            yield return StartCoroutine(FadeOutLine());
            yield return new WaitForSeconds(betweenLineFade);
        }

        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (subtitleText) subtitleText.alpha = 1f;

        // Áp gradient
        if (subtitleText)
        {
            subtitleText.enableVertexGradient = true;
            subtitleText.colorGradientPreset = isFinal ? finalGradient : normalGradient;
        }

        // Đổi outline
        if (subtitleOutline)
            subtitleOutline.effectColor = isFinal ? finalOutlineColor : normalOutlineColor;

        // Phát narrator voice
        if (narratorSource && narratorVoices.Length > index && narratorVoices[index] != null)
            narratorSource.PlayOneShot(narratorVoices[index], narratorVolume);

        // Typewriter (không có blip)
        string line = storyLines[index];
        if (subtitleText) subtitleText.text = "";
        foreach (char c in line)
        {
            if (subtitleText) subtitleText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Chờ voice đọc xong nếu còn đang phát
        if (narratorSource && narratorSource.isPlaying)
            yield return new WaitWhile(() => narratorSource.isPlaying);
    }

    public void HideSubtitle()
    {
        if (narratorSource) narratorSource.Stop();
        StartCoroutine(FadeOutLine());
    }

    private void HideSubtitleImmediate()
    {
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
        HideSubtitleImmediate();
        if (narratorSource) narratorSource.Stop();
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