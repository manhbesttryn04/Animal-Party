using System.Collections.Generic;
using UnityEngine;

public class MapMiniGameList : MonoBehaviour
{
    public static MapMiniGameList Instance { get; private set; }

    public List<GameObject> mapMiniGameList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void DisableActiveMapMiniGame()
    {
        foreach (GameObject miniGame in mapMiniGameList)
        {
            if (miniGame != null && miniGame.activeSelf)
            {
                miniGame.SetActive(false);
            }
        }
    }
}