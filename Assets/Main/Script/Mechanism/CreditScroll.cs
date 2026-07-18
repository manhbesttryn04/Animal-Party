using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroll : MonoBehaviour
{
    [Header("Credit Text")]
    public RectTransform creditText;

    [Header("Credit Position")]
    public Vector2 startPosition;
    public Vector2 endPosition;

    [Header("Intro Panel")]
    public CanvasGroup introPanel;
    public float introFadeTime = 4f;

    [Header("Credit Time")]
    public float creditTime = 60f;

    [Header("Outro Panel")]
    public GameObject outroPanel;
    public float outroFadeTime = 6f;

    [Header("Music")]
    public AudioSource musicSource1;
    public AudioSource musicSource2;

    [Header("THE END")]
    public TextMeshProUGUI theEndText;
    public float theEndFadeTime = 1.5f;
    public float waitAfterTheEnd = 2f;

    [Header("Load Scene")]
    public string loadSceneName = "MainMenu";

    private float timer;
    private bool canScroll;
    private bool finishCredit;

    private void Start()
    {
        timer = 0f;
        canScroll = false;
        finishCredit = false;

        //==========================
        // Credit
        //==========================

        if (creditText != null)
            creditText.anchoredPosition = startPosition;

        //==========================
        // Intro Panel
        //==========================

        if (introPanel != null)
        {
            introPanel.gameObject.SetActive(true);
            introPanel.alpha = 1f;
        }

        //==========================
        // Outro Panel
        //==========================

        if (outroPanel != null)
            outroPanel.SetActive(false);

        //==========================
        // THE END
        //==========================

        if (theEndText != null)
        {
            Color c = theEndText.color;
            c.a = 0f;
            theEndText.color = c;
        }

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        float timer = 0f;

        while (timer < introFadeTime)
        {
            timer += Time.deltaTime;

            introPanel.alpha =
                Mathf.Lerp(1f, 0f, timer / introFadeTime);

            yield return null;
        }

        introPanel.alpha = 0f;
        introPanel.gameObject.SetActive(false);

        canScroll = true;
    }

    private void Update()
    {
        if (!canScroll)
            return;

        if (finishCredit)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / creditTime);

        if (creditText != null)
        {
            creditText.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, t);
        }

        if (t >= 1f)
        {
            finishCredit = true;
            StartCoroutine(EndRoutine());
        }
    }

    private IEnumerator EndRoutine()
    {
        //==========================
        // Bật Outro Panel
        //==========================

        if (outroPanel != null)
            outroPanel.SetActive(true);

        //==========================
        // Fade Audio
        //==========================

        float startVolume1 = musicSource1 != null ? musicSource1.volume : 0f;
        float startVolume2 = musicSource2 != null ? musicSource2.volume : 0f;

        float timer = 0f;

        while (timer < outroFadeTime)
        {
            timer += Time.deltaTime;

            float percent = Mathf.Clamp01(timer / outroFadeTime);

            if (musicSource1 != null)
                musicSource1.volume =
                    Mathf.Lerp(startVolume1, 0f, percent);

            if (musicSource2 != null)
                musicSource2.volume =
                    Mathf.Lerp(startVolume2, 0f, percent);

            yield return null;
        }

        if (musicSource1 != null)
            musicSource1.volume = 0f;

        if (musicSource2 != null)
            musicSource2.volume = 0f;

        //==========================
        // Fade THE END
        //==========================

        yield return StartCoroutine(FadeTheEnd());

        //==========================
        // Đợi
        //==========================

        yield return new WaitForSeconds(waitAfterTheEnd);

        //==========================
        // Load Scene
        //==========================

      //  SceneManager.LoadScene(loadSceneName);
    }

    private IEnumerator FadeTheEnd()
    {
        if (theEndText == null)
            yield break;

        Color color = theEndText.color;

        float timer = 0f;

        while (timer < theEndFadeTime)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, timer / theEndFadeTime);

            theEndText.color = color;

            yield return null;
        }

        color.a = 1f;
        theEndText.color = color;
    }
}