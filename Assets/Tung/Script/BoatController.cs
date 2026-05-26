using UnityEngine;
using System.Collections;

public class BoatController : MonoBehaviour
{
    public enum BoatAxis { Forward, Right }

    [Header("Movement Settings")]
    public float moveSpeed = 100f;
    public float turnSpeed = 60f;
    public BoatAxis moveAxis = BoatAxis.Forward;

    [Header("Orientation Fix (Sửa Ngược Hướng)")]
    public bool invertForwardBackward = false;
    public bool invertLeftRight = false;

    [Header("Anti-Fly Settings (Khóa Độ Cao)")]
    public float fixedWaterHeight = 0.5f;
    public bool lockHeight = true;

    [Header("Mario Kart Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageOnCrash = 20f;
    [Tooltip("Thời gian giãn cách giữa các lần trừ máu khi va chạm liên tiếp (giây)")]
    public float damageInterval = 0.2f;

    [Header("Respawn Settings (Hồi Sinh)")]
    public float respawnDelay = 2.0f;
    public float respawnSafetyDistance = 3.0f;

    private Rigidbody rb;
    private float movementInput;
    private float turnInput;
    private float lastDamageTime;
    private bool isDead = false;
    private Vector3 lastSafePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        currentHealth = maxHealth;
        isDead = false;

        lastSafePosition = transform.position;
    }

    void Update()
    {
        if (isDead) return;

        movementInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude > 2f && !isDead)
        {
            if (Mathf.Abs(transform.position.y - fixedWaterHeight) < 0.2f)
            {
                lastSafePosition = transform.position;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            return;
        }

        MoveBoat();
        TurnBoat();

        if (lockHeight)
        {
            KeepBoatOnWater();
        }
    }

    void MoveBoat()
    {
        Vector3 direction = (moveAxis == BoatAxis.Forward) ? transform.forward : transform.right;

        if (invertForwardBackward)
        {
            direction = -direction;
        }

        Vector3 targetVelocity = direction * movementInput * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVelocity;
    }

    void TurnBoat()
    {
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        if (invertLeftRight)
        {
            turn = -turn;
        }

        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void KeepBoatOnWater()
    {
        if (rb.linearVelocity.y > 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
        }

        if (transform.position.y > fixedWaterHeight)
        {
            Vector3 pos = transform.position;
            pos.y = fixedWaterHeight;
            transform.position = pos;
        }
    }

    // VA CHẠM PHÁT ĐẦU TIÊN
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // LỌC BUG: Chỉ trừ máu khi đâm vào vật thể có TAG là "Map"
        if (collision.gameObject.CompareTag("Map"))
        {
            if (rb.linearVelocity.magnitude > 5f)
            {
                TakeDamage(damageOnCrash);
                lastDamageTime = Time.time;
            }
        }
    }

    // VA CHẠM LIÊN TIẾP (Cọ xát)
    private void OnCollisionStay(Collision collision)
    {
        if (isDead) return;

        // LỌC BUG: Chỉ trừ máu khi cọ xát với vật thể có TAG là "Map"
        if (collision.gameObject.CompareTag("Map"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                if (Mathf.Abs(movementInput) > 0.1f || rb.linearVelocity.magnitude > 2f)
                {
                    TakeDamage(damageOnCrash);
                    lastDamageTime = Time.time;
                }
            }
        }
    }

    void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Thuyền bị đâm! Máu còn lại: " + currentHealth);

        rb.linearVelocity = -rb.linearVelocity * 0.1f;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("THUYỀN BANH XÁC! Chuẩn bị hồi sinh...");
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 spawnPos = lastSafePosition;

        Vector3 moveDir = (moveAxis == BoatAxis.Forward) ? transform.forward : transform.right;
        Vector3 backwardDirection = invertForwardBackward ? moveDir : -moveDir;

        spawnPos += backwardDirection * respawnSafetyDistance;
        spawnPos.y = fixedWaterHeight;

        transform.position = spawnPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        currentHealth = maxHealth;
        isDead = false;

        Debug.Log("ĐÃ HỒI SINH! Đạp ga tiếp đi mày!");
    }
}