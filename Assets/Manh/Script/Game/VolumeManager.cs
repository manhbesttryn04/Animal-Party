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

    private Bloom bloom;

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

        // Tạo bản copy của Profile để chỉnh lúc runtime
        volume.profile = Instantiate(volume.sharedProfile);

        // Lấy Bloom
        if (volume.profile.TryGet(out bloom))
        {
            // Đảm bảo Bloom được bật
            bloom.active = true;

            // Đặt Intensity mặc định = 4
            bloom.intensity.overrideState = true;
            bloom.intensity.value = defaultBloomIntensity;
        }
    }

    /// <summary>
    /// Đổi cường độ Bloom
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        if (bloom != null)
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = intensity;
        }
    }

    /// <summary>
    /// Lấy cường độ Bloom hiện tại
    /// </summary>
    public float GetBloomIntensity()
    {
        if (bloom != null)
            return bloom.intensity.value;

        return 0f;
    }
}