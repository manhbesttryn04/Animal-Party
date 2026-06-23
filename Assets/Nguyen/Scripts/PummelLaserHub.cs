using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AnimalParty.Player;
using AnimalParty.Audio;

[RequireComponent(typeof(Collider))]
public class PummelLaserHub : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float depth = 5f;
    public float startDelay = 3f;

    public float minSpeed = 50f;
    public float maxSpeed = 150f;
    public float acceleration = 250f;
    public bool useProgression = true;
    public float speedIncreasePerSecond = 1.5f;
    public float absoluteMaxSpeed = 300f;

    public float minSpinTime = 2f;
    public float maxSpinTime = 4f;
    public float minPauseTime = 1f;
    public float maxPauseTime = 1.5f;

    [Range(0f, 1f)] public float fakeOutChance = 0.4f;
    public float fakeOutPauseTime = 0.4f;
    public float minFakeOutSpinTime = 2f;
    public float maxFakeOutSpinTime = 3f;

    public float knockbackForce = 15f;

    public List<AutoFitLaser> laserBeams = new List<AutoFitLaser>();

    private bool isReady = false;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float currentDirection = 1f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float originalMaxSpeed;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalMaxSpeed = maxSpeed;
    }

    private void Start()
    {
        ResetHub();
    }

    public void ResetHub()
    {
        StopAllCoroutines();

        isReady = false;
        currentSpeed = 0f;
        targetSpeed = 0f;
        currentDirection = 1f;
        maxSpeed = originalMaxSpeed;

        transform.position = originalPosition - new Vector3(0, depth, 0);
        transform.rotation = originalRotation;

        foreach (var laser in laserBeams)
        {
            if (laser != null)
                laser.SetLaserActive(false);
        }

        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        Vector3 targetPos = originalPosition;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(startDelay);

        isReady = true;

        foreach (var laser in laserBeams)
        {
            if (laser != null)
                laser.SetLaserActive(true);
        }
        MiniGameAudioManager.Instance.StartLaserLoop();
        StartCoroutine(VIPPatternRoutine());
    }

    private void Update()
    {
        if (!isReady) return;

        if (useProgression)
        {
            maxSpeed += speedIncreasePerSecond * Time.deltaTime;
            maxSpeed = Mathf.Min(maxSpeed, absoluteMaxSpeed);
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        transform.Rotate(
            Vector3.up * currentSpeed * currentDirection * Time.deltaTime
        );
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

    public void EndMinigameAndSink()
    {
        if (!isReady) return;

        isReady = false;
        StopAllCoroutines();

        StartCoroutine(SinkRoutine());
    }

    private IEnumerator SinkRoutine()
    {
        foreach (var laser in laserBeams)
        {
            if (laser != null)
                laser.SetLaserActive(false);
        }

        float duration = 1.5f;
        Vector3 startPos = transform.position;
        Vector3 endPos = originalPosition - new Vector3(0, depth, 0);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        gameObject.SetActive(false);
    }
}