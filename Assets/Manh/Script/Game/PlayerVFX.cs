using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer normalRenderer;
    public Renderer respawnRenderer;

    [Header("Particle")]
    public ParticleSystem circleEffect;

    [Header("Time")]
    public float dissolveOutTime = 1f;
    public float dissolveInTime = 1f;

    private Material respawnMat;

    private void Awake()
    {
        respawnMat = respawnRenderer.material;

        normalRenderer.gameObject.SetActive(true);
        respawnRenderer.enabled = false;

        respawnMat.SetFloat("_Speed", 0f);
        respawnMat.SetFloat("_Progress", 1f);

        if (circleEffect != null)
            circleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // Hiện -> Ẩn
    public IEnumerator DissolveOutRoutine()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.startLeteClip);
        yield return new WaitForSeconds(1.2f);
        respawnRenderer.enabled = true;
        normalRenderer.gameObject.SetActive(false);

        respawnMat.SetFloat("_Progress", 1f);

        if (circleEffect != null)
        {
            circleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            circleEffect.Play();
        }

        yield return StartCoroutine(SetProgress(1f, -1f, dissolveOutTime));

        normalRenderer.gameObject.SetActive(false);
    }

    // Ẩn -> Hiện
    public IEnumerator DissolveInRoutine()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.endLeteClip);
        respawnRenderer.enabled = true;
        normalRenderer.gameObject.SetActive(false);

        respawnMat.SetFloat("_Progress", -1f);

        if (circleEffect != null)
        {
            circleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            circleEffect.Play();
        }

        yield return StartCoroutine(SetProgress(-1f, 1f, dissolveInTime));

        if (circleEffect != null)
            circleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
     
       ;

        respawnRenderer.enabled = false;
        normalRenderer.gameObject.SetActive(true);
    }

    private IEnumerator SetProgress(float start, float end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float value = Mathf.Lerp(start, end, time / duration);
            respawnMat.SetFloat("_Progress", value);

            yield return null;
        }

        respawnMat.SetFloat("_Progress", end);
    }
}