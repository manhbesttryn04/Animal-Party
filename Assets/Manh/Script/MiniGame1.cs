using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame1 : MonoBehaviour
{
    public TrapRow[] rows;

    public float delayBetweenRounds = 3f;

    void Start()
    {
        StartCoroutine(RandomRowsRoutine());
    }

    IEnumerator RandomRowsRoutine()
    {
        while (true)
        {
            List<int> usedIndexes = new List<int>();

            List<Coroutine> runningRows =
                new List<Coroutine>();

            int count = 0;

            // RANDOM 3 HÀNG
            while (count < 3)
            {
                int rand =
                    Random.Range(0, rows.Length);

                if (!usedIndexes.Contains(rand))
                {
                    usedIndexes.Add(rand);

                    Coroutine c =
                        StartCoroutine(
                            rows[rand].RowRoutine()
                        );

                    runningRows.Add(c);

                    count++;
                }
            }

            // ĐỢI CHO TOÀN BỘ TRAP XONG
            yield return new WaitForSeconds(3f);

            // ĐỢI THÊM 5 GIÂY MỚI RANDOM TIẾP
            yield return new WaitForSeconds(
                delayBetweenRounds
            );
        }
    }
}