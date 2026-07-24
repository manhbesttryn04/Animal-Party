using UnityEngine;

public class SetUpStartMainScene : MonoBehaviour
{
   
    void Start()
    {
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(AudioManager.Instance.moveCamera);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.musicMainClip);
        }
       if(UIManager.Instance != null)
        {
            UIManager.Instance.canvasNotifi.SetActive(true);
            UIManager.Instance.openSettingPanelButton.SetActive(true);
            UIManager.Instance.FindPlayerManager();
        }
       if(CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowGameCursor();
        }
    }

   

}
