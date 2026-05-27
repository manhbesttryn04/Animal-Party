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
            // Nếu không có row nào
            if (rows == null || rows.Length == 0)
            {
                Debug.LogWarning("Rows is empty!");
                yield break;
            }

            // List chống random trùng
            List<int> usedIndexes = new List<int>();

            int count = 0;

            // Mặc định random 3 hàng
            int randomRowCount = 3;

            // Nếu đạt tốc độ max thì random 4 hàng
            if (currentDelay <= minDelay)
            {
                randomRowCount = 4;
            }

            // Không vượt quá số row hiện có
            int maxRows = Mathf.Min(randomRowCount, rows.Length);

            // RANDOM ROWS
            while (count < maxRows)
            {
                int rand = Random.Range(0, rows.Length);

                // Nếu row chưa được chọn
                if (!usedIndexes.Contains(rand))
                {
                    usedIndexes.Add(rand);

                    // Chạy trap row
                    StartCoroutine(rows[rand].RowRoutine());

                    count++;
                }
            }

            // Đợi trap chạy
            yield return new WaitForSeconds(2f);

            // CHECK PLAYER 1
            if (manager.currentPlayer1.transform.position.y < 0.5f)
            {
                PlayerMiniGame player1 =
                    manager.currentPlayer1.GetComponent<PlayerMiniGame>();

                if (player1 != null)
                {
                    player1.Respawn();
                }
            }

            // CHECK PLAYER 2
            if (manager.currentPlayer2.transform.position.y < 0.5f)
            {
                PlayerMiniGame player2 =
                    manager.currentPlayer2.GetComponent<PlayerMiniGame>();

                if (player2 != null)
                {
                    player2.Respawn();
                }
            }

            // Delay giữa round
            yield return new WaitForSeconds(currentDelay);

            // Giảm delay sau mỗi round
            currentDelay -= delayDecrease;

            // Không cho thấp hơn minDelay
            if (currentDelay < minDelay)
            {
                currentDelay = minDelay;
            }

            Debug.Log("Current Delay: " + currentDelay);
        }
    }
}