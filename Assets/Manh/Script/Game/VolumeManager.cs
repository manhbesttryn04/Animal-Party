using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;

    [Header("Volume")]
    public Volume volume;

    private Bloom bloom;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo bản copy profile để chỉnh lúc runtime
        volume.profile = Instantiate(volume.sharedProfile);

        // Lấy Bloom
        volume.profile.TryGet(out bloom);
    }

    /// <summary>
    /// Đổi cường độ Bloom
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        if (bloom != null)
        {
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