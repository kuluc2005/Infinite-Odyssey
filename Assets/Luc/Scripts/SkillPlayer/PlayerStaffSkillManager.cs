using UnityEngine;
using Invector.vMelee;

public class PlayerStaffSkillManager : MonoBehaviour
{
    public Animator animator;

    // ====== NEW: Stamina Costs ======
    [Header("Stamina Costs")]
    public float staminaCostSkill1 = 20f; // Fireball
    public float staminaCostSkill2 = 35f; // Ice AOE
    public float staminaCostSkill3 = 50f; // Targeted Strike / Burst

    [Tooltip("BẬT để chặn cast khi stamina hiện tại không đủ (đọc bằng reflection).")]
    public bool requireEnoughStaminaToTrigger = false;

    // NEW: cache motor để trừ stamina
    private Invector.vCharacterController.vThirdPersonController motor;

    // ──────────────────────────────────────────────────────────────────────────

    [Header("Skill 1 (Alpha2) - Fireball")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 30f;
    public float fireballDamage = 100f;
    public float skill1Cooldown = 120f;
    float skill1CooldownTimer;

    [Header("Skill 2 (Alpha3) - Ice Spikes (AOE)")]
    public GameObject skill2VFXPrefab;
    public Transform skill2VFXSpawnPoint;
    public float skill2Cooldown = 5f;
    float skill2CooldownTimer;

    [Header("Skill 2 Hit Settings")]
    public float skill2Damage = 200f;
    public Vector3 skill2HitboxSize = new Vector3(4f, 2f, 7f);
    public Vector3 skill2HitboxOffset = new Vector3(0f, 1f, 3.5f);
    public LayerMask enemyLayers = ~0;

    [Header("Skill 3 (Alpha4) - Targeted Strike")]
    public GameObject skill3EffectPrefab;
    public float skill3Damage = 500f;
    public float skill3AttackRange = 15f;
    public float skill3Cooldown = 10f;
    public float skill3CooldownTimer = 0f;

    [Header("Skill 3 - Fireball Burst")]
    public GameObject skill3FireballPrefab;
    public Transform skill3FireballSpawnPoint;
    public float skill3FireballSpeed = 22f;
    public float skill3FireballDamage = 60f;
    public float skill3FireballMaxLifetime = 5f;

    private Transform skill3LockedTarget;
    private vMeleeManager melee;

    [Header("Weapon Requirements")]
    public string StaffNameContains = "Staff";
    public bool requireTargetInRangeForSkill3 = true;

    // ======= AUDIO / SFX =======
    // Audio — General
    [Header("Audio — General")]
    [Tooltip("AudioSource phát SFX. Nếu để trống sẽ tự tạo.")]
    public AudioSource sfxSource;

    [Tooltip("Chỉ phát âm thanh khi Animation Event gọi. Nếu false, code sẽ tự phát SFX ở các thời điểm hợp lý.")]
    public bool useAnimationEventsOnly = true;

    [Tooltip("Random pitch ±rng khi phát SFX (0 = tắt).")]
    [Range(0f, 0.5f)] public float randomPitchRange = 0.08f;

    [Header("Audio — Skill 1 (Fireball)")]
    public AudioClip sfxSkill1Cast;
    public AudioClip sfxSkill1Release;

    [Header("Audio — Skill 2 (Ice AOE)")]
    public AudioClip sfxSkill2Cast;
    public AudioClip sfxSkill2Impact;

    [Header("Audio — Skill 3 (Strike/Burst)")]
    public AudioClip sfxSkill3Cast;
    public AudioClip sfxSkill3Impact;

    void Awake()
    {
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D
            sfxSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    void Start()
    {
        melee = GetComponent<vMeleeManager>();
        motor = GetComponent<Invector.vCharacterController.vThirdPersonController>();
    }

    void Update()
    {
        if (skill1CooldownTimer > 0f) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0f) skill2CooldownTimer -= Time.deltaTime;
        if (skill3CooldownTimer > 0f) skill3CooldownTimer -= Time.deltaTime;

        // Skill 1 (Alpha2)
        // Skill 1
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            if (HasStaffEquipped() && CanPayStamina(staminaCostSkill1))           
            {
                animator.SetTrigger("StaffSkill1");
                skill1CooldownTimer = skill1Cooldown;
            }
            else Debug.LogWarning("Không thể dùng Skill 1 (Staff/Stamina).");
        }

        // Skill 2
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            if (HasStaffEquipped() && CanPayStamina(staminaCostSkill2))        
            {
                animator.SetTrigger("StaffSkill2");
                skill2CooldownTimer = skill2Cooldown;
            }
            else Debug.LogWarning("Không thể dùng Skill 2 (Staff/Stamina).");
        }

        // Skill 3
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill3CooldownTimer <= 0f)
        {
            if (HasStaffEquipped() &&
                (!requireTargetInRangeForSkill3 || IsEnemyInSkill3Range()) &&
                CanPayStamina(staminaCostSkill3))                                 
            {
                animator.SetTrigger("StaffSkill3");
                skill3CooldownTimer = skill3Cooldown;
            }
            else Debug.LogWarning("Không thể dùng Skill 3 (Staff/Target/Stamina).");
        }
    }

    public void ConsumeStamina_Skill1() { ConsumeNow(staminaCostSkill1); }
    public void ConsumeStamina_Skill2() { ConsumeNow(staminaCostSkill2); }
    public void ConsumeStamina_Skill3() { ConsumeNow(staminaCostSkill3); }

    private void ConsumeNow(float cost)
    {
        if (!motor) return;
        motor.ChangeStamina(-Mathf.RoundToInt(cost));
        TrySetStaminaDelay(1f);
    }

    private bool CanPayStamina(float cost)
    {
        if (!requireEnoughStaminaToTrigger) return true;
        float cur = TryGetCurrentStamina();
        return cur >= cost;
    }

    private float TryGetCurrentStamina()
    {
        if (!motor) return float.MaxValue;
        var t = motor.GetType().BaseType; 
        var f = t.GetField("currentStamina", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null)
        {
            object v = f.GetValue(motor);
            if (v is float fv) return fv;
        }
        return float.MaxValue;
    }

    private void TrySetStaminaDelay(float delay)
    {
        var t = motor.GetType().BaseType;
        var f = t.GetField("currentStaminaRecoveryDelay",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null) f.SetValue(motor, delay);
    }

    // ===== Skill 1 =====
    public void SFX_Skill1_Cast() { SFX_Play(sfxSkill1Cast); }
    public void SFX_Skill1_Release() { SFX_Play(sfxSkill1Release); }

    public void SpawnFireball()
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
    public void SFX_Skill2_Cast() { SFX_Play(sfxSkill2Cast); }

    public void SpawnSkill2VFX()
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

    public void SFX_Skill2_Impact() { SFX_Play(sfxSkill2Impact); }

    void DoSkill2Hit()
    {
        if (!skill2VFXSpawnPoint) return;

        // Tính box theo forward phẳng để VFX & hitbox khớp hướng
        Vector3 center = skill2VFXSpawnPoint.TransformPoint(skill2HitboxOffset);
        Vector3 flatFwd = Vector3.ProjectOnPlane(skill2VFXSpawnPoint.forward, Vector3.up).normalized;
        if (flatFwd.sqrMagnitude < 1e-4f) flatFwd = transform.forward;
        Quaternion rot = Quaternion.LookRotation(flatFwd, Vector3.up);

        // Lấy mọi collider (cả trigger) trên các layer cho phép
        Collider[] hits = Physics.OverlapBox(
            center,
            skill2HitboxSize * 0.5f,
            rot,
            enemyLayers,
            QueryTriggerInteraction.Collide
        );

        // Tránh double-hit 1 enemy có nhiều collider
        var damaged = new System.Collections.Generic.HashSet<Invector.vHealthController>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];

            var health = col.GetComponentInParent<Invector.vHealthController>();
            if (health == null) continue; 

            bool taggedEnemy =
                  col.CompareTag("Enemy")
               || (col.transform.root && col.transform.root.CompareTag("Enemy"))
               || (health.transform.CompareTag("Enemy"));

            // taggedEnemy = true;

            if (!taggedEnemy) continue;

            if (health.isDead) continue; 

            if (damaged.Contains(health)) continue; 

            // Gây damage
            var dmg = new Invector.vDamage
            {
                damageValue = Mathf.RoundToInt(skill2Damage),
                sender = transform,
                hitPosition = col.bounds.center,
                hitReaction = true
            };
            health.TakeDamage(dmg);
            damaged.Add(health);
        }
    }


    // ===== Skill 3 =====
    public void SFX_Skill3_Cast() { SFX_Play(sfxSkill3Cast); }
    public void SFX_Skill3_Impact() { SFX_Play(sfxSkill3Impact); }

    public void SpawnSkill3EffectAtNearestEnemy()
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

    public void Skill3_OnStart_LockTarget() // Animation Event
    {
        GameObject nearestEnemy = FindNearestEnemyInRange(skill3AttackRange);
        skill3LockedTarget = nearestEnemy ? nearestEnemy.transform : null;
    }

    public void Skill3_SpawnFireball()
    {
        GameObject prefab = skill3FireballPrefab ? skill3FireballPrefab : fireballPrefab;
        Transform spawn = skill3FireballSpawnPoint ? skill3FireballSpawnPoint : fireballSpawnPoint;
        float spd = skill3FireballSpeed > 0 ? skill3FireballSpeed : fireballSpeed;
        float dmg = skill3FireballDamage > 0 ? skill3FireballDamage : fireballDamage;

        if (!prefab || !spawn) return;

        Vector3 dir;
        if (skill3LockedTarget != null)
        {
            dir = (skill3LockedTarget.position - spawn.position).normalized;
            if (dir.sqrMagnitude < 1e-4f) dir = spawn.forward;
        }
        else
        {
            dir = spawn.forward;
        }

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        GameObject go = Instantiate(prefab, spawn.position, rot);

        var prj = go.GetComponent<FireballProjectileplayer>();
        if (prj != null)
        {
            prj.Init(dir, spd, dmg, transform);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = dir * spd;
#else
                rb.velocity = dir * spd;
#endif
            }
            Destroy(go, skill3FireballMaxLifetime);
        }

        go.GetComponent<MaykerStudio.Demo.Projectile>()?.Fire();
    }

    // ===== Helpers =====
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
            var hp = e.GetComponent<Invector.vHealthController>();
            if (hp != null && hp.isDead) continue;

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
            var hp = enemies[i].GetComponent<Invector.vHealthController>();
            if (hp != null && hp.isDead) continue;
            if (Vector3.Distance(transform.position, enemies[i].transform.position) <= skill3AttackRange)
                return true;
        }
        return false;
    }

    // === Điều kiện cầm Staff ===
    private bool HasStaffEquipped()
    {
        if (melee == null) melee = GetComponent<vMeleeManager>();
        var w = melee ? melee.CurrentActiveAttackWeapon : null;
        if (w == null) return false;
        var n = w.gameObject.name;
        return !string.IsNullOrEmpty(n) &&
               n.IndexOf(StaffNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // (Giữ method cũ nếu bạn đang gọi từ nơi khác)
    public bool HasWeaponForSkill1() => HasStaffEquipped();

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

    // ===================== AUDIO CORE =====================
    private void SFX_Play(AudioClip clip, float volume = 1f)
    {
        if (!clip || !sfxSource) return;

        float original = sfxSource.pitch;
        if (randomPitchRange > 0f)
        {
            float delta = Random.Range(-randomPitchRange, randomPitchRange);
            sfxSource.pitch = Mathf.Clamp(1f + delta, 0.5f, 2f);
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        sfxSource.pitch = original;
    }
}
