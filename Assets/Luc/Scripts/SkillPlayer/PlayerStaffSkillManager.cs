using UnityEngine;
using Invector.vMelee;
public class PlayerStaffSkillManager : MonoBehaviour
{
    public Animator animator;

    [Header("Skill 1 (Alpha2) - Fireball")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 20f;
    public float fireballDamage = 50f;
    public float skill1Cooldown = 2f;
    float skill1CooldownTimer;

    [Header("Skill 2 (Alpha3) - Ice Spikes (AOE)")]
    public GameObject skill2VFXPrefab;
    public Transform skill2VFXSpawnPoint;
    public float skill2Cooldown = 5f;
    float skill2CooldownTimer;

    [Header("Skill 2 Hit Settings")]
    public float skill2Damage = 200f;

    [Tooltip("Kích thước hitbox (X=ngang, Y=độ cao, Z=chiều sâu về phía trước).")]
    public Vector3 skill2HitboxSize = new Vector3(4f, 2f, 7f);

    [Tooltip("Độ lệch tâm hitbox tính theo local của SpawnPoint (đẩy về trước trùng với VFX).")]
    public Vector3 skill2HitboxOffset = new Vector3(0f, 1f, 3.5f);

    [Tooltip("Layer của Enemy để quét va chạm (có thể để ~0 để quét mọi layer).")]
    public LayerMask enemyLayers = ~0;

    // ===== Skill 3 =====
    [Header("Skill 3 (Alpha4) - Targeted Strike")]
    public GameObject skill3EffectPrefab;
    public float skill3Damage = 500f;
    public float skill3AttackRange = 15f;
    public float skill3Cooldown = 10f;
    public float skill3CooldownTimer = 0f;

    private vMeleeManager melee;

    [Header("Weapon Requirements")]
    public string StaffNameContains = "Staff";

    public bool requireTargetInRangeForSkill3 = true;


    void Start()                                     
    {
        melee = GetComponent<vMeleeManager>();
    }
    void Update()
    {
        if (skill1CooldownTimer > 0f) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0f) skill2CooldownTimer -= Time.deltaTime;
        if (skill3CooldownTimer > 0f) skill3CooldownTimer -= Time.deltaTime; // NEW

        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            if (HasWeaponForSkill1()) 
            {
                animator.SetTrigger("StaffSkill1");
                skill1CooldownTimer = skill1Cooldown;
            }
            else
            {
                Debug.LogWarning("Bạn chưa cầm Staff. Hãy rút Staff ra tay để dùng Skill 1.");
            }
        }

            // Skill 2
            if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            animator.SetTrigger("StaffSkill2");
            skill2CooldownTimer = skill2Cooldown;
        }

        // Skill 3
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill3CooldownTimer <= 0f)
        {
            if (!requireTargetInRangeForSkill3 || IsEnemyInSkill3Range())
            {
                animator.SetTrigger("StaffSkill3");
                skill3CooldownTimer = skill3Cooldown;
            }
        }
    }

    // ===== Skill 1 =====
    public void SpawnFireball() // Animation Event
    {
        if (!fireballPrefab || !fireballSpawnPoint) return;

        Quaternion rot = Quaternion.LookRotation(fireballSpawnPoint.forward, Vector3.up);
        GameObject go = Instantiate(fireballPrefab, fireballSpawnPoint.position, rot);

        var prj = go.GetComponent<FireballProjectileplayer>();
        if (prj != null)
        {
            prj.Init(fireballSpawnPoint.forward, fireballSpeed, fireballDamage, transform);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = fireballSpawnPoint.forward * fireballSpeed;
#else
                rb.velocity = fireballSpawnPoint.forward * fireballSpeed;
#endif
            }
            else go.transform.forward = fireballSpawnPoint.forward;
        }

        go.GetComponent<MaykerStudio.Demo.Projectile>()?.Fire();
    }

    // ===== Skill 2 =====
    public void SpawnSkill2VFX() // Animation Event
    {
        if (!skill2VFXPrefab || !skill2VFXSpawnPoint) return;

        Instantiate(
            skill2VFXPrefab,
            skill2VFXSpawnPoint.position,
            skill2VFXSpawnPoint.rotation,
            skill2VFXSpawnPoint
        );

        DoSkill2Hit();
    }

    void DoSkill2Hit()
    {
        if (!skill2VFXSpawnPoint) return;

        Vector3 center = skill2VFXSpawnPoint.TransformPoint(skill2HitboxOffset);
        Vector3 flatFwd = Vector3.ProjectOnPlane(skill2VFXSpawnPoint.forward, Vector3.up).normalized;
        if (flatFwd.sqrMagnitude < 1e-4f) flatFwd = transform.forward;
        Quaternion rot = Quaternion.LookRotation(flatFwd, Vector3.up);

        var triggerMode = QueryTriggerInteraction.Collide;

        Collider[] hits = Physics.OverlapBox(
            center,
            skill2HitboxSize * 0.5f,
            rot,
            enemyLayers,
            triggerMode
        );

        for (int i = 0; i < hits.Length; i++)
        {
            var root = hits[i].transform.root;

            if (!root.CompareTag("Enemy") && !hits[i].CompareTag("Enemy")) continue;

            var health = root.GetComponent<Invector.vHealthController>();
            if (health)
            {
                var dmg = new Invector.vDamage
                {
                    damageValue = Mathf.RoundToInt(skill2Damage),
                    sender = transform,
                    hitPosition = hits[i].bounds.center,
                    hitReaction = true
                };
                health.TakeDamage(dmg);
            }
        }
    }

    // ===== Skill 3 =====
    public void SpawnSkill3EffectAtNearestEnemy() // Animation Event
    {
        GameObject nearestEnemy = FindNearestEnemyInRange(skill3AttackRange);
        if (nearestEnemy == null) return;

        var health = nearestEnemy.GetComponent<Invector.vHealthController>();
        if (health)
        {
            var dmg = new Invector.vDamage
            {
                damageValue = Mathf.RoundToInt(skill3Damage),
                sender = transform,
                hitPosition = nearestEnemy.transform.position,
                hitReaction = true
            };
            health.TakeDamage(dmg);
        }

        if (skill3EffectPrefab)
        {
            Instantiate(skill3EffectPrefab, nearestEnemy.transform.position, Quaternion.identity);
        }
    }

    private GameObject FindNearestEnemyInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0) return null;

        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            float dist = Vector3.Distance(pos, e.transform.position);
            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    public bool IsEnemyInSkill3Range()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0) return false;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (Vector3.Distance(transform.position, enemies[i].transform.position) <= skill3AttackRange)
                return true;
        }
        return false;
    }

    // === Cho UI & input dùng ===
    public bool HasWeaponForSkill1()
    {
        if (melee == null) melee = GetComponent<vMeleeManager>();
        var w = melee ? melee.CurrentActiveAttackWeapon : null;
        if (w == null) return false;
        var n = w.gameObject.name;
        return !string.IsNullOrEmpty(n) &&
               n.IndexOf(StaffNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void OnDrawGizmosSelected()
    {
        if (!skill2VFXSpawnPoint) return;
        Gizmos.color = new Color(0, 0.7f, 1f, 0.25f);

        Vector3 center = skill2VFXSpawnPoint.TransformPoint(skill2HitboxOffset);
        Vector3 flatFwd = Vector3.ProjectOnPlane(skill2VFXSpawnPoint.forward, Vector3.up).normalized;
        if (flatFwd.sqrMagnitude < 1e-4f) flatFwd = transform.forward;
        Quaternion rot = Quaternion.LookRotation(flatFwd, Vector3.up);

        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, skill2HitboxSize);
        Gizmos.matrix = prev;
    }

    public bool IsSkill1Ready() => skill1CooldownTimer <= 0f;
    public bool IsSkill2Ready() => skill2CooldownTimer <= 0f;
    public bool IsSkill3Ready() => skill3CooldownTimer <= 0f;

    public float GetSkill1CooldownPercent() => Mathf.Clamp01(skill1CooldownTimer / Mathf.Max(0.0001f, skill1Cooldown));
    public float GetSkill2CooldownPercent() => Mathf.Clamp01(skill2CooldownTimer / Mathf.Max(0.0001f, skill2Cooldown));
    public float GetSkill3CooldownPercent() => Mathf.Clamp01(skill3CooldownTimer / Mathf.Max(0.0001f, skill3Cooldown));
}
