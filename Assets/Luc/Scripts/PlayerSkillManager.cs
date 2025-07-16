using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public Animator animator;
    public GameObject projectilePrefab;      // Prefab Slash cho Skill 1
    public Transform projectileSpawnPoint;   // Vị trí spawn hiệu ứng Skill 1
    private GameObject currentBuffVFX;

    [Header("Skill 1 Settings")]
    public float skill1Cooldown = 3f;
    private float skill1CooldownTimer = 0;

    [Header("Skill 2 Settings")]
    public float skill2Cooldown = 8f;
    private float skill2CooldownTimer = 0;

    [Header("Skill 3 Settings")]
    public GameObject skill3EffectPrefab;  // Prefab hiệu ứng Skill 3
    public float skill3Cooldown = 10f;
    public float skill3CooldownTimer = 0;

    // (Nếu muốn spawn hiệu ứng buff khi dùng Skill 2)
    public GameObject buffVFXPrefab;
    public Transform buffVFXSpawnPoint;

    void Update()
    {
        // Đếm ngược hồi chiêu
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;

        // Skill 1 - Bắn Slash (Q hoặc số 2 tuỳ bạn chọn)
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            animator.SetTrigger("Skill1");
            skill1CooldownTimer = skill1Cooldown;
        }

        // Skill 2 - Buff (số 3)
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            animator.SetTrigger("Skill2");
            skill2CooldownTimer = skill2Cooldown;
        }

        // Đếm cooldown
        if (skill3CooldownTimer > 0) skill3CooldownTimer -= Time.deltaTime;

        // Skill 3 (ví dụ phím số 4)
        if (Input.GetKeyDown(KeyCode.Alpha4) && skill3CooldownTimer <= 0f)
        {
            animator.SetTrigger("Skill3");
            skill3CooldownTimer = skill3Cooldown;
        }
    }

    // Gọi từ Animation Event của Skill 1
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

    public void SpawnBuffVFX()
    {
        if (buffVFXPrefab && buffVFXSpawnPoint)
        {
            // Gán lại object vừa Instantiate cho currentBuffVFX
            currentBuffVFX = Instantiate(buffVFXPrefab, buffVFXSpawnPoint.position, Quaternion.identity, buffVFXSpawnPoint);
        }

        // Nếu có logic buff thì đặt ở đây
        // StartCoroutine(BuffSkillCooldown());
    }


    public void DestroyBuffVFX()
    {
        if (currentBuffVFX != null)
        {
            Destroy(currentBuffVFX);
            currentBuffVFX = null;
        }
    }

    public void SpawnSkill3EffectAtNearestEnemy()
    {
        // 1. Tìm Enemy gần nhất
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null && skill3EffectPrefab != null)
        {
            Instantiate(skill3EffectPrefab, nearestEnemy.transform.position, Quaternion.identity);
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }


    // Hàm kiểm tra còn hồi chiêu không (hữu ích cho UI)
    public bool IsSkill1Ready() => skill1CooldownTimer <= 0f;
    public bool IsSkill2Ready() => skill2CooldownTimer <= 0f;

    public float GetSkill1CooldownPercent() => Mathf.Clamp01(skill1CooldownTimer / skill1Cooldown);
    public float GetSkill2CooldownPercent() => Mathf.Clamp01(skill2CooldownTimer / skill2Cooldown);

    // Nếu muốn buff ảnh hưởng đến cooldown, bạn có thể làm thêm Coroutine ở đây
    // IEnumerator BuffSkillCooldown() { ... }
}
