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

    private Animator animator;
    private vHealthController health;
    private Transform player;

    private float fireballTimer = 0f;
    private float breathTimer = 0f;

    private bool isPlayerInZone = false;
    private bool hasTakenOff = false;
    private bool isUsingBreath = false;
    private Coroutine returnToSleepCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<vHealthController>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator.Play("Sleep"); // Bắt đầu ở trạng thái ngủ
        Debug.Log("💤 Boss khởi động ở trạng thái Sleep.");
    }

    void Update()
    {
        if (!hasTakenOff || health == null || health.currentHealth <= 0 || !isPlayerInZone)
            return;

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found) player = found.transform;
            else return;
        }

        RotateTowardsPlayer();

        if (isUsingBreath) return; // khi đang khè thì không bắn

        HandleFireBreath();
        HandleFireball();
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
            rb.linearVelocity = dir * 3f;
        }

        Debug.Log("🔥 Boss bắn Fireball!");
    }

    void HandleFireBreath()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= fireBreathCooldown)
        {
            breathTimer = 0f;
            StartCoroutine(UseFireBreath());
        }
    }

    IEnumerator UseFireBreath()
    {
        isUsingBreath = true;

        animator.SetTrigger("TriggerBreath");
        yield return new WaitForSeconds(0.5f); // delay đúng khung animation

        if (fireBreathEffectPrefab && fireBreathSpawnPoint)
        {
            GameObject breath = Instantiate(fireBreathEffectPrefab, fireBreathSpawnPoint.position, fireBreathSpawnPoint.rotation, transform);
            Destroy(breath, fireBreathDuration);
        }

        yield return new WaitForSeconds(fireBreathDuration);
        isUsingBreath = false;
    }

    public void SetPlayerInRange(bool inZone)
    {
        isPlayerInZone = inZone;
        Debug.Log("📍 Player trong vùng: " + inZone);

        if (inZone)
        {
            if (returnToSleepCoroutine != null)
            {
                StopCoroutine(returnToSleepCoroutine);
                returnToSleepCoroutine = null;
            }

            if (!hasTakenOff)
            {
                animator.SetTrigger("TriggerTakeOff");
                Invoke(nameof(EnableCombat), 3f);
                hasTakenOff = true;
                Debug.Log("🚀 Boss TakeOff");
            }
        }
        else
        {
            returnToSleepCoroutine = StartCoroutine(ReturnToSleepAfterDelay(5f));
        }
    }

    void EnableCombat()
    {
        Debug.Log("✅ Boss sẵn sàng chiến đấu.");
    }

    IEnumerator ReturnToSleepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isPlayerInZone && hasTakenOff)
        {
            Debug.Log("😴 Boss trở lại Sleep vì Player đã rời vùng.");

            hasTakenOff = false;
            isUsingBreath = false;
            fireballTimer = 0f;
            breathTimer = 0f;
            animator.Play("Sleep");
        }

        returnToSleepCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetPlayerInRange(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetPlayerInRange(false);
        }
    }
    public void SpawnFireBreathEffect()
    {
        if (fireBreathEffectPrefab && fireBreathSpawnPoint)
        {
            GameObject breath = Instantiate(fireBreathEffectPrefab, fireBreathSpawnPoint.position, fireBreathSpawnPoint.rotation, transform);
            Destroy(breath, fireBreathDuration);
        }
    }

}
