using System.Collections;
using UnityEngine;

public class TrapRow : MonoBehaviour
{
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

        // =========================
        // HIỆN ĐẦU LÂU
        // =========================
        foreach (GameObject skull in skulls)
        {
            skull.SetActive(true);
        }

        // =========================
        // CẢNH BÁO
        // =========================
        yield return new WaitForSeconds(warningTime);

        // =========================
        // ẨN ĐẦU LÂU
        // =========================
        foreach (GameObject skull in skulls)
        {
            skull.SetActive(false);
        }


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
        foreach (GameObject brick in bricks)
        {
            MeshRenderer mesh =
                brick.GetComponent<MeshRenderer>();

            if (mesh != null)
                mesh.enabled = true;

            Collider col =
                brick.GetComponent<Collider>();

            if (col != null)
                col.enabled = true;
        }

        // XONG
        isRunning = false;
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
}