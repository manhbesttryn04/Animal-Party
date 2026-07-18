using System.Collections;
using UnityEngine;

public class PlayerFunnyItem : MonoBehaviour
{
    [Header("Manager")]
    public PlayerManager manager;

    [Header("Funny Box")]
    public bool isBoxFunny = false;
    public GameObject boxFunny;

    [Header("Move")]
    public float moveHeight = 2f;
    public float moveDuration = 0.5f;

    private Vector3 startPos;
    private Coroutine moveRoutine;
    private bool boxRaised;

    private void Start()
    {
        if (boxFunny == null)
        {
            Debug.LogError("Funny Box chưa được gán!");
            return;
        }

        startPos = boxFunny.transform.localPosition;
        boxFunny.SetActive(false);
    }

    private void Update()
    {
        if (boxFunny == null || manager == null)
            return;

        // Không có hiệu ứng thùng
        if (!isBoxFunny)
        {
            boxFunny.SetActive(false);
            boxRaised = false;
            return;
        }

        // Có hiệu ứng thùng
        boxFunny.SetActive(true);

        bool isMoving = false;

        // Player 1 - WASD
        if (!manager.playerType.isPlayer2)
        {
            isMoving =
                Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D);
        }
        // Player 2 - Arrow Keys
        else
        {
            isMoving =
                Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetKey(KeyCode.RightArrow);
        }

        // Di chuyển => nhấc thùng lên
        if (isMoving)
        {
            if (!boxRaised)
            {
                boxRaised = true;
                RaiseBox();
            }
        }
        // Đứng yên => hạ thùng xuống
        else
        {
            if (boxRaised)
            {
                boxRaised = false;
                LowerBox();
            }
        }
    }

    // Hạ thùng xuống che người chơi
    public void LowerBox()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        Vector3 from = boxFunny.transform.localPosition;
        Vector3 to = startPos;

        moveRoutine = StartCoroutine(MoveSmooth(from, to));
    }

    // Nhấc thùng lên
    public void RaiseBox()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        Vector3 from = boxFunny.transform.localPosition;
        Vector3 to = startPos + Vector3.up * moveHeight;

        moveRoutine = StartCoroutine(MoveSmooth(from, to));
    }

    IEnumerator MoveSmooth(Vector3 from, Vector3 to)
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            boxFunny.transform.localPosition =
                Vector3.Lerp(from, to, t);

            yield return null;
        }

        boxFunny.transform.localPosition = to;
    }
}