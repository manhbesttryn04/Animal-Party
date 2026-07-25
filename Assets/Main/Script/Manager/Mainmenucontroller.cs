using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("--- TÊN SCENE GAME CHÍNH ---")]
    [Tooltip("Tên scene chứa minigame, phải đúng tên trong Build Settings")]
    public string gameSceneName = "GameScene";



    // ====== NÚT START ======
    private void Start()
    {
        var cursor = CursorManager.Instance;
        if(cursor != null)
        {
            cursor.ShowGameCursor();
        }
        var audio = AudioManager.Instance;
        if(audio != null)
        {
            audio.PlayMusic(audio.musicMainMenuClip);
            audio.PlayEnvironment(audio.theNightClip);
            audio.SetupMainGameAudio(); 
        }
        var setting = SettingManager.Instance;
        if(setting != null)
        { 
            setting.isOpenExitButton = false;
        }
        var volume = VolumeManager.Instance;
        if(volume != null) { 
            volume.ResetVignette();
            //volume.SetGraphicsQuality(volume.currentQuality);
        }

    }
    public void OnStartClicked()
    {
        AudioManager.Instance.PlayUI(AudioManager.Instance.clickButton);

        StartCoroutine(StartLoadScene());
    }
    IEnumerator StartLoadScene()
    {
        var audio = AudioManager.Instance;
        if(audio != null)
        {
            audio.PauseAudio();
        }
        var cursor = CursorManager.Instance;
        if (cursor != null)
        {
            cursor.HideGameCursor();
        }
        SettingManager.Instance.ResetSetting();
        yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        SceneManager.LoadScene(gameSceneName);
    }

    // ====== NÚT SETTINGS ======
    public void OnSettingsClicked()
    {
        SettingManager.Instance.ToggleSetting();
        
    }


    public void OnCloseSettingsClicked()
    {
        SettingManager.Instance.ResetSetting();
    }


    // ====== NÚT EXIT GAME ======
    public void OnExitClicked()
    {
      AudioManager.Instance.PlayUI(AudioManager.Instance.clickButton);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

   
}