using System.Collections;
using UnityEngine;

public class ThunderStorm : MonoBehaviour
{
    [Header("References")]
    public Light lightningLight;
    public AudioSource audioSource;
    public AudioClip thunderSound;

    [Header("Settings")]
    public float minTimeBetweenLightning = 10f;
    public float maxTimeBetweenLightning = 60f;

    private float nextLightningTime;
    private bool isLightning;

    private void Start()
    {
        if (lightningLight != null)
            lightningLight.enabled = false;

        SetNextLightning();
    }

    private void Update()
    {
        if (Time.time >= nextLightningTime && !isLightning)
        {
            StartCoroutine(LightningStrike());
            SetNextLightning();
        }
    }

    private void SetNextLightning()
    {
        nextLightningTime = Time.time + Random.Range(minTimeBetweenLightning, maxTimeBetweenLightning);
    }

    private IEnumerator LightningStrike()
    {
        isLightning = true;

        float originalIntensity = lightningLight.intensity;
        int flashCount = Random.Range(2, 6);

        for (int i = 0; i < flashCount; i++)
        {
            // Tăng sáng
            lightningLight.enabled = true;
            lightningLight.intensity = originalIntensity + Random.Range(1.5f, 3f);

            yield return new WaitForSeconds(Random.Range(0.03f, 0.08f));

            // Giảm về bình thường
            lightningLight.intensity = originalIntensity;

            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

        if (audioSource != null && thunderSound != null)
        {
            audioSource.PlayOneShot(thunderSound);
        }

        lightningLight.intensity = originalIntensity;
        lightningLight.enabled = true;

        isLightning = false;
    }
}