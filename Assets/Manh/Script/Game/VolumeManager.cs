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

    private Bloom bloom;
    private MotionBlur motionBlur;

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

        // Tạo bản copy Profile để chỉnh runtime
        volume.profile = Instantiate(volume.sharedProfile);

        // ==========================
        // Bloom
        // ==========================
        if (volume.profile.TryGet(out bloom))
        {
            bloom.active = true;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = defaultBloomIntensity;
        }

        // ==========================
        // Motion Blur
        // ==========================
        if (volume.profile.TryGet(out motionBlur))
        {
            motionBlur.active = defaultMotionBlurIntensity > 0f;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = defaultMotionBlurIntensity;
        }
    }

    //==================================================
    // Bloom
    //==================================================

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

    //==================================================
    // Motion Blur
    //==================================================

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
}