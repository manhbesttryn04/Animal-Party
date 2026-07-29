using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider slider;
    [SerializeField] private float loadingTime = 2f;

    private void Awake()
    {
        Instance = this;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public IEnumerator ShowLoading()
    {
        if (loadingPanel == null || slider == null)
        {
            Debug.LogWarning("LoadingManager: Chưa gán Loading Panel hoặc Slider.");
            yield break;
        }

        loadingPanel.SetActive(true);
        slider.value = 0f;

        float t = 0f;

        while (t < loadingTime)
        {
            // Vẫn chạy kể cả khi Time.timeScale = 0
            t += Time.unscaledDeltaTime;

            slider.value = Mathf.Clamp01(t / loadingTime);

            yield return null;
        }

        slider.value = 1f;

        // Vẫn chờ được khi game đang Pause
        yield return new WaitForSecondsRealtime(1f);
    }

    public void HideLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}