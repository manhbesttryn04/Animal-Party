using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance;

    [Header("--- CẤU HÌNH ---")]
    public bool useRandomSubset = false;
    public int activeTrapCount = 3;

    private readonly List<TrapBase> allTraps = new List<TrapBase>();
    private bool isGameRunning = false; // Thêm biến check game đang chạy

    private void Awake()
    {
        Instance = this;
    }

    // ====== ĐĂNG KÝ / HỦY ĐĂNG KÝ ======
    public void Register(TrapBase trap)
    {
        if (!allTraps.Contains(trap))
            allTraps.Add(trap);

        // Nếu Bẫy spawn ra trong lúc Game ĐÃ VÀO TRẬN -> Cho nó Active luôn chứ đừng ép tắt!
        if (isGameRunning && !useRandomSubset)
        {
            trap.SetActive(true);
        }
        else
        {
            trap.SetActive(false);
        }
    }

    public void Unregister(TrapBase trap)
    {
        allTraps.Remove(trap);
    }

    // ====== GỌI TỪ MiniGame6.StartMiniGame() ======
    public void ActivateTraps()
    {
        isGameRunning = true;
        if (allTraps.Count == 0) return;

        if (!useRandomSubset)
        {
            foreach (var trap in allTraps)
                trap.SetActive(true);
            return;
        }

        // Random 1 tập con bẫy được bật
        List<TrapBase> pool = new List<TrapBase>(allTraps);
        int count = Mathf.Min(activeTrapCount, pool.Count);

        foreach (var trap in allTraps)
            trap.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            pool[idx].SetActive(true);
            pool.RemoveAt(idx);
        }
    }

    // ====== GỌI TỪ MiniGame6.StopMiniGame() ======
    public void DeactivateTraps()
    {
        isGameRunning = false;
        foreach (var trap in allTraps)
            trap.SetActive(false);
    }

    public int TrapCount() => allTraps.Count;
}