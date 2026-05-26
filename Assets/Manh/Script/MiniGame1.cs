using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame1 : MonoBehaviour
{
    public MiniGameManager manager;
    public TrapRow[] rows;

    public float delayBetweenRounds = 3f;

    bool isRunning = false;

    public void StartMiniGame()
    {
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

        yield return new WaitForSeconds(5f);

        while (isRunning)
        {
            List<int> usedIndexes =
                new List<int>();

            int count = 0;

            // RANDOM 3 HÀNG
            while (count < 3)
            {
                int rand =
                    Random.Range(0, rows.Length);

                if (!usedIndexes.Contains(rand))
                {
                    usedIndexes.Add(rand);

                    StartCoroutine(
                        rows[rand].RowRoutine()
                    );

                    count++;
                }
            }

            // ĐỢI TRAP CHẠY
            yield return new WaitForSeconds(3f);
            if(manager.currentPlayer1.transform.position.y < 0.5f)
            {
                manager.currentPlayer1.GetComponent<PlayerMiniGame>().Respawn();
            }
            if(manager.currentPlayer2.transform.position.y < 0.5f)
            {
                manager.currentPlayer2.GetComponent<PlayerMiniGame>().Respawn();
            }

            // DELAY ROUND
            yield return new WaitForSeconds(
                delayBetweenRounds
            );
        }
    }
}