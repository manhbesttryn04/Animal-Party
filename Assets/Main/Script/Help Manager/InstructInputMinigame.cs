using System.Collections.Generic;
using UnityEngine;

public class InstructInputMinigame : MonoBehaviour
{
    [Header("4 Input UI Player 1")]
    public List<GameObject> instructInputListP1 = new List<GameObject>();

    [Header("4 Input UI Player 2")]
    public List<GameObject> instructInputListP2 = new List<GameObject>();

    [Header("Input Minigame 1")]
    public List<bool> instructInputMinigame1 = new List<bool>();

    [Header("Input Minigame 2")]
    public List<bool> instructInputMinigame2 = new List<bool>();

    [Header("Input Minigame 3")]
    public List<bool> instructInputMinigame3 = new List<bool>();

    [Header("Input Minigame 4")]
    public List<bool> instructInputMinigame4 = new List<bool>();

    [Header("Input Minigame 5")]
    public List<bool> instructInputMinigame5 = new List<bool>();

    [Header("Input Minigame 6")]
    public List<bool> instructInputMinigame6 = new List<bool>();

    [Header("Input Minigame 7")]
    public List<bool> instructInputMinigame7 = new List<bool>();

    [Header("Input Minigame 8")]
    public List<bool> instructInputMinigame8 = new List<bool>();

    private const int INPUT_COUNT = 5;

    private void Start()
    {
        HideAllInput();
    }

    /// <summary>
    /// indexMinigame chạy từ 1 đến 8.
    /// Ví dụ:
    /// 1 = Minigame 1
    /// 2 = Minigame 2
    /// 8 = Minigame 8
    /// </summary>
    public void ShowInputMinigame(int indexMinigame)
    {
        // Chuyển từ số minigame 1–8 sang index 0–7
        int listIndex = indexMinigame - 1;

        List<bool> selectedBoolList = GetBoolList(listIndex);

        if (selectedBoolList == null)
        {
            Debug.LogWarning(
                $"Index minigame không hợp lệ: {indexMinigame}. " +
                "Index phải từ 1 đến 8."
            );

            HideAllInput();
            return;
        }

        ApplyBoolToInput(selectedBoolList);
    }

    /// <summary>
    /// Lấy List bool tương ứng với minigame.
    /// </summary>
    private List<bool> GetBoolList(int listIndex)
    {
        switch (listIndex)
        {
            case 0:
                return instructInputMinigame1;

            case 1:
                return instructInputMinigame2;

            case 2:
                return instructInputMinigame3;

            case 3:
                return instructInputMinigame4;

            case 4:
                return instructInputMinigame5;

            case 5:
                return instructInputMinigame6;

            case 6:
                return instructInputMinigame7;

            case 7:
                return instructInputMinigame8;

            default:
                return null;
        }
    }

    /// <summary>
    /// Bật hoặc tắt 4 UI input của P1 và P2
    /// theo List bool của minigame.
    /// </summary>
    private void ApplyBoolToInput(List<bool> boolList)
    {
        for (int i = 0; i < INPUT_COUNT; i++)
        {
            bool state = i < boolList.Count && boolList[i];

            // Player 1
            if (i < instructInputListP1.Count &&
                instructInputListP1[i] != null)
            {
                instructInputListP1[i].SetActive(state);
            }

            // Player 2
            if (i < instructInputListP2.Count &&
                instructInputListP2[i] != null)
            {
                instructInputListP2[i].SetActive(state);
            }
        }
    }

    /// <summary>
    /// Ẩn toàn bộ UI input của Player 1 và Player 2.
    /// </summary>
    public void HideAllInput()
    {
        foreach (GameObject inputP1 in instructInputListP1)
        {
            if (inputP1 != null)
            {
                inputP1.SetActive(false);
            }
        }

        foreach (GameObject inputP2 in instructInputListP2)
        {
            if (inputP2 != null)
            {
                inputP2.SetActive(false);
            }
        }
    }
}