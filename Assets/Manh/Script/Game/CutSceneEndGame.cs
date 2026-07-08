using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CutSceneEndGame : MonoBehaviour
{
    [Header("Teleport")]
    public GameObject teleport;

    [Header("Player")]
    public GameObject player;
    public Transform[] transPlayerToWalk;

    [Header("Camera Cut Scene Points")]
    public Transform[] transCameraCutSceneList;

    [Header("Settings")]
    public float teleportOpenTime = 0.5f;
    public float waitTime = 0.5f;
    public float cameraMoveTime = 1f;

    [Header("Player Move")]
    public float playerMoveSpeed = 1f;
    public float playerSlowMoveSpeed = 0.4f;

    private Vector3 teleportOriginalScale;
    public GameObject blackPanel;

    private void Awake()
    {
        FindPlayerWinner();
        if (teleport != null)
        {
            teleportOriginalScale = teleport.transform.localScale;
            teleport.transform.localScale = Vector3.zero;
        }
      
    }
    private void Start()
    {
        PlayCutScene();
    }
    public void PlayCutScene()
    {
        StartCoroutine(CutSceneRoutine());
    }

    private IEnumerator CutSceneRoutine()
    {
        if (player == null || teleport == null)
            yield break;
        // StartCoroutine(ShowBlackPanelRoutine());
       AudioManager audio = AudioManager.Instance;
        if(audio != null)
        {
            audio.PlayEnvironment(audio.javaLoopClip);
        }
       
        
        //==============================
        // 1. Mở cổng từ từ
        //==============================
       if(audio != null)
        {
            audio.PlaySFX(audio.openTeleportClip);
        }
        yield return StartCoroutine(ScaleTeleport(teleportOriginalScale, teleportOpenTime));

        yield return new WaitForSeconds(waitTime);

        //==============================
        // 2. Player đi tới Walk 0
        //==============================
        if (transPlayerToWalk.Length > 0 && transPlayerToWalk[0] != null)
        {
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();

            if (playerVFX != null)
            {
                StartCoroutine(playerVFX.DissolveInNoParticleRoutine());
            }

            // Player bắt đầu đi
            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(player, transPlayerToWalk[0].position, playerMoveSpeed)
            );

            // Sau 1 giây thì đóng cổng
            yield return new WaitForSeconds(3f);

           if(audio != null)
            {
                audio.PlaySFX(audio.closeTeleportClip);
            }
            yield return StartCoroutine(
                ScaleTeleport(Vector3.zero, 1)
            );

            // Đợi player đi xong
            yield return playerMoveRoutine;
        }

        //==============================
        // 3. Camera cut 0 -> 16
        //==============================

        // Dịch chuyển cam đến cut 0
        SetCameraToPoint(0);

        // Mượt đến cut 1
        yield return StartCoroutine(MoveCameraToPoint(1));
       // yield return new WaitForSeconds(waitTime);

        // Mượt đến cut 2
        yield return StartCoroutine(MoveCameraToPoint(2));
       // yield return new WaitForSeconds(waitTime);

        // Mượt đến cut 3
        yield return StartCoroutine(MoveCameraToPoint(3));
       // yield return new WaitForSeconds(0.2f);

        // Dịch chuyển cam đến cut 4
        SetCameraToPoint(4);

        // Mượt đến cut 5
        yield return StartCoroutine(MoveCameraToPoint(5));
        yield return new WaitForSeconds(waitTime);

        // Dịch chuyển cam đến cut 6
        SetCameraToPoint(6);

        // Mượt đến cut 7
        yield return StartCoroutine(MoveCameraToPoint(7));
        //yield return new WaitForSeconds(0.2f );

        // Dịch chuyển cam đến cut 8
        SetCameraToPoint(8);

        // Mượt đến cut 9
        yield return StartCoroutine(MoveCameraToPoint(9));
        //yield return new WaitForSeconds(waitTime);

        // Dịch chuyển cam đến cut 10
        SetCameraToPoint(10);

        yield return new WaitForSeconds(waitTime);

        // Mượt đến cut 11
        yield return StartCoroutine(MoveCameraToPoint(11));
        yield return new WaitForSeconds(waitTime);

        // Dịch chuyển cam đến cut 12
        SetCameraToPoint(12);

        //==============================
        // 4. Player đi chậm tới Walk 1
        //    Camera đi 12 -> 13 xong nhảy 14 -> đi 15 luôn
        //==============================
        if (transPlayerToWalk.Length > 1 && transPlayerToWalk[1] != null)
        {
            Coroutine playerMoveRoutine = StartCoroutine(
                MovePlayerToPoint(player, transPlayerToWalk[1].position, playerSlowMoveSpeed)
            );

            // Camera 12 -> 13
            yield return StartCoroutine(MoveCameraToPoint(13));

            // Sau khi tới 13 thì lập tức dịch chuyển cam tới 14
            SetCameraToPoint(14);

            // Rồi mượt tới 15 luôn
            yield return StartCoroutine(MoveCameraToPoint(15));
            yield return new WaitForSeconds(0.2f);

            // Dịch chuyển cam đến cut 16
            SetCameraToPoint(16);

            // Đợi player đi xong nếu player chưa tới
            yield return playerMoveRoutine;
        }
        else
        {
            yield return StartCoroutine(MoveCameraToPoint(13));

            SetCameraToPoint(14);

            yield return StartCoroutine(MoveCameraToPoint(15));
        }

     
    }

    //==================================================
    // TELEPORT SCALE
    //==================================================
    private IEnumerator ScaleTeleport(Vector3 targetScale, float duration)
    {
        Vector3 startScale = teleport.transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            teleport.transform.localScale =
                Vector3.Lerp(startScale, targetScale, t / duration);

            yield return null;
        }

        teleport.transform.localScale = targetScale;
    }

    //==================================================
    // PLAYER MOVE
    //==================================================
    private IEnumerator MovePlayerToPoint(GameObject playerObj, Vector3 targetPos, float speed)
    {
        if (playerObj == null)
            yield break;

        PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();
        NavMeshAgent agent = playerObj.GetComponent<NavMeshAgent>();

        // Giữ nguyên Y của player
        targetPos.y = playerObj.transform.position.y;

        SetPlayerWalk(playerManager, 1f);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(targetPos);

            while (agent.pathPending ||
                   agent.remainingDistance > agent.stoppingDistance)
            {
                SetPlayerWalk(playerManager, agent.velocity.magnitude);
                yield return null;
            }

            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            while (Vector3.Distance(playerObj.transform.position, targetPos) > 0.05f)
            {
                Vector3 dir = targetPos - playerObj.transform.position;
                dir.y = 0f;

                if (dir != Vector3.zero)
                {
                    dir.Normalize();

                    playerObj.transform.position += dir * speed * Time.deltaTime;

                    Quaternion targetRot = Quaternion.LookRotation(dir);

                    playerObj.transform.rotation = Quaternion.Slerp(
                        playerObj.transform.rotation,
                        targetRot,
                        Time.deltaTime * 5f
                    );
                }

                yield return null;
            }

            playerObj.transform.position = new Vector3(
                targetPos.x,
                playerObj.transform.position.y,
                targetPos.z
            );
        }

        SetPlayerWalk(playerManager, 0f);
    }

    private void SetPlayerWalk(PlayerManager playerManager, float value)
    {
        if (playerManager != null &&
            playerManager.playerAnimator != null &&
            playerManager.playerAnimator.playerAnimator != null)
        {
            playerManager.playerAnimator.playerAnimator.SetFloat("Walk", value);
        }
    }

    //==================================================
    // CAMERA
    //==================================================
    private void SetCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index))
            return;
        StartCoroutine(ShowBlackPanelRoutine());
        Camera.main.transform.position =
            transCameraCutSceneList[index].position;

        Camera.main.transform.rotation =
            transCameraCutSceneList[index].rotation;
    }

    private IEnumerator MoveCameraToPoint(int index)
    {
        if (!IsValidCameraPoint(index))
            yield break;

        Transform cam = Camera.main.transform;
        Transform target = transCameraCutSceneList[index];

        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        float t = 0f;

        while (t < cameraMoveTime)
        {
            t += Time.deltaTime;
            float lerp = t / cameraMoveTime;

            cam.position = Vector3.Lerp(startPos, target.position, lerp);
            cam.rotation = Quaternion.Slerp(startRot, target.rotation, lerp);

            yield return null;
        }

        cam.position = target.position;
        cam.rotation = target.rotation;
    }

    private bool IsValidCameraPoint(int index)
    {
        return transCameraCutSceneList != null &&
               index >= 0 &&
               index < transCameraCutSceneList.Length &&
               transCameraCutSceneList[index] != null &&
               Camera.main != null;
    }
    //==================================================
    // BLACK PANEL
    //==================================================
    private IEnumerator ShowBlackPanelRoutine()
    {
        if (blackPanel == null)
            yield break;

        blackPanel.SetActive(true);

        // Thời gian bằng độ dài animation
        yield return new WaitForSeconds(1.6f);

        blackPanel.SetActive(false);
    }
    public void FindPlayerWinner()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
}