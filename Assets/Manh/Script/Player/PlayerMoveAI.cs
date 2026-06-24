using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveAI : MonoBehaviour
{
    #region Variables

    //==========================
    // References
    //==========================

    // Quản lý toàn bộ Player
    public PlayerManager manager;

    // NavMeshAgent dùng để điều khiển di chuyển
    public NavMeshAgent navMeshAgent;

    //==========================
    // Board
    //==========================

    // Danh sách các Point trên bàn cờ
    public List<GameObject> pointCheck = new List<GameObject>();

    // Ô hiện tại của Player
    public int currentIndex = 0;

    // Trạng thái đang di chuyển
    public bool isMoving = false;

    #endregion

    #region Unity Events

    //==================================================
    // Khởi tạo
    //==================================================
    private void Start()
    {
        // Lấy NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Thiết lập NavMesh
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navMeshAgent.avoidancePriority = 50;
        navMeshAgent.updateRotation = false;

        // Lấy danh sách Point
        pointCheck = FindAnyObjectByType<PointCheck>().point.ToList();

        // Tìm lại Point theo tên
        FindPonit();
    }

    //==================================================
    // Update
    //==================================================
    private void Update()
    {
        // Dùng nếu muốn điều khiển BlendTree Walk
        //manager.playerAnimator.playerAnimator.SetFloat("Walk", navMeshAgent.velocity.magnitude);
    }

    #endregion

    #region Movement

    //==================================================
    // Bắt đầu di chuyển
    // Được gọi sau khi tung xúc xắc
    //==================================================
    public void StartMove(int value)
    {
        if (!isMoving)
        {
            StartCoroutine(AIToPoint(value));
        }
    }

    //==================================================
    // Di chuyển theo số xúc xắc
    //
    // Flow
    // Đi từng ô
    // ↓
    // Quay mặt
    // ↓
    // Nếu có Dice Bonus
    //      ↓
    //      MoveBonus()
    //
    // Nếu không
    //      ↓
    //      CheckCurrentTile()
    // ↓
    // Kết thúc lượt
    //==================================================
    public IEnumerator AIToPoint(int value)
    {
        isMoving = true;

        // Tốc độ di chuyển bình thường
        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        // Đi từng ô
        for (int i = 0; i <= value; i++)
        {
            // Phát âm thanh bước chân
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);

            // Sang ô tiếp theo
            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            // Offset để Player1 và Player2 không đứng chồng nhau
            Vector3 offset = manager.playerType.isPlayer2
                ? new Vector3(0, 0, -0.3f)
                : new Vector3(0, 0, 0.3f);

            // Thực hiện Jump sang Point tiếp theo
            yield return StartCoroutine(JumpTo(target.transform.position + offset));
        }

        yield return new WaitForSeconds(0.5f);

        // Quay mặt về Point kế tiếp
        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt(pointCheck[currentIndex + 1].transform.position);
        }

        // Nếu còn Dice Bonus thì đi tiếp
        if (manager.playerBuff.isBuffDice > 0)
        {
            manager.playerBuff.isBuffDice--;

            yield return StartCoroutine(MoveBonus());
        }
        else
        {
            // Không có Bonus thì kiểm tra Bomb/Coin
            yield return StartCoroutine(CheckCurrentTile());

            // Camera Follow
            manager.playerCamera.isFllow2 = false;
            manager.playerCamera.isFllow3 = true;

            yield return new WaitForSeconds(1f);

            manager.playerCamera.isFllow3 = false;

            // Kết thúc lượt
            isMoving = false;

            if (!manager.playerRound.isRound1)
            {
                manager.playerRound.isRound1 = true;
            }

            if (!manager.playerRound.nextRound)
            {
                manager.playerRound.nextRound = true;
            }
        }
    }

    //==================================================
    // Dice Bonus
    //
    // Đi thêm 2 ô
    // Sau đó tiếp tục kiểm tra Bomb/Coin
    //==================================================
    public IEnumerator MoveBonus()
    {
        // Ẩn UI Bonus
        StartCoroutine(UIManager.Instance.HideBonusPanel());

        yield return new WaitForSeconds(1f);

        // Đi thêm 2 ô
        for (int i = 0; i < 2; i++)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walkPlayerClip);

            currentIndex++;

            GameObject target = pointCheck[currentIndex];

            Vector3 offset = manager.playerType.isPlayer2
                ? new Vector3(0, 0, -0.3f)
                : new Vector3(0, 0, 0.3f);

            yield return StartCoroutine(JumpTo(target.transform.position + offset));

            yield return new WaitForSeconds(0.3f);
        }

        // Quay mặt về Point kế tiếp
        if (currentIndex + 1 < pointCheck.Count)
        {
            transform.LookAt(pointCheck[currentIndex + 1].transform.position);
        }
        // Kiểm tra ô vừa đến
        yield return StartCoroutine(CheckCurrentTile());
        // Camera Follow Bonus
        manager.playerCamera.isFllow2 = false;
        manager.playerCamera.isFllow3 = true;

        yield return new WaitForSeconds(1f);


        manager.playerCamera.isFllow3 = false;



        // Kết thúc lượt
        isMoving = false;

        if (!manager.playerRound.isRound1)
        {
            manager.playerRound.isRound1 = true;
        }

        if (!manager.playerRound.nextRound)
        {
            manager.playerRound.nextRound = true;
        }
    }

    #endregion

    #region Tile Check

    //==================================================
    // Kiểm tra ô hiện tại
    //
    // Nếu có Bomb
    //      ↓
    //      Kích hoạt Bomb
    //
    // Nếu có Coin
    //      ↓
    //      Kích hoạt Coin
    //==================================================
    IEnumerator CheckCurrentTile()
    {
        TrapAndCoin trap = pointCheck[currentIndex].GetComponentInChildren<TrapAndCoin>();

        if (trap == null)
            yield break;

        //==========================
        // Bomb
        //==========================
        if (trap.hasBom)
        {
            trap.BomActivated();

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(BoomHitEffect(3));

            yield break;
        }

        //==========================
        // Coin
        //==========================
        if (trap.hasCoin)
        {
            trap.CoinActivated(manager.playerType.isPlayer2 ? 1 : 0);
            manager.playerCoin.coinEndMiniGame += 100;

            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion

    #region Effects

    //==================================================
    // Hiệu ứng Bomb
    //
    // Player bị đẩy lùi power ô
    //==================================================
    public IEnumerator BoomHitEffect(int power)
    {
        isMoving = true;

        // Animation Jump
        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        // Lùi lại power ô
        currentIndex = Mathf.Max(0, currentIndex - power);

        GameObject targetPoint = pointCheck[currentIndex];

        Vector3 offset = manager.playerType.isPlayer2
            ? new Vector3(0, 0, -0.3f)
            : new Vector3(0, 0, 0.3f);

        Vector3 finalPos = targetPoint.transform.position + offset;

        // Dash nhanh về vị trí mới
        navMeshAgent.speed = 25f;
        navMeshAgent.acceleration = 999f;

        navMeshAgent.SetDestination(finalPos);

        while (navMeshAgent.pathPending ||
               navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Trả tốc độ về bình thường
        navMeshAgent.speed = 4f;
        navMeshAgent.acceleration = 8f;

        navMeshAgent.ResetPath();
        navMeshAgent.Warp(transform.position);
        yield return StartCoroutine(CheckCurrentTile());
        isMoving = false;
    }

    //==================================================
    // Jump giữa hai Point
    //
    // Tắt NavMesh
    // Chạy Animation Jump
    // Bay theo quỹ đạo Parabol
    // Bật lại NavMesh
    //==================================================
    IEnumerator JumpTo(Vector3 targetPos)
    {
        // Tắt NavMesh để tự điều khiển transform
        navMeshAgent.enabled = false;

        Vector3 startPos = transform.position;

        // Animation Jump
        manager.playerAnimator.playerAnimator.SetTrigger("Jump");

        // Chờ Jump bắt đầu
        while (!manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        float duration = 0.4f;
        float height = 0.8f;
        float t = 0;

        // Bay theo đường Parabol
        while (t < duration)
        {
            t += Time.deltaTime;

            float percent = t / duration;

            Vector3 pos = Vector3.Lerp(startPos, targetPos, percent);

            pos.y += Mathf.Sin(percent * Mathf.PI) * height;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        // Chờ Animation Jump kết thúc
        while (manager.playerAnimator.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Bật lại NavMesh
        navMeshAgent.enabled = true;
        navMeshAgent.Warp(targetPos);
    }

    #endregion

    #region Utility

    //==================================================
    // Tìm tất cả Point trên bàn cờ
    // Point 1 -> Point 34
    //==================================================
    public void FindPonit()
    {
        for (int i = 0; i < 34; i++)
        {
            pointCheck[i] = GameObject.Find($"Point {i + 1}");
        }
    }

    #endregion
}