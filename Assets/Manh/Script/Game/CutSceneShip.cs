using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Tooltip("Thời gian fade in (giây)")]
    public float fadeInDuration = 0.5f;

    [Tooltip("Thời gian fade out (giây)")]
    public float fadeOutDuration = 0.3f;

    private string[] storyLines = new string[]
    {
        "Somewhere in the vast ocean, a ship sails toward an unknown island...",
        "On board: five animals, each dreaming of glory and adventure.",
        "The island holds ancient mini-games, forgotten by time.",
        "Only the cleverest and bravest will claim the ultimate prize.",
        // [4] dùng cho Point 6 cuối cutscene
        "Let the Animal Party begin!"
    };

    private Coroutine typingCoroutine;

    private void Start()
    {
        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (subtitleText)
        {
            subtitleText.text = "";
            subtitleText.alpha = 0f;
        }
        StartCoroutine(CutScene());
    }

    IEnumerator CutScene()
    {
        source.PlayOneShot(shipVoiceClip);
        source.PlayOneShot(shipMoveClip);

        // Point 1
        ShowSubtitle(storyLines[0]);
        yield return StartCoroutine(MoveAndRotate(transVideoList[0]));
        yield return new WaitForSeconds(1f);

        // Point 2
        ShowSubtitle(storyLines[1]);
        yield return StartCoroutine(MoveAndRotate(transVideoList[1]));
        yield return new WaitForSeconds(1f);

        // Point 3
        ShowSubtitle(storyLines[2]);
        yield return StartCoroutine(MoveAndRotate(transVideoList[2]));
        yield return new WaitForSeconds(1f);

        // Point 4
        ShowSubtitle(storyLines[3]);
        yield return StartCoroutine(MoveAndRotate(transVideoList[3]));
        yield return new WaitForSeconds(1f);

        // Point 5 — flash đen + camera teleport (không subtitle)
        HideSubtitle();
        blackFlastPanel.SetActive(true);
        cam.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cam.transform.position = transVideoList[4].position;
        yield return new WaitForSeconds(0.5f);

        // Point 6 — hiện "Let the Animal Party begin!" + fade out scene
        // Chờ camera di chuyển 1 chút rồi mới hiện chữ cuối
        StartCoroutine(MoveAndRotate(transVideoList[5]));
        yield return new WaitForSeconds(1.5f); // chờ 1.5s rồi hiện chữ
        ShowSubtitle(storyLines[4]);
        yield return new WaitForSeconds(3f);   // giữ chữ 3s rồi fade màn hình đen
        StartCoroutine(ShowBlackPanel(4f));
    }

    // ====== FADE IN ======
    public void ShowSubtitle(string line)
    {
        if (subtitlePanel) subtitlePanel.SetActive(true);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(FadeInLine(line));
    }

    public void HideSubtitle()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(FadeOutLine());
    }

    IEnumerator FadeInLine(string line)
    {
        // Set text ngay, fade từ alpha 0 → 1
        if (subtitleText)
        {
            subtitleText.text = line;
            subtitleText.alpha = 0f;
        }

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

        if (subtitleText) subtitleText.alpha = 0f;
        if (subtitleText) subtitleText.text = "";
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }

    // ====== Logic gốc giữ nguyên ======
    IEnumerator MoveAndRotate(Transform target)
    {
        while (
            Vector3.Distance(cam.transform.position, target.position) > 0.05f ||
            Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.1f
        )
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                target.rotation,
                rotateSpeed * Time.deltaTime
            );

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
        foreach (AudioSource audio in audioSources)
            startVolumes.Add(audio.volume);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            for (int i = 0; i < audioSources.Count; i++)
                audioSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, time / duration);
            yield return null;
        }

        foreach (AudioSource audio in audioSources)
            audio.volume = 0f;
    }
}