using UnityEngine;
using UnityEngine.Audio;

public class SetUpStartMainScene : MonoBehaviour
{
   
    void Start()
    {
        var audio = AudioManager.Instance;
        if(audio != null)
        {
            audio.SetupMainGameAudio();
            audio.PlayUI(AudioManager.Instance.moveCamera);
            audio.PlayMusic(AudioManager.Instance.musicMainClip);
        }

       var ui = UIManager.Instance;
       if(ui  != null)
        {
            ui.canvasNotifi.SetActive(true);
            ui.ActiveOpenSettingButton(true);
            ui.FindPlayerManager();
        }

       var cursor = CursorManager.Instance;
       if(cursor != null)
        {

          cursor.SetSceneCursorVisible(true);
          cursor.SetSettingCursorActive(true);
        }
       var setting = SettingManager.Instance;
        if(setting != null)
        {
            setting.ResetSetting();
            setting.isOpenExitButton = true;
            setting.enableSettingMusic = true;
            setting.canOpenSettingByEsc = false;

            setting.canOpenSettingByController = true;
        }
    }

   

}
