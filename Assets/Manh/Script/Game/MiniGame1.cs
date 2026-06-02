using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame1 : MonoBehaviour
{
    [Header("Manager")]
    public MiniGameManager manager;

    [Header("Trap Rows")]
    public TrapRow[] rows;

    [Header("Round Delay")]

    // Thời gian nghỉ ban đầu giữa các round
    public float delayBetweenRounds = 3f;

    // Delay nhỏ nhất
    public float minDelay = 1f;

    // Mỗi round giảm bao nhiêu giây
    public float delayDecrease = 0.2f;

    [Header("Start Delay")]

    // Thời gian chờ trước khi game bắt đầu
    public float startDelay = 5f;

    // Kiểm tra game đang chạy hay không
    bool isRunning = false;

    // Delay hiện tại runtime
    public float currentDelay;

    public void StartMiniGame()
    {
        // Nếu game đang chạy thì không start nữa
        if (isRunning)
            return;

        StartCoroutine(RandomRowsRoutine());
    }

    public void StopMiniGame()
    {
        isRunning = false;

        StopAllCoroutines();
    }

    IEnumerator RandomRowsRoutine()
    {
        isRunning = true;

        // Reset delay khi start game
        currentDelay = delayBetweenRounds;

        // Delay đầu game
        yield return new WaitForSeconds(startDelay);

        while (isRunning)
        {
            // =========================
            // CHECK ROW
            // =========================

            if (rows == null || rows.Length == 0)
            {
                Debug.LogWarning("Rows is empty!");
                yield break;
            }

            // =========================
            // RANDOM ROW
            // =========================

            List<int> usedIndexes =
                new List<int>();

            int count = 0;

            // Mặc định random 3 hàng
            int randomRowCount = 3;

            // Nếu đạt tốc độ max thì random 4 hàng
            if (currentDelay <= minDelay)
            {
                randomRowCount = 4;
            }

            // Không vượt quá số row hiện có
            int maxRows =
                Mathf.Min(
                    randomRowCount,
                    rows.Length
                );

            // =========================
            // RANDOM KHÔNG TRÙNG
            // =========================

            List<TrapRow> selectedRows =
      new List<TrapRow>();

            while (count < maxRows)
            {
                int rand =
                    Random.Range(
                        0,
                        rows.Length
                    );

                if (!usedIndexes.Contains(rand))
                {
                    usedIndexes.Add(rand);

                    selectedRows.Add(rows[rand]);

                    count++;
                }
            }
            foreach (TrapRow row in selectedRows)
            {
                // HIỆN WARNING
                row.ShowWarning();

                // PLAYER NHÌN
                yield return new WaitForSeconds(0.4f);

                // TẮT WARNING
                row.HideWarning();

                // DELAY NHỎ
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.5f);

            // CHẠY TẤT CẢ TRAP
            foreach (TrapRow row in selectedRows)
            {
                StartCoroutine(
                    row.RowRoutine()
                );
            }

            // =========================
            // ĐỢI TẤT CẢ ROW XONG
            // =========================

            yield return StartCoroutine(
                WaitForRowsFinished()
            );

            // =========================
            // CHECK PLAYER 1
            // =========================

            if (
                manager.currentPlayer1
                .transform.position.y < 0.5f
            )
            {
                PlayerMiniGame player1 =
                    manager.currentPlayer1
                    .GetComponent<PlayerMiniGame>();

                if (player1 != null)
                {
                    player1.Respawn();
                }
            }

            // =========================
            // CHECK PLAYER 2
            // =========================

            if (
                manager.currentPlayer2
                .transform.position.y < 0.5f
            )
            {
                PlayerMiniGame player2 =
                    manager.currentPlayer2
                    .GetComponent<PlayerMiniGame>();

                if (player2 != null)
                {
                    player2.Respawn();
                }
            }

            // =========================
            // DELAY GIỮA ROUND
            // =========================

            yield return new WaitForSeconds(
                currentDelay
            );

            // =========================
            // GIẢM DELAY
            // =========================

            currentDelay -= delayDecrease;

            // Không cho thấp hơn minDelay
            if (currentDelay < minDelay)
            {
                currentDelay = minDelay;
            }
        }


        IEnumerator WaitForRowsFinished()
        {
            bool allFinished = false;

            while (!allFinished)
            {
                allFinished = true;

                for (int i = 0; i < rows.Length; i++)
                {
                    // Nếu còn row đang chạy
                    if (rows[i].isRunning)
                    {
                        allFinished = false;
                        break;
                    }
                }

                yield return null;
            }
        }
    }
}