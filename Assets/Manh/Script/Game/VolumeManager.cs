using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;

    [Header("Volume")]
    public Volume volume;

    [Header("Default Bloom")]
    public float defaultBloomIntensity = 4f;

    [Header("Default Motion Blur")]
    public float defaultMotionBlurIntensity = 0.5f;

    [Header("Vignette")]
    public float vignetteTargetIntensity = 0.25f;
    public float vignetteMoveTime = 0.5f;

    private Bloom bloom;
    private MotionBlur motionBlur;
    private Vignette vignette;

    private Coroutine vignetteRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        volume.profile = Instantiate(volume.sharedProfile);

        if (volume.profile.TryGet(out bloom))
        {
            bloom.active = true;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = defaultBloomIntensity;
        }

        if (volume.profile.TryGet(out motionBlur))
        {
            motionBlur.active = defaultMotionBlurIntensity > 0f;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = defaultMotionBlurIntensity;
        }

        if (volume.profile.TryGet(out vignette))
        {
            vignette.active = false;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0f;
        }
    }

    public void SetBloomIntensity(float intensity)
    {
        if (bloom != null)
        {
            bloom.active = intensity > 0f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = intensity;
        }
    }

    public float GetBloomIntensity()
    {
        if (bloom != null)
            return bloom.intensity.value;

        return 0f;
    }

    public void SetMotionBlurIntensity(float intensity)
    {
        if (motionBlur != null)
        {
            motionBlur.active = intensity > 0f;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = intensity;
        }
    }

    public void ResetMotionBlur()
    {
        if (motionBlur != null)
        {
            motionBlur.active = defaultMotionBlurIntensity > 0f;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = defaultMotionBlurIntensity;
        }
    }

    //==================================================
    // Vignette
    //==================================================

    public void StartVignette()
    {
        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(MoveVignette(0f, vignetteTargetIntensity, vignetteMoveTime));
    }

    public void ResetVignette()
    {
        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(MoveVignette(vignette.intensity.value, 0f, vignetteMoveTime));
    }

    public IEnumerator MoveVignette(float start, float end, float duration)
    {
        if (vignette == null)
            yield break;

        vignette.active = true;
        vignette.intensity.overrideState = true;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            vignette.intensity.value = Mathf.Lerp(start, end, lerp);

            yield return null;
        }

        vignette.intensity.value = end;

        if (end <= 0f)
            vignette.active = false;
    }
}