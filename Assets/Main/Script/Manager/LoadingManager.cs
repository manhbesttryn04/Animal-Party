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
        loadingPanel.SetActive(false);
    }

    public IEnumerator ShowLoading()
    {
        
        loadingPanel.SetActive(true);
        slider.value = 0;

        float t = 0;

        while (t < loadingTime)
        {
            t += Time.deltaTime;
            slider.value = t / loadingTime;
            yield return null;
        }

        slider.value = 1;

        yield return new WaitForSeconds(1f);

        //loadingPanel.SetActive(false);
    }
    public void HideLoading()
    {
        loadingPanel.SetActive(false);
    }
}