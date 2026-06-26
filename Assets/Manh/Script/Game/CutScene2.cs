using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CutScene2 : MonoBehaviour
{
    [Header("References")]
    public ShipPatrol ship;
    public SetUpPlayerCutScene set;
    public Camera cam;

    [Header("CutScene Points")]
    public List<Transform> transVideos;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 5f;

    [Header("Ship Stop Position")]
    public Vector3 shipStopPosition = new Vector3(-42.9f, 0.2f, 16f);
    public float shipStopDistance = 0.5f;
    public GameObject blackPanel;
    public GameObject blackClosePanel;
    public GameObject nameMapPanel;

    [Header("Audio")]
    public List<AudioSource> audioSources;

    [Header("--- SUBTITLE ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Outline subtitleOutline;

    [Header("--- GRADIENT PRESET ---")]
    [Tooltip("NormalGradient: trái #5C2E00, phải #1A0A00")]
    public TMP_ColorGradient normalGradient;
    [Tooltip("FinalGradient: 4 góc đều #7A0000")]
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE COLOR ---")]
    public Color normalOutlineColor = new Color(1f, 1f, 1f, 0.2f);
    public Color finalOutlineColor = new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    [Tooltip("Kéo file voice vào đây theo thứ tự — để trống nếu không có voice cho câu đó")]
    public AudioClip[] narratorVoices;
    [Range(0f, 1f)] public float narratorVolume = 0.9f;

    [Header("--- TIMING ---")]
    public float typeSpeed = 0.05f;       // chậm hơn chút → chữ ra từ từ hơn
    public float fadeOutDuration = 0.4f;  // fade out mượt hơn
    public float betweenLineFade = 0.5f;  // chờ lâu hơn giữa 2 câu

    // Nội dung giới thiệu Animal Party
    private string[] storyLines = new string[]
    {
        // [0] — Point 0
        "Welcome to Party Land — where the fun never stops!",
        // [1] — Point 2
        "The wildest party on the island is about to begin!",
        // [2] — Point 3
        "Roll the dice — your fate is in the hands of luck!",
        // [3] — Point 5
        "Survive the craziest mini-games this island has to offer!",
        // [4] — Point 7
        "33 steps or a bag full of Pirate Coins — first one wins!",
        // [5] — Point 8
        "No cheating, no crying — just pure chaotic fun!",
        // [6] — Point 9: câu cuối đỏ son
        "Two players. One island. Who will claim victory?"
    };

    private AudioSource narratorSource;

    private void Start()
    {
        narratorSource = gameObject.AddComponent<AudioSource>();
        narratorSource.playOnAwake = false;
        narratorSource.loop = false;
        narratorSource.volume = narratorVolume;

        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (subtitleText) { subtitleText.text = ""; subtitleText.alpha = 1f; }

        StartCoroutine(CutScene());
        StartCoroutine(CheckShipStop());
    }

    IEnumerator CutScene()
    {
        // Move -> 0 + câu 0
        ShowSubtitleImmediate(storyLines[0], false);
        PlayVoice(0);
        yield return MoveToTransform(transVideos[0]);

        // Teleport -> 1
        TeleportToTransform(transVideos[1]);
        nameMapPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        // Teleport -> 2 + câu 1
        TeleportToTransform(transVideos[2]);
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[1], 1, false));

        // Move -> 3 + câu 2
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[2], 2, false));
        yield return MoveToTransform(transVideos[3]);

        // Teleport -> 4
        TeleportToTransform(transVideos[4]);

        // Move -> 5 + câu 3
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[3], 3, false));
        yield return MoveToTransform(transVideos[5]);

        // Teleport -> 6
        TeleportToTransform(transVideos[6]);
        ship.gameObject.SetActive(false);
        set.StartCutScene();

        // Move -> 7 + câu 4
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[4], 4, false));
        yield return MoveToTransform(transVideos[7]);

        // Teleport -> 8 + câu 5
        TeleportToTransform(transVideos[10]);
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[5], 5, false));
        nameMapPanel.SetActive(false);
        yield return new WaitForSeconds(3f);
        moveSpeed = 100f;

        // Move -> 9 + câu 6 (câu cuối đỏ son — chờ 2 nhân vật xuất hiện trước)
        yield return new WaitForSeconds(2f); // chờ 2 nhân vật xuất hiện
        typeSpeed = 0.08f;                   // chữ ra chậm hơn cho câu cuối
        yield return StartCoroutine(ShowSubtitleWithVoice(storyLines[6], 6, true));
        yield return MoveToTransform(transVideos[9]);
        yield return new WaitForSeconds(5f); // giữ câu cuối lâu hơn

        HideSubtitleImmediate();
        set.StartMovePlayer();
        StartCoroutine(FadeOutAudio(4f));
        blackClosePanel.gameObject.SetActive(true);
        TeleportToTransform(transVideos[10]);
        yield return new WaitForSeconds(4f);
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        SceneManager.LoadScene("MainScene");

        Debug.Log("CutScene Finished");
    }

    // Hiện subtitle + phát voice + typewriter
    IEnumerator ShowSubtitleWithVoice(string line, int voiceIndex, bool isFinal)
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

        // Phát voice
        PlayVoice(voiceIndex);

        // Typewriter
        if (subtitleText) subtitleText.text = "";
        foreach (char c in line)
        {
            if (subtitleText) subtitleText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Chờ voice xong
        if (narratorSource && narratorSource.isPlaying)
            yield return new WaitWhile(() => narratorSource.isPlaying);
    }

    // Hiện subtitle ngay (không typewriter) — dùng cho điểm đầu
    void ShowSubtitleImmediate(string line, bool isFinal)
    {
        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (subtitleText)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = line;
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

    // ====== Logic gốc giữ nguyên ======
    IEnumerator CheckShipStop()
    {
        while (Vector3.Distance(ship.transform.position, shipStopPosition) > shipStopDistance)
            yield return null;
        ship.isStop = true;
    }

    IEnumerator MoveToTransform(Transform target)
    {
        while (
            Vector3.Distance(cam.transform.position, target.position) > 0.05f ||
            Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.5f)
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position, target.position, moveSpeed * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    void TeleportToTransform(Transform target)
    {
        StartCoroutine(ShowBlackPanel(2));
        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    IEnumerator ShowBlackPanel(float time = 1f)
    {
        blackPanel.SetActive(true);
        yield return new WaitForSeconds(time);
        blackPanel.SetActive(false);
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