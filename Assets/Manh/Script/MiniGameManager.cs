using System.Collections;
using UnityEngine;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    [Header("Index MiniGame")]
    public int indexMiniGame;

    [Header("Camera")]
    public Camera miniGameCamera;

    [Header("Cutscene")]
    public MiniGameCameraCutscene cutscene;

    [Header("MiniGame Logic")]
    public MiniGame1 miniGame1;

    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Player Prefab")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Timer")]
    public TextMeshProUGUI timerText;

    public float countDownTime = 99f;

    public GameObject currentPlayer1;
    public GameObject currentPlayer2;

    public bool isPlaying = false;

    private void Update()
    {
        StartMiniGame();
    }
    public void StartMiniGame()
    {
        if (isPlaying)
            return;

        StartCoroutine(MiniGameRoutine());
    }

    IEnumerator MiniGameRoutine()
    {
        isPlaying = true;

        // =========================
        // BẬT CAMERA
        // =========================
        miniGameCamera.gameObject.SetActive(true);

        // =========================
        // HIỆN TIMER
        // =========================
        timerText.gameObject.SetActive(true);

        // =========================
        // SPAWN PLAYER
        // =========================
        currentPlayer1 =
            Instantiate(
                player1Prefab,
                spawnPoint.position,
                Quaternion.identity
            );

        currentPlayer2 =
            Instantiate(
                player2Prefab,
                spawnPoint.position + Vector3.right * 2,
                Quaternion.identity
            );

        // =========================
        // CHECKPOINT
        // =========================
        PlayerMiniGame p1 =
            currentPlayer1.GetComponent<PlayerMiniGame>();

        PlayerMiniGame p2 =
            currentPlayer2.GetComponent<PlayerMiniGame>();

        PlayerType player2Type =
            currentPlayer2.GetComponent<PlayerType>();

        player2Type.isPlayer2 = true;

        p1.checkPoint = spawnPoint;
        p2.checkPoint = spawnPoint;

        // =========================
        // PLAY CUTSCENE
        // =========================
        if (cutscene != null)
        {
            yield return StartCoroutine(
                cutscene.PlayCutscene()
            );
        }

        // =========================
        // BẮT ĐẦU MINIGAME
        // =========================
        miniGame1.StartMiniGame();

        // =========================
        // TIMER ĐẾM NGƯỢC
        // =========================
        float timer = countDownTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            int seconds =
                Mathf.CeilToInt(timer);

            int minutes =
                seconds / 60;

            int remainSeconds =
                seconds % 60;

            timerText.text =
                minutes.ToString("00") +
                ":" +
                remainSeconds.ToString("00");

            yield return null;
        }

        // =========================
        // HẾT GIỜ
        // =========================
        timerText.text = "00:00";

        // =========================
        // STOP MINIGAME
        // =========================
        miniGame1.StopMiniGame();

        // =========================
        // TẮT CAMERA
        // =========================
        miniGameCamera.gameObject.SetActive(false);

        // =========================
        // ẨN TIMER
        // =========================
        timerText.gameObject.SetActive(false);

        // =========================
        // XOÁ PLAYER
        // =========================
        Destroy(currentPlayer1);

        Destroy(currentPlayer2);

        isPlaying = false;
    }
}