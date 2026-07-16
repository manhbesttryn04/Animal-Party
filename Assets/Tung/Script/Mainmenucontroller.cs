using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("--- TÊN SCENE GAME CHÍNH ---")]
    [Tooltip("Tên scene chứa minigame, phải đúng tên trong Build Settings")]
    public string gameSceneName = "GameScene";

    [Header("--- SETTINGS PANEL ---")]
    [Tooltip("Panel cài đặt âm thanh, kéo vào đây, để Inactive trong Hierarchy")]
    public GameObject settingsPanel;

    // ====== NÚT START ======
    private void Start()
    {
        var cursor = CursorManager.Instance;
        if(cursor != null)
        {
            cursor.ShowGameCursor();
        }
    }
    public void OnStartClicked()
    {
        SafePlayClick();
        SafeStopMusic();

        StartCoroutine(StartLoadScene());
    }
    IEnumerator StartLoadScene()
    {
        var audio = AudioManager1.Instance;
        if(audio != null)
        {
            audio.ambientSource.volume = 0;
        }
        var cursor = CursorManager.Instance;
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        SceneManager.LoadScene(gameSceneName);
    }

    // ====== NÚT SETTINGS ======
    public void OnSettingsClicked()
    {
        SafePlayClick();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsClicked()
    {
        SafePlayClick();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ====== SLIDER NHẠC NỀN ======
    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager1.Instance != null)
            AudioManager1.Instance.SetMusicVolume(value);
    }

    // ====== SLIDER AMBIENT (sóng biển, hải âu) ======
    public void OnAmbientVolumeChanged(float value)
    {
        if (AudioManager1.Instance != null)
            AudioManager1.Instance.SetAmbientVolume(value);
    }

    // ====== SLIDER ÂM THANH CLICK / SFX ======
    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager1.Instance != null)
            AudioManager1.Instance.SetSfxVolume(value);
    }

    // ====== NÚT EXIT GAME ======
    public void OnExitClicked()
    {
        SafePlayClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SafePlayClick()
    {
        if (AudioManager1.Instance != null) AudioManager1.Instance.PlayClick();
    }

    private void SafeStopMusic()
    {
        if (AudioManager1.Instance != null) AudioManager1.Instance.StopMusic();
    }
}