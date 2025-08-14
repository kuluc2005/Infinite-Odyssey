// FlyingDragonBossManager.cs
using UnityEngine;
using Invector;
using System.Collections;

public class FlyingDragonBossManager : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballCooldown = 6f;

    [Header("Fire Breath Settings")]
    public GameObject fireBreathEffectPrefab;
    public Transform fireBreathSpawnPoint;
    public float fireBreathDuration = 3f;
    public float fireBreathCooldown = 15f;

    [Header("Fire Zone Settings")]
    public GameObject fireZonePrefab;
    public LayerMask groundLayer;
    public float fireSpawnInterval = 0.5f;

    [Header("Wake Zone Settings")]
    public float wakeUpDistance = 10f;

    [Header("Win Settings")]
    [Range(0f, 5f)] public float winDelay = 0f;

    private Animator animator;
    private vHealthController health;
    private Transform player;
    private bool isDead = false;

    private float fireballTimer = 0f;
    private float breathTimer = 0f;

    private bool isPlayerInZone = false;
    private bool hasTakenOff = false;
    private bool isUsingBreath = false;
    private Coroutine returnToSleepCoroutine;

    private bool winTriggered = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<vHealthController>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator.Play("Sleep");
        Debug.Log("Boss khởi động ở trạng thái Sleep.");
    }

    void Update()
    {
        if (!isDead && health != null && health.currentHealth <= 0)
        {
            HandleDeath();
            return;
        }

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found) player = found.transform;
            else return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isPlayerInZone && distanceToPlayer <= wakeUpDistance)
        {
            SetPlayerInRange(true);
        }
        else if (isPlayerInZone && distanceToPlayer > wakeUpDistance)
        {
            SetPlayerInRange(false);
        }

        if (!hasTakenOff || health == null || !isPlayerInZone)
            return;

        RotateTowardsPlayer();

        if (isUsingBreath) return;

        HandleFireBreath();
        HandleFireball();
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        isUsingBreath = false;

        animator.SetTrigger("IsDead");
        Debug.Log("Boss đã chết!");
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (!winTriggered)
        {
            winTriggered = true;
            StartCoroutine(WinAfterDelay());
        }

        Destroy(gameObject, 3f);
    }

    IEnumerator WinAfterDelay()
    {
        float t = Mathf.Max(winDelay, 0f); 
        if (t > 0f) yield return new WaitForSeconds(t);
        GameFlowManager.Win();
    }


    void RotateTowardsPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * 2f);
    }

    void HandleFireball()
    {
        fireballTimer += Time.deltaTime;
        if (fireballTimer >= fireballCooldown)
        {
            fireballTimer = 0f;
            SpawnFireball();
        }
    }

    void SpawnFireball()
    {
        if (!fireballPrefab || !fireballSpawnPoint) return;

        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = fireball.GetComponent<Rigidbody>();

        if (rb && player)
        {
            Vector3 dir = (player.position - fireballSpawnPoint.position).normalized;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * 3f;
#else
            rb.velocity = dir * 3f;
#endif
        }

        Debug.Log("Boss bắn Fireball!");
    }

    void HandleFireBreath()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= fireBreathCooldown)
        {
            breathTimer = 0f;
            isUsingBreath = true;
            animator.SetTrigger("TriggerBreath");
        }
    }

    public void TriggerFireBreathEffect()
    {
        Debug.Log("Animation event gọi TriggerFireBreathEffect()");
        StartCoroutine(UseFireBreath());
    }

    private IEnumerator UseFireBreath()
    {
        Debug.Log("Coroutine UseFireBreath bắt đầu");
        isUsingBreath = true;

        yield return new WaitForSeconds(0.5f);

        if (fireBreathEffectPrefab && fireBreathSpawnPoint)
        {
            GameObject breath = Instantiate(fireBreathEffectPrefab, fireBreathSpawnPoint.position, fireBreathSpawnPoint.rotation);
            var follow = breath.GetComponent<FollowSpawnPoint>();
            if (follow != null) follow.target = fireBreathSpawnPoint;

            Destroy(breath, fireBreathDuration);

            float elapsed = 0f;
            while (elapsed < fireBreathDuration)
            {
                RaycastHit hit;
                if (Physics.Raycast(fireBreathSpawnPoint.position, Vector3.down, out hit, 10f, groundLayer))
                {
                    Vector3 spawnPos = hit.point + transform.forward * 7f;
                    Instantiate(fireZonePrefab, spawnPos, Quaternion.identity);
                }

                yield return new WaitForSeconds(fireSpawnInterval);
                elapsed += fireSpawnInterval;
            }

            isUsingBreath = false;
        }

        yield return new WaitForSeconds(fireBreathDuration);
        isUsingBreath = false;
    }

    public void SetPlayerInRange(bool inZone)
    {
        isPlayerInZone = inZone;
        Debug.Log("Player trong vùng: " + inZone);

        if (inZone)
        {
            if (returnToSleepCoroutine != null)
            {
                StopCoroutine(returnToSleepCoroutine);
                returnToSleepCoroutine = null;
                Debug.Log("Đã hủy coroutine ngủ vì Player quay lại vùng.");
            }

            if (!hasTakenOff)
            {
                animator.SetTrigger("TriggerTakeOff");
                Invoke(nameof(EnableCombat), 3f);
                hasTakenOff = true;
                Debug.Log("Boss bắt đầu combat");
            }
        }
        else
        {
            if (returnToSleepCoroutine == null)
            {
                returnToSleepCoroutine = StartCoroutine(ReturnToSleepAfterDelay(5f));
                Debug.Log("Bắt đầu đếm để trở về trạng thái ngủ.");
            }
        }
    }

    void EnableCombat()
    {
        Debug.Log("Boss sẵn sàng chiến đấu.");
    }

    IEnumerator ReturnToSleepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isPlayerInZone && hasTakenOff)
        {
            Debug.Log("Boss trở lại Sleep vì Player đã rời vùng.");

            hasTakenOff = false;
            isUsingBreath = false;
            fireballTimer = 0f;
            breathTimer = 0f;
            animator.Play("Sleep");
        }

        returnToSleepCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, wakeUpDistance);
    }
}
