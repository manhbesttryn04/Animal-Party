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

    private Tonemapping tonemapping;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;

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

        if (volume == null)
        {
            Debug.LogError("VolumeManager chưa được gắn Volume!");
            return;
        }

        // Tạo Profile riêng để không chỉnh trực tiếp Profile gốc
        volume.profile = Instantiate(volume.sharedProfile);

        //==================================================
        // Lấy Bloom
        //==================================================

        if (volume.profile.TryGet(out bloom))
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = defaultBloomIntensity;
            bloom.active = true;
        }

        //==================================================
        // Lấy Motion Blur
        //==================================================

        if (volume.profile.TryGet(out motionBlur))
        {
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = defaultMotionBlurIntensity;
            motionBlur.active = defaultMotionBlurIntensity > 0f;
        }

        //==================================================
        // Lấy Vignette
        //==================================================

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0f;
            vignette.active = false;
        }

        //==================================================
        // Lấy Tonemapping
        //==================================================

        if (volume.profile.TryGet(out tonemapping))
        {
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;
            tonemapping.active = true;
        }

        //==================================================
        // Lấy Color Adjustments
        //==================================================

        if (volume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.active = true;
        }

        //==================================================
        // Lấy Depth Of Field
        //==================================================

        if (volume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = true;
        }

        // Mặc định đầu game dùng High
        SetGraphicsQuality(2);
    }

    //==================================================
    // GRAPHICS QUALITY
    // 0 = Low
    // 1 = Medium
    // 2 = High
    //==================================================

    public void SetGraphicsQuality(int quality)
    {
        switch (quality)
        {
            //==================================================
            // LOW
            // Tắt toàn bộ Post Processing trên tất cả Camera
            //==================================================

            case 0:
                SetAllCameraPostProcessing(false);

                Debug.Log(
                    "Graphics Quality: LOW - Post Processing OFF"
                );
                break;

            //==================================================
            // MEDIUM
            // Bật Post Processing + Neutral
            //==================================================

            case 1:
                SetAllCameraPostProcessing(true);

                SetTonemapping(TonemappingMode.Neutral);
                SetBloomActive(true);
                SetMotionBlurActive(false);
                SetColorAdjustmentsActive(true);
                SetDepthOfFieldActive(true);

                Debug.Log(
                    "Graphics Quality: MEDIUM - Post Processing ON"
                );
                break;

            //==================================================
            // HIGH
            // Bật Post Processing + ACES
            //==================================================

            case 2:
                SetAllCameraPostProcessing(true);

                SetTonemapping(TonemappingMode.ACES);
                SetBloomActive(true);
                SetMotionBlurActive(true);
                SetColorAdjustmentsActive(true);
                SetDepthOfFieldActive(true);

                Debug.Log(
                    "Graphics Quality: HIGH - Post Processing ON"
                );
                break;

            default:
                Debug.LogWarning(
                    "Graphics Quality chỉ nhận 0, 1 hoặc 2."
                );
                break;
        }
    }

    //==================================================
    // TONEMAPPING
    //==================================================

    private void SetTonemapping(TonemappingMode mode)
    {
        if (tonemapping == null)
            return;

        tonemapping.active = true;
        tonemapping.mode.overrideState = true;
        tonemapping.mode.value = mode;
    }

    //==================================================
    // BLOOM
    //==================================================

    private void SetBloomActive(bool state)
    {
        if (bloom == null)
            return;

        bloom.active = state;

        if (state)
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = defaultBloomIntensity;
        }
    }

    public void SetBloomIntensity(float intensity)
    {
        if (bloom == null)
            return;

        bloom.active = intensity > 0f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = intensity;
    }

    public float GetBloomIntensity()
    {
        if (bloom != null)
            return bloom.intensity.value;

        return 0f;
    }

    //==================================================
    // MOTION BLUR
    //==================================================

    private void SetMotionBlurActive(bool state)
    {
        if (motionBlur == null)
            return;

        motionBlur.active = state;

        if (state)
        {
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = defaultMotionBlurIntensity;
        }
    }

    public void SetMotionBlurIntensity(float intensity)
    {
        if (motionBlur == null)
            return;

        motionBlur.active = intensity > 0f;
        motionBlur.intensity.overrideState = true;
        motionBlur.intensity.value = intensity;
    }

    public void ResetMotionBlur()
    {
        if (motionBlur == null)
            return;

        motionBlur.active = defaultMotionBlurIntensity > 0f;
        motionBlur.intensity.overrideState = true;
        motionBlur.intensity.value = defaultMotionBlurIntensity;
    }

    //==================================================
    // COLOR ADJUSTMENTS
    //==================================================

    private void SetColorAdjustmentsActive(bool state)
    {
        if (colorAdjustments == null)
            return;

        colorAdjustments.active = state;
    }

    // Có thể gọi từ script khác
    public void SetColorActive(bool state)
    {
        SetColorAdjustmentsActive(state);
    }

    //==================================================
    // DEPTH OF FIELD
    //==================================================

    private void SetDepthOfFieldActive(bool state)
    {
        if (depthOfField == null)
            return;

        depthOfField.active = state;
    }

    // Có thể gọi từ script khác
    public void SetDepthActive(bool state)
    {
        SetDepthOfFieldActive(state);
    }

    //==================================================
    // VIGNETTE
    //==================================================

    public void StartVignette()
    {
        if (vignette == null)
            return;

        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(
            MoveVignette(
                vignette.intensity.value,
                vignetteTargetIntensity,
                vignetteMoveTime
            )
        );
    }

    public void ResetVignette()
    {
        if (vignette == null)
            return;

        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(
            MoveVignette(
                vignette.intensity.value,
                0f,
                vignetteMoveTime
            )
        );
    }

    public IEnumerator MoveVignette(
        float start,
        float end,
        float duration
    )
    {
        if (vignette == null)
            yield break;

        vignette.active = true;
        vignette.intensity.overrideState = true;

        if (duration <= 0f)
        {
            vignette.intensity.value = end;
            vignette.active = end > 0f;
            vignetteRoutine = null;
            yield break;
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / duration);

            vignette.intensity.value = Mathf.Lerp(
                start,
                end,
                progress
            );

            yield return null;
        }

        vignette.intensity.value = end;

        if (end <= 0f)
            vignette.active = false;

        vignetteRoutine = null;
    }
    //==================================================
    // CAMERA POST PROCESSING
    //==================================================

    private void SetAllCameraPostProcessing(bool state)
    {
        Camera[] cameras = FindObjectsByType<Camera>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (Camera cameraItem in cameras)
        {
            if (cameraItem == null)
                continue;

            UniversalAdditionalCameraData cameraData =
                cameraItem.GetUniversalAdditionalCameraData();

            if (cameraData != null)
            {
                cameraData.renderPostProcessing = state;
            }
        }
    }
}