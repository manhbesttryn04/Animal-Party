using AnimalParty.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalParty.Obstacles
{
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class LaserTrap : MonoBehaviour
    {
        [Header("--- Target Detection ---")]
        [SerializeField] private LayerMask targetLayer;

        [Header("--- Wall Raycast ---")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LineRenderer laserLine;
        [SerializeField] private ParticleSystem leftWallImpact;
        [SerializeField] private ParticleSystem rightWallImpact;
        [SerializeField] private float impactOffset = 0.05f;

        [Header("--- Trap Settings ---")]
        [SerializeField] private float hitCooldown = 0.5f;
        [SerializeField] private int coinPenalty = 5;
        [SerializeField] private float stunTime = 0.2f;

        [Header("--- Debug ---")]
        [SerializeField] private bool showDebug = true;

        private readonly Dictionary<Collider, float> _lastHitTimes =
            new Dictionary<Collider, float>();

        private Collider _trapCollider;

        private void Awake()
        {
            _trapCollider = GetComponent<Collider>();
            _trapCollider.isTrigger = true;

            HideWallImpact();
        }

        private void Update()
        {
            UpdateWallImpact();
        }

        private void UpdateWallImpact()
        {
            if (laserLine == null || laserLine.positionCount < 2)
            {
                HideWallImpact();
                return;
            }

            // Vì KHÔNG dùng World Space nên phải đổi local -> world
            Vector3 point0 = laserLine.transform.TransformPoint(laserLine.GetPosition(0));
            Vector3 point1 = laserLine.transform.TransformPoint(laserLine.GetPosition(1));

            Vector3 midPoint = Vector3.Lerp(point0, point1, 0.5f);

            float leftDistance = Vector3.Distance(midPoint, point0);
            float rightDistance = Vector3.Distance(midPoint, point1);

            Vector3 dirMidToLeft = (point0 - midPoint).normalized;
            Vector3 dirMidToRight = (point1 - midPoint).normalized;

            Debug.DrawRay(midPoint, dirMidToLeft * leftDistance, Color.red);
            Debug.DrawRay(midPoint, dirMidToRight * rightDistance, Color.blue);

            if (Physics.Raycast(midPoint, dirMidToLeft, out RaycastHit leftHit, leftDistance, wallLayer))
            {
                Debug.Log("LEFT HIT: " + leftHit.collider.name);
                ShowOneImpact(leftWallImpact, leftHit);
            }
            else
            {
                Debug.Log("LEFT KHONG HIT");
                StopOneImpact(leftWallImpact);
            }

            if (Physics.Raycast(midPoint, dirMidToRight, out RaycastHit rightHit, rightDistance, wallLayer))
            {
                Debug.Log("RIGHT HIT: " + rightHit.collider.name);
                ShowOneImpact(rightWallImpact, rightHit);
            }
            else
            {
                Debug.Log("RIGHT KHONG HIT");
                StopOneImpact(rightWallImpact);
            }
        }

        private void ShowOneImpact(ParticleSystem impact, RaycastHit hit)
        {
            if (impact == null)
            {
                if (showDebug) Debug.LogWarning("Impact Particle NULL");
                return;
            }

            impact.gameObject.SetActive(true);

            impact.transform.position =
                hit.point + hit.normal * impactOffset;

            impact.transform.rotation =
                Quaternion.LookRotation(hit.normal);

            if (!impact.isPlaying)
                impact.Play();

            if (showDebug)
                Debug.Log("PLAY VFX AT: " + impact.transform.position);
        }

        private void StopOneImpact(ParticleSystem impact)
        {
            if (impact != null && impact.isPlaying)
                impact.Stop();
        }

        private void HideWallImpact()
        {
            StopOneImpact(leftWallImpact);
            StopOneImpact(rightWallImpact);
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            ProcessHit(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_lastHitTimes.ContainsKey(other))
                _lastHitTimes.Remove(other);
        }

        private void ProcessHit(Collider targetCollider)
        {
            if ((targetLayer.value & (1 << targetCollider.gameObject.layer)) == 0)
                return;

            if (!CanHitTarget(targetCollider))
                return;

            HitPlayer(targetCollider);

            _lastHitTimes[targetCollider] = Time.time;
        }

        private bool CanHitTarget(Collider targetCollider)
        {
            if (_lastHitTimes.TryGetValue(targetCollider, out float lastTime))
                return Time.time - lastTime >= hitCooldown;

            return true;
        }

        private void HitPlayer(Collider targetCollider)
        {
            if (MiniGameAudioManager.Instance != null)
                MiniGameAudioManager.Instance.PlayHitLaserSound();

            PlayerMiniGame miniGame = targetCollider.GetComponent<PlayerMiniGame>();

            if (miniGame != null)
                miniGame.UpCoin(0, coinPenalty);

            PlayerMove move = targetCollider.GetComponent<PlayerMove>();

            if (move != null)
                StartCoroutine(ElectricStun(move));
        }

        private IEnumerator ElectricStun(PlayerMove move)
        {
            move.isMove = false;
            move.isJump = false;

            if (move.manager != null && move.manager.playerAnimator != null)
                move.manager.playerAnimator.playerAnimator.SetTrigger("Jump");

            yield return new WaitForSeconds(stunTime);

            move.isMove = true;
            move.isJump = true;
        }
    }
}