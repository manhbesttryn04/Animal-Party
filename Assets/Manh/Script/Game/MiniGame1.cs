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

    [Header("Audio")]
    public AudioSource source;
    public AudioClip warningAudio;
    public AudioClip sharkAttackAudio;
    public AudioClip loadBrickAudio;

    // Kiểm tra game đang chạy hay không
    bool isRunning = false;

    // Delay hiện tại runtime
    public float currentDelay;

    public Coroutine gameRoutine;

    public void StartMiniGame()
    {
        // Nếu đang chạy thì dừng trước
        StopMiniGame();

        gameRoutine = StartCoroutine(RandomRowsRoutine());
    }

    public void StopMiniGame()
    {
        isRunning = false;

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        // Reset trạng thái tất cả row
        foreach (TrapRow row in rows)
        {
            if (row != null)
                row.isRunning = false;
        }
    }

    IEnumerator RandomRowsRoutine()
    {
        isRunning = true;

        // Reset tốc độ mỗi lần bắt đầu game
        currentDelay = delayBetweenRounds;

        // Delay đầu game
        yield return new WaitForSeconds(startDelay);

        while (isRunning)
        {
            if (rows == null || rows.Length == 0)
            {
                Debug.LogWarning("Rows is empty!");
                yield break;
            }

            List<int> usedIndexes = new List<int>();
            List<TrapRow> selectedRows = new List<TrapRow>();

            int randomRowCount = 3;

            if (currentDelay <= minDelay)
                randomRowCount = 4;

            int maxRows =
                Mathf.Min(
                    randomRowCount,
                    rows.Length
                );

            while (selectedRows.Count < maxRows)
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
                }
            }

            // Hiện warning
            foreach (TrapRow row in selectedRows)
            {
                row.ShowWarning();
                source.PlayOneShot(warningAudio);

                yield return new WaitForSeconds(0.4f);

                row.HideWarning();

                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);

            // Chạy trap
            foreach (TrapRow row in selectedRows)
            {
                StartCoroutine(
                    row.RowRoutine()
                );
                source.PlayOneShot(sharkAttackAudio);
            }
         

            // Chờ tất cả row xong
            yield return StartCoroutine(
                WaitForRowsFinished()
            );

            // Check Player 1
            if (
                manager.currentPlayer1.transform.position.y < 0.5f
            )
            {
                PlayerMiniGame player1 =
                    manager.currentPlayer1.GetComponent<PlayerMiniGame>();

                if (player1 != null)
                    player1.Respawn();
            }

            // Check Player 2
            if (
                manager.currentPlayer2.transform.position.y < 0.5f
            )
            {
                PlayerMiniGame player2 =
                    manager.currentPlayer2.GetComponent<PlayerMiniGame>();

                if (player2 != null)
                    player2.Respawn();
            }

            // Delay giữa round
            yield return new WaitForSeconds(
                currentDelay
            );

            // Giảm delay
            currentDelay -= delayDecrease;

            if (currentDelay < minDelay)
                currentDelay = minDelay;
        }

        isRunning = false;
        gameRoutine = null;
    }

    IEnumerator WaitForRowsFinished()
    {
        while (true)
        {
            bool allFinished = true;

            foreach (TrapRow row in rows)
            {
                if (row != null && row.isRunning)
                {
                    allFinished = false;
                    break;
                }
              
            }

            if (allFinished)
                yield break;

            yield return null;
        }
    }
}