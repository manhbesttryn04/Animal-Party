using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutScene2 : MonoBehaviour
{
    [Header("References")]
    public ShipPatrol ship;
    public SetUpPlayerCutScene set;
    public Camera cam;
    public GameObject teleport;

    [Header("CutScene Points")]
    public List<Transform> transVideos;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 5f;

    [Header("Ship Stop Position")]
    public Vector3 shipStopPosition =
        new Vector3(-42.9f, 0.2f, 16f);

    public float shipStopDistance = 0.5f;

    [Header("Panels")]
    public GameObject blackPanel;
    public GameObject blackClosePanel;
    public GameObject nameMapPanel;

    [Header("--- SUBTITLE ---")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;
    public Outline subtitleOutline;

    [Header("--- SKIP UI ---")]
    [Tooltip("GameObject chứa chữ Press Space to Skip")]
    public GameObject skipHintObject;

    [Tooltip("TMP Text của skip hint")]
    public TMP_Text skipHintText;

    [Tooltip(
        "Button index dùng để skip bằng tay cầm.\n" +
        "Legacy Input thường dùng Button 1 cho Xbox B / PlayStation Circle."
    )]
    [Range(0, 19)]
    [SerializeField] private int controllerSkipButtonIndex = 1;

    [Header("--- GRADIENT PRESET ---")]
    [Tooltip("NormalGradient: trái #5C2E00, phải #1A0A00")]
    public TMP_ColorGradient normalGradient;

    [Tooltip("FinalGradient: 4 góc đều #7A0000")]
    public TMP_ColorGradient finalGradient;

    [Header("--- OUTLINE COLOR ---")]
    public Color normalOutlineColor =
        new Color(1f, 1f, 1f, 0.2f);

    public Color finalOutlineColor =
        new Color(1f, 1f, 1f, 0.31f);

    [Header("--- NARRATOR VOICE ---")]
    [Tooltip("Kéo file voice vào đây theo đúng thứ tự câu")]
    public AudioClip[] narratorVoices;

    [Header("--- TIMING ---")]
    public float typeSpeed = 0.05f;
    public float fadeOutDuration = 0.4f;
    public float betweenLineFade = 0.5f;

    [Header("--- END AUDIO FADE ---")]
    public float endAudioFadeTime = 4.5f;

    private bool isSkipped;

    // Sau khi đóng Setting, phải nhả Space/Enter/B/Circle
    // rồi mới cho phép skip cutscene.
    private bool waitSkipReleaseAfterSetting;

    private readonly string[] storyLines =
    {
        "Welcome to Party Land — where the fun never stops!",
        "The wildest party on the island is about to begin!",
        "Roll the dice — your fate is in the hands of luck!",
        "Survive the craziest mini-games",
        "33 steps or a bag full of Pirate Coins — first one wins!",
        "No cheating, no crying — just pure chaotic fun!",
        "Two players. One island. Who will claim victory?"
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

        var audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.SetupMainGameAudio();
            audio.PlayMusic(audio.musicCutScene2Clip);
        }
        var setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.canOpenSettingByEsc = true;
            setting.canOpenSettingByController = true;
        }

        StartCoroutine(CutScene());
        StartCoroutine(CheckShipStop());
        StartCoroutine(ShowSkipHint());
    }

    private void Update()
    {
        if (isSkipped)
            return;

        bool settingOpen =
            SettingManager.Instance != null &&
            SettingManager.Instance.IsSettingBlockingInput;

        // Setting đang mở thì không cho phím hoặc tay cầm skip.
        if (settingOpen)
        {
            waitSkipReleaseAfterSetting = true;
            return;
        }

        // Sau khi đóng Setting, phải nhả phím/nút trước.
        // Tránh nút dùng để đóng Setting làm skip luôn cutscene.
        if (waitSkipReleaseAfterSetting)
        {
            bool skipInputHeld =
                Input.GetKey(KeyCode.Space) ||
                Input.GetKey(KeyCode.Return) ||
                IsControllerSkipHeld();

            if (!skipInputHeld)
            {
                waitSkipReleaseAfterSetting = false;
            }

            return;
        }

        bool keyboardSkipDown =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return);

        bool controllerSkipDown =
            IsControllerSkipDown();

        if (keyboardSkipDown || controllerSkipDown)
        {
            SkipCutscene();
        }
    }

    private bool IsControllerSkipDown()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
            return false;

        bool console1Skip =
            controller.IsConsole1Connected() &&
            controller.GetConsoleButtonDown(
                1,
                controllerSkipButtonIndex
            );

        bool console2Skip =
            controller.IsConsole2Connected() &&
            controller.GetConsoleButtonDown(
                2,
                controllerSkipButtonIndex
            );

        return console1Skip || console2Skip;
    }

    private bool IsControllerSkipHeld()
    {
        ControllerManager controller =
            ControllerManager.Instance;

        if (controller == null)
            return false;

        bool console1SkipHeld =
            controller.IsConsole1Connected() &&
            controller.GetConsoleButton(
                1,
                controllerSkipButtonIndex
            );

        bool console2SkipHeld =
            controller.IsConsole2Connected() &&
            controller.GetConsoleButton(
                2,
                controllerSkipButtonIndex
            );

        return console1SkipHeld || console2SkipHeld;
    }

    private void SkipCutscene()
    {
        if (isSkipped)
            return;

        isSkipped = true;

        StopAllCoroutines();
        StopNarratorVoice();

        var setting = SettingManager.Instance;
        if (setting != null)
        {
            setting.ResetEscSetting();
            setting.canOpenSettingByEsc = false;
            setting.canOpenSettingByController = false;
        }
        AudioManager audio = AudioManager.Instance;

        if (audio != null)
            audio.PauseAudio();

        HideSubtitleImmediate();

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        StartCoroutine(SkipToEnd());
    }

    private IEnumerator SkipToEnd()
    {
        if (blackClosePanel != null)
            blackClosePanel.SetActive(true);

        // Skip thì tắt âm thanh ngay, không cần fade.
        AudioManager audio = AudioManager.Instance;

        if (audio != null)
        {
            audio.ZeroAllAudio();
            audio.PauseAudio();

            if (audio.specialSource != null)
                audio.specialSource.Stop();
        }

        if (LoadingManager.Instance != null)
        {
            yield return StartCoroutine(
                LoadingManager.Instance.ShowLoading()
            );
        }

        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator ShowSkipHint()
    {
        yield return null;

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
                1f,
                time / duration
            );

            yield return null;
        }

        skipHintText.alpha = 1f;
    }

    private IEnumerator CutScene()
    {
        if (!CheckReferences())
            yield break;

        // Move tới point 0 và hiện câu đầu.
        ShowSubtitleImmediate(storyLines[0], false);
        PlayVoice(0);

        yield return StartCoroutine(
            MoveToTransform(transVideos[0])
        );

        // Teleport tới point 1.
        TeleportToTransform(transVideos[1]);

        if (nameMapPanel != null)
            nameMapPanel.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[1],
                1,
                false
            )
        );

        // Teleport tới point 2.
        TeleportToTransform(transVideos[2]);

        // Move tới point 3 và câu số 2.
        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[2],
                2,
                false
            )
        );

        yield return StartCoroutine(
            MoveToTransform(transVideos[3])
        );

        // Teleport tới point 4.
        TeleportToTransform(transVideos[4]);

        // Move tới point 5 và câu số 3.
        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[3],
                3,
                false
            )
        );

        yield return StartCoroutine(
            MoveToTransform(transVideos[5])
        );

        // Teleport tới point 6.
        TeleportToTransform(transVideos[6]);

        if (ship != null)
            ship.gameObject.SetActive(false);

        if (set != null)
            set.StartCutScene();

        // Move tới point 7 và câu số 4.
        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[4],
                4,
                false
            )
        );

        yield return StartCoroutine(
            MoveToTransform(transVideos[7])
        );

        // Teleport tới point 8 và câu số 5.
        if (teleport != null)
            teleport.SetActive(true);

        TeleportToTransform(transVideos[8]);

        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[5],
                5,
                false
            )
        );

        if (nameMapPanel != null)
            nameMapPanel.SetActive(false);

        yield return new WaitForSeconds(3f);

        moveSpeed = 100f;

        if (skipHintObject != null)
            skipHintObject.SetActive(false);

        // Câu cuối.
        yield return new WaitForSeconds(2f);

        typeSpeed = 0.08f;

        StartCoroutine(
            ShowSubtitleWithVoice(
                storyLines[6],
                6,
                true
            )
        );

        yield return StartCoroutine(
            MoveToTransform(transVideos[9])
        );

        yield return new WaitForSeconds(5f);
        /* var setting = SettingManager.Instance;
         if (setting != null)
         {
             setting.ResetEscSetting();
             setting.canOpenSettingByEsc = false;
             setting.canOpenSettingByController = false;
         }*/
        HideSubtitleImmediate();
        StopNarratorVoice();

        if (set != null)
            set.StartMovePlayer();

        if (blackClosePanel != null)
            blackClosePanel.SetActive(true);

        TeleportToTransform(transVideos[10]);

        // Giảm 4 nguồn âm thanh chính theo thời gian.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutAllAudio(
                endAudioFadeTime
            );
        }

        yield return new WaitForSeconds(
            endAudioFadeTime
        );
        var setting = SettingManager.Instance;
        if (setting != null)
        {
            //setting.ResetEscSetting();
            setting.canOpenSettingByEsc = false;
            setting.canOpenSettingByController = false;
        }

        if (LoadingManager.Instance != null)
        {
            yield return StartCoroutine(
                LoadingManager.Instance.ShowLoading()
            );
        }

        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator ShowSubtitleWithVoice(
        string line,
        int voiceIndex,
        bool isFinal)
    {
        if (isSkipped)
            yield break;

        StopNarratorVoice();

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

        PlayVoice(voiceIndex);

        foreach (char character in line)
        {
            if (isSkipped)
                yield break;

            if (subtitleText != null)
                subtitleText.text += character;

            yield return new WaitForSeconds(typeSpeed);
        }

        AudioManager audio = AudioManager.Instance;

        if (audio != null && audio.specialSource != null)
        {
            yield return new WaitWhile(
                () =>
                    !isSkipped &&
                    AudioManager.Instance != null &&
                    AudioManager.Instance.specialSource != null &&
                    AudioManager.Instance.specialSource.isPlaying
            );
        }
    }

    private void ShowSubtitleImmediate(
        string line,
        bool isFinal)
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        if (subtitleText != null)
        {
            subtitleText.alpha = 1f;
            subtitleText.text = line;
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

    private void PlayVoice(int index)
    {
        if (AudioManager.Instance == null)
            return;

        if (narratorVoices == null)
            return;

        if (index < 0 || index >= narratorVoices.Length)
            return;

        if (narratorVoices[index] == null)
            return;

        AudioManager.Instance.PlaySpecial(
            narratorVoices[index]
        );
    }

    private void StopNarratorVoice()
    {
        AudioManager audio = AudioManager.Instance;

        if (audio != null && audio.specialSource != null)
            audio.specialSource.Stop();
    }

    private void HideSubtitleImmediate()
    {
        StopNarratorVoice();

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

        float duration = Mathf.Max(
            0.01f,
            fadeOutDuration
        );

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

    private IEnumerator CheckShipStop()
    {
        if (ship == null)
            yield break;

        while (
            Vector3.Distance(
                ship.transform.position,
                shipStopPosition
            ) > shipStopDistance)
        {
            if (isSkipped)
                yield break;

            yield return null;
        }

        ship.isStop = true;
    }

    private IEnumerator MoveToTransform(
        Transform target)
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
            ) > 0.5f)
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

    private void TeleportToTransform(
        Transform target)
    {
        if (cam == null || target == null)
            return;

        StartCoroutine(ShowBlackPanel(2f));

        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    private IEnumerator ShowBlackPanel(
        float time = 1f)
    {
        if (blackPanel == null)
            yield break;

        blackPanel.SetActive(true);

        yield return new WaitForSeconds(time);

        blackPanel.SetActive(false);
    }

    private bool CheckReferences()
    {
        if (cam == null)
        {
            Debug.LogError(
                "CutScene2 chưa gán Camera."
            );

            return false;
        }

        if (transVideos == null ||
            transVideos.Count < 11)
        {
            Debug.LogError(
                "CutScene2 cần ít nhất 11 Transform."
            );

            return false;
        }

        for (int i = 0; i < 11; i++)
        {
            if (transVideos[i] == null)
            {
                Debug.LogError(
                    "transVideos[" + i + "] chưa được gán."
                );

                return false;
            }
        }

        return true;
    }
}