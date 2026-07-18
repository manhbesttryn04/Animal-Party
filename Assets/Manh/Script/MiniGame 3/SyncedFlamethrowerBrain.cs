using UnityEngine;
using System.Collections;

public class SyncedFlamethrowerBrain : MonoBehaviour
{
    [Header("--- Trạm Điều Khiển Bẫy ---")]
    public WallFlamethrowerCore leftCore;
    public Transform leftTrap; 
    public WallFlamethrowerCore rightCore;
    public Transform rightTrap;

    [Header("--- Nhịp Độ Chung ---")]
    public float idleTimeMin = 2f;
    public float idleTimeMax = 4f;

    [Header("--- Cảnh Báo (Telegraph) ---")]
    public float telegraphDuration = 1f;
    public float shakeIntensity = 5f;

    [Header("--- Cơ chế 1: Burst ---")]
    public float burstDuration = 1.5f; 
    public float burstPause = 1f;

    [Header("--- Cơ chế 2: Sweep ---")]
    public float sweepDuration = 4f;
    public float sweepAngle = 45f;
    public float rpm = 30f;
    public bool mirrorSweep = true;

    [Header("--- Cơ chế 3: Tracking ---")]
    public float detectionRadius = 50f;
    public float trackingSpeed = 5f;
    public float trackingFireDuration = 3.5f; 
    public LayerMask playerLayer;

    [Header("--- Debug / Test ---")]
    public int debugForceMode = -1;

    private Quaternion leftInitialRot;
    private Quaternion rightInitialRot;
    private Coroutine brainCoroutine;
    private bool trapRunning;
    private void Awake()
    {
        if (leftTrap != null) leftInitialRot = leftTrap.localRotation;
        if (rightTrap != null) rightInitialRot = rightTrap.localRotation;
    }

    private void OnEnable()
    {
        SetTrapRunning(false);
    }

    private void OnDisable()
    {
        SetTrapRunning(false);
    }
    public void SetTrapRunning(bool state)
    {
        if (state)
        {
            if (trapRunning)
                return;

            trapRunning = true;

            SetFireState(false);
            ResetRotations();

            if (brainCoroutine != null)
                StopCoroutine(brainCoroutine);

            brainCoroutine = StartCoroutine(TrapRoutine());
        }
        else
        {
            trapRunning = false;

            if (brainCoroutine != null)
            {
                StopCoroutine(brainCoroutine);
                brainCoroutine = null;
            }

            // Chỉ tắt Particle và Collider lửa
            SetFireState(false);
            ResetRotations();
        }
    }

    private void SetFireState(bool state)
    {
        if (leftCore != null) leftCore.SyncFlamethrowerState(state);
        if (rightCore != null) rightCore.SyncFlamethrowerState(state);
    }

    private void ResetRotations()
    {
        if (leftTrap != null) leftTrap.localRotation = leftInitialRot;
        if (rightTrap != null) rightTrap.localRotation = rightInitialRot;
    }

    private IEnumerator TrapRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(idleTimeMin, idleTimeMax));
            yield return StartCoroutine(TelegraphRoutine());

            int mode = debugForceMode != -1 ? debugForceMode : Random.Range(0, 3);
            
            if (mode == 0) yield return StartCoroutine(BurstModeRoutine());
            else if (mode == 1) yield return StartCoroutine(SweepModeRoutine());
            else yield return StartCoroutine(TrackingModeRoutine());

            SetFireState(false);
            ResetRotations();
        }
    }

    private IEnumerator TelegraphRoutine()
    {
        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(Time.time * 50f) * shakeIntensity;
            
            if (leftTrap != null) leftTrap.localRotation = leftInitialRot * Quaternion.Euler(0, shake, 0);
            if (rightTrap != null) rightTrap.localRotation = rightInitialRot * Quaternion.Euler(0, shake, 0);
            
            yield return null;
        }
        ResetRotations();
    }

    private IEnumerator BurstModeRoutine()
    {
        int bursts = Random.Range(1, 3);
        for (int i = 0; i < bursts; i++)
        {
            SetFireState(true);
            yield return new WaitForSeconds(burstDuration);
            SetFireState(false);
            yield return new WaitForSeconds(burstPause);
        }
    }

    private IEnumerator SweepModeRoutine()
    {
        SetFireState(true);
        float elapsed = 0f;
        float angularSpeed = (rpm * Mathf.PI * 2f) / 60f;

        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            float baseAngle = Mathf.Sin(elapsed * angularSpeed) * sweepAngle;

            if (leftTrap != null) 
                leftTrap.localRotation = leftInitialRot * Quaternion.Euler(0, baseAngle, 0);
            
            if (rightTrap != null) 
            {
                float rightAngle = mirrorSweep ? -baseAngle : baseAngle;
                rightTrap.localRotation = rightInitialRot * Quaternion.Euler(0, rightAngle, 0);
            }
            yield return null;
        }
    }

    private IEnumerator TrackingModeRoutine()
    {
        Transform leftTarget = FindNearestPlayer(leftTrap.position);
        Transform rightTarget = FindNearestPlayer(rightTrap.position);

        float aimTime = 1.5f;
        float elapsedAim = 0f;

        while (elapsedAim < aimTime)
        {
            elapsedAim += Time.deltaTime;

            if (leftTarget != null && leftTrap != null)
            {
                Vector3 dir = (leftTarget.position - leftTrap.position).normalized;
                dir.y = 0;
                leftTrap.rotation = Quaternion.Slerp(leftTrap.rotation, Quaternion.LookRotation(dir), Time.deltaTime * trackingSpeed);
            }

            if (rightTarget != null && rightTrap != null)
            {
                Vector3 dir = (rightTarget.position - rightTrap.position).normalized;
                dir.y = 0;
                rightTrap.rotation = Quaternion.Slerp(rightTrap.rotation, Quaternion.LookRotation(dir), Time.deltaTime * trackingSpeed);
            }
            yield return null;
        }

        SetFireState(true);
        yield return new WaitForSeconds(trackingFireDuration); 
    }

    private Transform FindNearestPlayer(Vector3 origin)
    {
        Collider[] hits = Physics.OverlapSphere(origin, detectionRadius, playerLayer);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(origin, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }
}