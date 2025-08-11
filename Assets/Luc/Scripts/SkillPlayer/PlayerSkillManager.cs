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
    public GameObject buffVFXPrefab;
    public Transform buffVFXSpawnPoint;
    private GameObject currentBuffVFX;

    [Header("Skill 3 Settings")]
    public GameObject skill3EffectPrefab;  // Prefab hiệu ứng Skill 3
    public float skill3Cooldown = 10f;
    public float skill3CooldownTimer = 0;
    public float skill3AttackRange = 15f;

    public bool requireTargetInRangeForSkill3 = true;
                     
    [Header("Skill 1 — yêu cầu vũ khí")]
    private vMeleeManager melee;
    public string katanaNameContains = "Katana";



    private Invector.vCharacterController.vThirdPersonController controller;
    void Start()
    {
        melee = GetComponent<vMeleeManager>();
        controller = GetComponent<Invector.vCharacterController.vThirdPersonController>();
    }


    void Update()
    {
        // --- Cooldown Timer ---
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;
        if (skill3CooldownTimer > 0) skill3CooldownTimer -= Time.deltaTime;

        // --- Skill 1 ---
        // --- Skill 1 ---
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            if (IsHoldingKatana())
            {
                animator.SetTrigger("Skill1");
                skill1CooldownTimer = skill1Cooldown;
            }
            else
            {
                // tùy chọn: feedback cho người chơi
                Debug.LogWarning("Bạn chưa cầm Katana. Hãy rút Katana ra tay để dùng Skill 1.");
            }
        }


        // --- Skill 2 ---
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            animator.SetTrigger("Skill2");
            skill2CooldownTimer = skill2Cooldown;
        }

        // --- Skill 3 ---
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill3CooldownTimer <= 0f)
        {
            if (!requireTargetInRangeForSkill3 || IsEnemyInSkill3Range())
            {
                animator.SetTrigger("Skill3");
                skill3CooldownTimer = skill3Cooldown;
            }
        }
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
            if (script != null)
            {
                script.Fire();
            }
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
        StartCoroutine(SpawnAndLaunchFireballs());
    }

    IEnumerator SpawnAndLaunchFireballs()
    {
        List<OrbitalFireballs> orbList = new List<OrbitalFireballs>();

        // --- Spawn các quả cầu ---
        for (int i = 0; i < numberOfFireballs; i++)
        {
            float angle = i * (360f / numberOfFireballs);
            GameObject orbObj = Instantiate(fireballPrefab);
            var orbital = orbObj.GetComponent<OrbitalFireballs>();

            if (orbital != null)
            {
                orbital.Init(transform, angle, orbitRadius, 1f);
                orbList.Add(orbital);
            }
        }

        float timer = 0f;
        float maxWaitTime = 10f;
        bool hasLaunched = false;

        while (timer < maxWaitTime)
        {
            timer += Time.deltaTime;

            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            List<GameObject> validEnemies = new List<GameObject>();

            foreach (GameObject enemy in allEnemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist <= skill2AttackRange)
                {
                    validEnemies.Add(enemy);
                }
            }

            if (validEnemies.Count > 0)
            {
                int enemyIndex = 0;
                foreach (var orb in orbList)
                {
                    var target = validEnemies[enemyIndex % validEnemies.Count];
                    orb.FlyToTarget(target.transform);
                    enemyIndex++;
                    yield return new WaitForSeconds(1f);
                }

                hasLaunched = true;
                break;
            }

            yield return null; 
        }

        if (!hasLaunched)
        {
            Debug.Log("Không có enemy nào sau 10s, huỷ các quả cầu.");
            foreach (var orb in orbList)
            {
                Destroy(orb.gameObject);
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

        // Vũ khí đang cầm “thực sự” trên tay
        var active = melee.CurrentActiveAttackWeapon;
        if (active == null) return false;

        // So khớp theo tên GameObject của vũ khí (đơn giản & ổn định)
        // Nếu tên khác, sửa katanaNameContains trong Inspector
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
}
