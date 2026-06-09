using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TrapRow : MonoBehaviour
{
    public MiniGame1 mini1;

    [Header("Objects")]
    public GameObject[] skulls;
    public GameObject[] bricks;
    public Transform[] sharks;

    [Header("Shark Setting")]
    public float sharkUpHeight = 3f;
    public float sharkSpeed = 5f;

    [Header("Time Setting")]
    public float warningTime = 2f;
    public float sharkStayTime = 2f;

    // KIỂM TRA HÀNG ĐANG CHẠY HAY KHÔNG
    [HideInInspector]
    public bool isRunning = false;

    Vector3[] sharkStartPos;
    Animator[] sharkAnimators;

    void Start()
    {
        mini1 = GetComponentInParent<MiniGame1>();
        // Lưu vị trí ban đầu cá mập
        sharkStartPos = new Vector3[sharks.Length];

        // Mảng animator
        sharkAnimators = new Animator[sharks.Length];

        for (int i = 0; i < sharks.Length; i++)
        {
            sharkStartPos[i] = sharks[i].position;

            // Lấy animator
            sharkAnimators[i] =
                sharks[i].GetComponent<Animator>();

            // Ẩn cá mập lúc đầu
            sharks[i].gameObject.SetActive(false);
        }

        // Ẩn đầu lâu lúc đầu
        foreach (GameObject skull in skulls)
        {
            skull.SetActive(false);
        }
    }

    public IEnumerator RowRoutine()
    {
        // NẾU ĐANG CHẠY THÌ KHÔNG CHẠY TIẾP
        if (isRunning)
            yield break;

        isRunning = true;

        
         yield return new WaitForSeconds(warningTime);


        // =========================
        // HIỆN CÁ MẬP + ATTACK
        // =========================
        for (int i = 0; i < sharks.Length; i++)
        {
            sharks[i].gameObject.SetActive(true);

            if (sharkAnimators[i] != null)
            {
                sharkAnimators[i].SetTrigger("Attack");
            }
        }

        // =========================
        // CÁ MẬP BAY LÊN
        // =========================
        yield return StartCoroutine(MoveSharks(true));

        // =========================
        // ẨN GẠCH
        // =========================
        foreach (GameObject brick in bricks)
        {
            MeshRenderer mesh =
                brick.GetComponent<MeshRenderer>();

            if (mesh != null)
                mesh.enabled = false;

            Collider col =
                brick.GetComponent<Collider>();

            if (col != null)
                col.enabled = false;
        }

        // =========================
        // CHỜ
        // =========================
        yield return new WaitForSeconds(sharkStayTime);

        // =========================
        // CÁ MẬP BAY XUỐNG
        // =========================
        yield return StartCoroutine(MoveSharks(false));

        // =========================
        // ẨN CÁ MẬP
        // =========================
        foreach (Transform shark in sharks)
        {
            shark.gameObject.SetActive(false);
        }

        // =========================
        // HIỆN LẠI GẠCH
        // =========================
        yield return StartCoroutine(
     RestoreBricks()
 );

        // XONG
        isRunning = false;
    }
    IEnumerator RestoreBricks()
    {
        yield return new WaitForSeconds(1f);
        // Hồi từng cục
        for (int i = 0; i < bricks.Length; i++)
        {
            StartCoroutine(
                RestoreSingleBrick(bricks[i])
              
            );
            mini1.source.PlayOneShot(mini1.loadBrickAudio);

            // Delay giữa từng cục
            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator RestoreSingleBrick(GameObject brick)
    {
        float time = 0;
        float duration = 0.3f;

        // BẬT COLLIDER
        Collider col =
            brick.GetComponent<Collider>();
        MeshRenderer mes = brick.GetComponent<MeshRenderer>();
        if(mes != null) { mes.enabled = true; }

        if (col != null)
            col.enabled = true;

        // SCALE TỪ 0
        brick.transform.localScale =
            Vector3.zero;

        while (time < 1)
        {
            time += Time.deltaTime / duration;

            float smoothTime =
                Mathf.SmoothStep(0, 1, time);

            brick.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    Vector3.one,
                    smoothTime
                );

            yield return null;
        }

        // FIX SCALE
        brick.transform.localScale =
            Vector3.one;
    }

    IEnumerator MoveSharks(bool moveUp)
    {
        float time = 0;

        Vector3[] targetPos =
            new Vector3[sharks.Length];

        for (int i = 0; i < sharks.Length; i++)
        {
            if (moveUp)
            {
                targetPos[i] =
                    sharkStartPos[i] +
                    Vector3.up * sharkUpHeight;
            }
            else
            {
                targetPos[i] =
                    sharkStartPos[i];
            }
        }

        while (time < 1)
        {
            time += Time.deltaTime * sharkSpeed;

            for (int i = 0; i < sharks.Length; i++)
            {
                sharks[i].position =
                    Vector3.Lerp(
                        sharks[i].position,
                        targetPos[i],
                        time
                    );
            }

            yield return null;
        }
    }
    public void ShowWarning()
    {
        foreach (GameObject skull in skulls)
        {
            skull.SetActive(true);
        }
    }

    public void HideWarning()
    {
        foreach (GameObject skull in skulls)
        {
            skull.SetActive(false);
        }
    }
}
