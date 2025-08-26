using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Invector.vMelee;


public class PlayerSkillManager : MonoBehaviour
{
    public Animator animator;

    [Header("Skill 1 Settings")]
    public GameObject projectilePrefab;      // Prefab Slash cho Skill 1
    public Transform projectileSpawnPoint;   // Vị trí spawn hiệu ứng Skill 1
    public float skill1Cooldown = 3f;
    private float skill1CooldownTimer = 0;

    [Header("Skill 2 Settings")]
    public GameObject fireballPrefab;
    public int numberOfFireballs = 5;
    public float orbitRadius = 2f;
    public float orbitSpeed = 40f;
    public float skill2Cooldown = 8f;
    private float skill2CooldownTimer = 0;
    public float skill2AttackRange = 50f;

    [Tooltip("Tối đa thời gian chờ có mục tiêu trước khi hủy các quả cầu (giây)")]
    public float skill2MaxWaitForTargets = 10f;

    [Tooltip("Sau khi phóng, nếu không còn mục tiêu hợp lệ, quả cầu sẽ tự hủy trong thời gian này (giây)")]
    public float skill2OrbsMaxLifetimeAfterLaunch = 6f;

    [Tooltip("Tần suất kiểm tra lại mục tiêu để retarget (giây)")]
    public float skill2RetargetInterval = 0.15f;

    public GameObject buffVFXPrefab;
    public Transform buffVFXSpawnPoint;
    private GameObject currentBuffVFX;

    [Header("Skill 3 Settings")]
    public GameObject skill3EffectPrefab;  
    public float skill3Cooldown = 10f;
    public float skill3CooldownTimer = 0;
    public float skill3AttackRange = 15f;
    public bool requireTargetInRangeForSkill3 = true;

    [Header("Skill 1 — yêu cầu vũ khí")]
    private vMeleeManager melee;
    public string katanaNameContains = "Katana";

    private Invector.vCharacterController.vThirdPersonController controller;

    // ======= AUDIO / SFX =======
    [Header("Audio — General")]
    [Tooltip("AudioSource phát SFX. Nếu để trống sẽ tự tạo.")]
    public AudioSource sfxSource;

    [Tooltip("Chỉ phát âm thanh khi Animation Event gọi. Nếu false, code sẽ tự phát SFX ở các thời điểm hợp lý.")]
    public bool useAnimationEventsOnly = false;

    [Tooltip("Random pitch ±rng khi phát SFX (0 = tắt).")]
    [Range(0f, 0.5f)] public float randomPitchRange = 0.08f;

    [Header("Audio — Skill 1")]
    public AudioClip sfxSkill1Cast;
    public AudioClip sfxSkill1Impact;   // khi projectile chém / xuất chiêu

    [Header("Audio — Skill 2")]
    public AudioClip sfxSkill2Start;    // bắt đầu buff/charge
    public AudioClip sfxSkill2Launch;   // mỗi lần phóng 1 orb
    public AudioClip sfxSkill2Impact;   // khi orb trúng đích (gọi từ OrbitalFireballs khi va chạm)

    [Header("Audio — Skill 3")]
    public AudioClip sfxSkill3Cast;     // bắt đầu giơ tay/cast
    public AudioClip sfxSkill3Impact;   // lúc giáng đòn/đặt effect lên enemy

    // ===== Helper theo dõi đạn Skill 2 =====
    private class OrbTrack
    {
        public OrbitalFireballs orb;
        public Transform target;
        public bool launched;
        public float launchedAt;
    }

    // ====== Stamina Costs ======
    [Header("Stamina Costs")]
    public float staminaCostSkill1 = 30f;
    public float staminaCostSkill2 = 50f;
    public float staminaCostSkill3 = 100f;

    [Tooltip("BẬT để chặn cast khi stamina hiện tại không đủ (đọc bằng reflection).")]
    public bool requireEnoughStaminaToTrigger = false;

    private Invector.vCharacterController.vThirdPersonController motor;

    void Awake()
    {
        // Chuẩn AudioSource
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;     // 0 = 2D (UI-like). Nếu muốn 3D đổi sang 1f.
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
        // --- Cooldown Timer ---
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;
        if (skill3CooldownTimer > 0) skill3CooldownTimer -= Time.deltaTime;

        // --- Skill 1 ---
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            if (IsHoldingKatana() && CanPayStamina(staminaCostSkill1))       
            {
                animator.SetTrigger("Skill1");
                skill1CooldownTimer = skill1Cooldown;
            }
            else
            {
                Debug.LogWarning("Không thể dùng Skill 1 (chưa cầm Katana hoặc không đủ Stamina).");
            }
        }

        // --- Skill 2 ---
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            if (CanPayStamina(staminaCostSkill2))                         
            {
                animator.SetTrigger("Skill2");
                skill2CooldownTimer = skill2Cooldown;
            }
            else
            {
                Debug.LogWarning("Không đủ Stamina cho Skill 2.");
            }
        }

        // --- Skill 3 ---
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill3CooldownTimer <= 0f)
        {
            if (IsHoldingKatana() &&
                (!requireTargetInRangeForSkill3 || IsEnemyInSkill3Range()) &&
                CanPayStamina(staminaCostSkill3))                                 
            {
                animator.SetTrigger("Skill3");
                skill3CooldownTimer = skill3Cooldown;
            }
            else
            {
                Debug.LogWarning("Không thể dùng Skill 3 (vũ khí/mục tiêu/Stamina không đạt).");
            }
        }
    }

    public void ConsumeStamina_Skill1() { ConsumeNow(staminaCostSkill1); }
    public void ConsumeStamina_Skill2() { ConsumeNow(staminaCostSkill2); }
    public void ConsumeStamina_Skill3() { ConsumeNow(staminaCostSkill3); }

    private void TrySetStaminaDelay(float delay)
    {
        var t = motor.GetType().BaseType; 
        var f = t.GetField("currentStaminaRecoveryDelay",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null) f.SetValue(motor, delay);
    }

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

    // -----------------------------
    // Skill 1 Logic
    // -----------------------------
    public void SpawnProjectile()
    {
        if (projectilePrefab && projectileSpawnPoint)
        {
            GameObject prj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            var script = prj.GetComponent<MaykerStudio.Demo.Projectile>();
            if (script != null) script.Fire();

        }
    }

    // -----------------------------
    // Skill 2 Logic 
    // -----------------------------
    public void SpawnBuffVFX()
    {
        if (buffVFXPrefab && buffVFXSpawnPoint)
        {
            currentBuffVFX = Instantiate(buffVFXPrefab, buffVFXSpawnPoint.position, Quaternion.identity, buffVFXSpawnPoint);
        }
    }

    public void DestroyBuffVFX()
    {
        if (currentBuffVFX != null)
        {
            Destroy(currentBuffVFX);
            currentBuffVFX = null;
        }
    }

    public void SpawnFireballsAroundPlayer()
    {
        StartCoroutine(SpawnAndLaunchFireballs_Robust());
    }

    private List<Transform> GetValidEnemiesForSkill2()
    {
        var result = new List<Transform>();
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in allEnemies)
        {
            if (!e) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > skill2AttackRange) continue;

            var hp = e.GetComponent<Invector.vHealthController>();
            if (hp == null || hp.isDead) continue;

            result.Add(e.transform);
        }
        return result;
    }

    IEnumerator SpawnAndLaunchFireballs_Robust()
    {
        List<OrbTrack> orbs = new List<OrbTrack>();

        // Spawn các orb quay quanh player
        for (int i = 0; i < numberOfFireballs; i++)
        {
            float angle = i * (360f / Mathf.Max(1, numberOfFireballs));
            GameObject orbObj = Instantiate(fireballPrefab);
            var orbital = orbObj.GetComponent<OrbitalFireballs>();

            if (orbital != null)
            {
                orbital.Init(transform, angle, orbitRadius, 1f);
                // Truyền owner + clip impact để orb gọi âm thanh khi trúng
                orbital.SetupSFX(this, sfxSkill2Impact);
                orbs.Add(new OrbTrack { orb = orbital, target = null, launched = false, launchedAt = -1f });
            }
            else
            {
                Destroy(orbObj);
            }
        }

        // Chờ có mục tiêu rồi lần lượt phóng
        float waitTimer = 0f;
        bool launchedAny = false;

        while (waitTimer < skill2MaxWaitForTargets)
        {
            waitTimer += Time.deltaTime;

            orbs.RemoveAll(o => o == null || o.orb == null || o.orb.gameObject == null);
            if (orbs.Count == 0) yield break;

            var validTargets = GetValidEnemiesForSkill2();

            if (validTargets.Count > 0)
            {
                int enemyIndex = 0;
                foreach (var ot in orbs)
                {
                    if (ot.orb == null) continue;

                    var target = validTargets[enemyIndex % validTargets.Count];
                    ot.target = target;
                    ot.launched = true;
                    ot.launchedAt = Time.time;

                    ot.orb.FlyToTarget(target);


                    enemyIndex++;
                    yield return new WaitForSeconds(1f);
                }

                launchedAny = true;
                break;
            }

            yield return null;
        }

        // Không có mục tiêu → hủy các orb
        if (!launchedAny)
        {
            foreach (var ot in orbs)
                if (ot != null && ot.orb != null) Destroy(ot.orb.gameObject);
            yield break;
        }

        // Giám sát: retarget / tự hủy khi hết mục tiêu
        float monitorElapsed = 0f;
        while (true)
        {
            yield return new WaitForSeconds(skill2RetargetInterval);
            monitorElapsed += skill2RetargetInterval;

            orbs.RemoveAll(o => o == null || o.orb == null || o.orb.gameObject == null);
            if (orbs.Count == 0) break;

            var validTargetsNow = GetValidEnemiesForSkill2();

            foreach (var ot in orbs)
            {
                if (ot.orb == null) continue;

                if (!ot.launched)
                {
                    Destroy(ot.orb.gameObject);
                    continue;
                }

                bool targetValid =
                    ot.target != null &&
                    validTargetsNow.Contains(ot.target);

                if (!targetValid)
                {
                    if (validTargetsNow.Count > 0)
                    {
                        Transform best = null;
                        float bestDist = float.MaxValue;
                        Vector3 orbPos = ot.orb.transform.position;

                        foreach (var t in validTargetsNow)
                        {
                            float d = Vector3.Distance(orbPos, t.position);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                best = t;
                            }
                        }

                        if (best != null)
                        {
                            ot.target = best;
                            ot.orb.FlyToTarget(best);
                        }
                    }
                    else
                    {
                        if (ot.launchedAt > 0f &&
                            Time.time - ot.launchedAt >= skill2OrbsMaxLifetimeAfterLaunch)
                        {
                            Destroy(ot.orb.gameObject);
                        }
                    }
                }
            }

            bool anyTargetLeft = GetValidEnemiesForSkill2().Count > 0;
            if (!anyTargetLeft)
            {
                bool anyAliveOrb = false;
                foreach (var ot in orbs)
                {
                    if (ot.orb != null &&
                        (Time.time - ot.launchedAt) < skill2OrbsMaxLifetimeAfterLaunch)
                    {
                        anyAliveOrb = true;
                        break;
                    }
                }

                if (!anyAliveOrb)
                {
                    foreach (var ot in orbs)
                        if (ot.orb != null) Destroy(ot.orb.gameObject);
                    break;
                }
            }
        }
    }

    // -----------------------------
    // Skill 3 Logic
    // -----------------------------
    public void SpawnSkill3EffectAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemyInRange(skill3AttackRange);
        if (nearestEnemy != null && skill3EffectPrefab != null)
        {
            // --- Gây damage ---
            var health = nearestEnemy.GetComponent<Invector.vHealthController>();
            if (health != null)
            {
                Invector.vDamage dmg = new Invector.vDamage
                {
                    damageValue = 500,
                    sender = transform,
                    hitPosition = nearestEnemy.transform.position,
                    hitReaction = true
                };
                health.TakeDamage(dmg);
            }

            // --- Hiệu ứng ---
            Instantiate(skill3EffectPrefab, nearestEnemy.transform.position, Quaternion.identity);

        }
    }

    private GameObject FindNearestEnemyInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy) continue;

            var hp = enemy.GetComponent<Invector.vHealthController>();
            if (hp == null || hp.isDead) continue;

            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist <= range && dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    public bool IsEnemyInSkill3Range()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (!enemy) continue;
            var hp = enemy.GetComponent<Invector.vHealthController>();
            if (hp == null || hp.isDead) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= skill3AttackRange)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsHoldingKatana()
    {
        if (melee == null) return false;

        var active = melee.CurrentActiveAttackWeapon;
        if (active == null) return false;

        var goName = active.gameObject.name;
        return !string.IsNullOrEmpty(goName) &&
               !string.IsNullOrEmpty(katanaNameContains) &&
               goName.IndexOf(katanaNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // === Cho UI & input dùng ===
    public bool HasWeaponForSkill1()
    {
        if (melee == null) melee = GetComponent<vMeleeManager>();
        var w = melee ? melee.CurrentActiveAttackWeapon : null;
        if (w == null) return false;
        var n = w.gameObject.name;
        return !string.IsNullOrEmpty(n) &&
               n.IndexOf(katanaNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // -----------------------------
    // UI Support Functions
    // -----------------------------
    public bool IsSkill1Ready() => skill1CooldownTimer <= 0f;
    public bool IsSkill2Ready() => skill2CooldownTimer <= 0f;

    public float GetSkill1CooldownPercent() => Mathf.Clamp01(skill1CooldownTimer / skill1Cooldown);
    public float GetSkill2CooldownPercent() => Mathf.Clamp01(skill2CooldownTimer / skill2Cooldown);

    // =========================================================
    // ============== ANIMATION EVENT SFX HOOKS ================
    // =========================================================

    public void SFX_Skill1_Cast() { SFX_Play(sfxSkill1Cast); }
    public void SFX_Skill1_Impact() { SFX_Play(sfxSkill1Impact); }

    public void SFX_Skill2_Start() { SFX_Play(sfxSkill2Start); }
    public void SFX_Skill2_Launch() { SFX_Play(sfxSkill2Launch); }
    public void SFX_Skill2_Impact() { SFX_Play(sfxSkill2Impact); }

    public void SFX_Skill3_Cast() { SFX_Play(sfxSkill3Cast); }
    public void SFX_Skill3_Impact() { SFX_Play(sfxSkill3Impact); }

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
