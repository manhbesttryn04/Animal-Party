using Unity.VisualScripting;
using UnityEngine;

public class TheSeaAudio : MonoBehaviour
{
    private void Start()
    {
        var audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.PlayEnvironment(audio.theNightClip);
        }
    }
    private void OnEnable()
    {
        var audio = AudioManager.Instance;
        if(audio != null)
        {
            audio.PlayEnvironment(audio.theNightClip);
        }
    }
}
