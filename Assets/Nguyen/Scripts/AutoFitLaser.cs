using AnimalParty.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AutoFitLaser : MonoBehaviour
{
    public LineRenderer line;

    [Header("--- Cài đặt Laser ---")]
    public float laserWidth = 0.5f;
    public float maxLaserDistance = 50f;
    public float fadeSpeed = 10f;

    [Header("--- Layer ---")]
    public LayerMask obstacleLayer;
    public LayerMask targetLayer;

    [Header("--- Player Hit ---")]
    public int coinPenalty = 5;
    public float hitCooldown = 0.5f;
    public float stunTime = 0.2f;

    [Header("--- VFX chạm tường ---")]
    public ParticleSystem wallImpact;
    public float impactOffset = 0.05f;

    public float currentWidthMultiplier = 0f;
    public float targetWidthMultiplier = 0f;

    private readonly Dictionary<PlayerMove, float> lastHitTimes = new();

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0f;
        line.endWidth = 0f;

        HideWallImpact();
    }

    private void Update()
    {
        currentWidthMultiplier = Mathf.MoveTowards(
            currentWidthMultiplier,
            targetWidthMultiplier,
            fadeSpeed * Time.deltaTime
        );

        bool laserVisible = currentWidthMultiplier > 0.1f;

        float calculatedWidth = laserWidth * currentWidthMultiplier;

        line.startWidth = calculatedWidth;
        line.endWidth = calculatedWidth;

        Vector3 startPoint = transform.position;
        Vector3 direction = transform.forward;
        Vector3 endPoint = startPoint + direction * maxLaserDistance;

        // Line luôn dài full
        line.SetPosition(0, startPoint);
        line.SetPosition(1, endPoint);

        // Raycast chỉ để hiện VFX tại điểm chạm Wall
        if (Physics.Raycast(
            startPoint,
            direction,
            out RaycastHit wallHit,
            maxLaserDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore))
        {
        
            if (laserVisible)
                ShowWallImpact(wallHit);
            else
            {
             
                HideWallImpact();
            }

        }
        else
        {
            HideWallImpact();
        }

        if (laserVisible)
        {
            CheckHitPlayer(calculatedWidth, maxLaserDistance);
        }
    }

    private void ShowWallImpact(RaycastHit wallHit)
    {
        if (wallImpact == null) return;

        wallImpact.transform.position =
            wallHit.point + wallHit.normal * impactOffset;

        wallImpact.transform.rotation =
            Quaternion.LookRotation(wallHit.normal);

        if (!wallImpact.isPlaying)
            wallImpact.Play();
    }

    private void HideWallImpact()
    {
        if (wallImpact == null) return;

        if (wallImpact.isPlaying)
            wallImpact.Stop();
    }

    private void CheckHitPlayer(float calculatedWidth, float laserLength)
    {
        float radius = calculatedWidth / 2f;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            radius,
            transform.forward,
            laserLength,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            PlayerMove move =
                hit.collider.GetComponentInParent<PlayerMove>();

            if (move == null) continue;
            if (!CanHit(move)) continue;

            HitPlayer(move);
            lastHitTimes[move] = Time.time;
        }
    }

    private bool CanHit(PlayerMove move)
    {
        if (lastHitTimes.TryGetValue(move, out float lastTime))
            return Time.time - lastTime >= hitCooldown;

        return true;
    }

    private void HitPlayer(PlayerMove move)
    {
        MiniGameAudioManager.Instance.PlayHitLaserSound();
        PlayerMiniGame mini =
            move.GetComponent<PlayerMiniGame>();

        if (mini != null)
            mini.UpCoin(0, coinPenalty);

        StartCoroutine(ElectricStun(move));
    }

    private IEnumerator ElectricStun(PlayerMove move)
    {
        move.isMove = false;
        move.isJump = false;

        if (move.manager != null &&
            move.manager.playerAnimator != null)
        {
            move.manager.playerAnimator.playerAnimator.SetTrigger("Jump");
        }

        yield return new WaitForSeconds(stunTime);

        move.isMove = true;
        move.isJump = true;
    }

    public void SetLaserActive(bool isActive)
    {
        targetWidthMultiplier = isActive ? 1f : 0f;

        if (!isActive)
            HideWallImpact();
    }
}
