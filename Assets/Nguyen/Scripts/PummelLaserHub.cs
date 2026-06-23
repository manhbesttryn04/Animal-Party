using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AnimalParty.Player;
using AnimalParty.Audio; 

[RequireComponent(typeof(Collider))]
public class PummelLaserHub : MonoBehaviour
{
    [Header("--- Movement Settings ---")]
    public float moveSpeed = 3f;
    public float depth = 5f;

    [Header("--- Delay Settings ---")]
    [Tooltip("Thời gian chờ (giây) sau khi trồi lên hẳn rồi mới kích hoạt laser và quay")]
    public float startDelay = 3f; // Bạn có thể chỉnh thành 3, 4 hoặc 5 tùy ý trên Inspector

    [Header("--- Speed Settings ---")]
    public float minSpeed = 50f;
    public float maxSpeed = 150f;
    public float acceleration = 250f;
    public bool useProgression = true;
    public float speedIncreasePerSecond = 1.5f;
    public float absoluteMaxSpeed = 300f;

    [Header("--- Timing & Pattern Settings ---")]
    [Tooltip("Thời gian quay bình thường (Tối thiểu - Tối đa)")]
    public float minSpinTime = 2f;
    public float maxSpinTime = 4f;
    
    [Tooltip("Thời gian dừng nghỉ giữa các lần quay")]
    public float minPauseTime = 1f;
    public float maxPauseTime = 1.5f;

    [Header("--- Fake-out Settings ---")]
    [Tooltip("Tỉ lệ trụ sẽ giật ngược lại để lừa người chơi (0.4 = 40%)")]
    [Range(0f, 1f)] public float fakeOutChance = 0.4f;
    public float fakeOutPauseTime = 0.4f;
    public float minFakeOutSpinTime = 2f;
    public float maxFakeOutSpinTime = 3f;

    [Header("--- Collision Settings ---")]
    [Tooltip("Lực hất văng khi người chơi chạm vào tia lazer")]
    public float knockbackForce = 15f;

    [Header("--- Laser Configuration ---")]
    public List<AutoFitLaser> laserBeams = new List<AutoFitLaser>();

    private bool isReady = false;
    private bool isSinkingComplete = false;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float currentDirection = 1f;

    void Start()
    {
        transform.position -= new Vector3(0, depth, 0);
        StartCoroutine(RiseRoutine());
    }

    public void SetDifficultyParams(float newMinSpeed, float newMaxSpeed, float newMinPause, float newMaxPause)
    {
        minSpeed = newMinSpeed;
        maxSpeed = newMaxSpeed;
        minPauseTime = newMinPause;
        maxPauseTime = newMaxPause;
        Debug.Log($"[{gameObject.name}] Đã cập nhật độ khó Lazer Hub!");
    }

    private IEnumerator RiseRoutine()
    {
        // 1. Giai đoạn đi lên
        Vector3 targetPos = transform.position + new Vector3(0, depth, 0);
        while (transform.position.y < targetPos.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // --- ĐOẠN ĐƯỢC THÊM: ĐỨNG IM CHỜ NGƯỜI CHƠI CHUẨN BỊ ---
        yield return new WaitForSeconds(startDelay);

        isReady = true;

        // 2. Bật tia laser lên sau khi hết thời gian chờ
        foreach (var laser in laserBeams)
        {
            if (laser != null) laser.SetLaserActive(true);
        }

        // 3. Bắt đầu quay vòng tròn
        StartCoroutine(VIPPatternRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndMinigameAndSink();
        }

        if (!isReady) return;

        if (useProgression)
        {
            maxSpeed += speedIncreasePerSecond * Time.deltaTime;
            maxSpeed = Mathf.Min(maxSpeed, absoluteMaxSpeed);
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        transform.Rotate(Vector3.up * currentSpeed * currentDirection * Time.deltaTime);
    }

    private IEnumerator VIPPatternRoutine()
    {
        while (isReady)
        {
            currentDirection = Random.value > 0.5f ? 1f : -1f;
            targetSpeed = Random.Range(minSpeed, maxSpeed * 0.6f);
            yield return new WaitForSeconds(Random.Range(minSpinTime, maxSpinTime));

            targetSpeed = 0f;
            yield return new WaitForSeconds(Random.Range(minPauseTime, maxPauseTime));

            if (Random.value <= fakeOutChance)
            {
                targetSpeed = 40f; 
                yield return new WaitForSeconds(fakeOutPauseTime);
                
                currentDirection *= -1f; 
                targetSpeed = maxSpeed; 
                yield return new WaitForSeconds(Random.Range(minFakeOutSpinTime, maxFakeOutSpinTime));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isReady) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerHealth healthScript))
            {
                healthScript.TakeDamage();
            }

            if (other.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.3f, 0f, rb.linearVelocity.z * 0.3f); 
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                pushDirection.y = 1.2f; 
                rb.AddForce(pushDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    public void EndMinigameAndSink()
    {
        if (!isReady) return;
        isReady = false;
        
        StartCoroutine(SinkRoutine());
    }

    private IEnumerator SinkRoutine()
    {
        float duration = 1.5f; 
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0, depth, 0);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            transform.Rotate(Vector3.up * currentSpeed * currentDirection * Time.deltaTime);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        foreach (var laser in laserBeams)
        {
            if (laser != null) laser.SetLaserActive(false);
        }
        
        isSinkingComplete = true; 
        gameObject.SetActive(false);
    }
}