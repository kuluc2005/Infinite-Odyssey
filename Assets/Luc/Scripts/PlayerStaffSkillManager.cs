using UnityEngine;

public class PlayerStaffSkillManager : MonoBehaviour
{
    public Animator animator;
    public GameObject fireballPrefab;           // Prefab Skill 1
    public Transform fireballSpawnPoint;

    public GameObject skill2VFXPrefab;          // Prefab hiệu ứng buff Skill 2 (aura/vòng sáng...)
    public Transform skill2VFXSpawnPoint;       // Vị trí spawn hiệu ứng buff (dưới chân, giữa người...)

    public float skill1Cooldown = 2f;
    private float skill1CooldownTimer = 0f;

    public float skill2Cooldown = 5f;
    private float skill2CooldownTimer = 0f;

    void Update()
    {
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;

        // Skill 1
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill1CooldownTimer <= 0f)
        {
            animator.SetTrigger("StaffSkill1");
            skill1CooldownTimer = skill1Cooldown;
        }

        // Skill 2 (chỉ buff, không bắn)
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill2CooldownTimer <= 0f)
        {
            animator.SetTrigger("StaffSkill2");
            skill2CooldownTimer = skill2Cooldown;
        }
    }

    // Animation Event Skill 1
    public void SpawnFireball()
    {
        if (fireballPrefab && fireballSpawnPoint)
        {
            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);
            var projectile = fireball.GetComponent<MaykerStudio.Demo.Projectile>();
            if (projectile != null)
                projectile.Fire();
        }
    }

    // Animation Event Skill 2 (buff tại chỗ, không di chuyển)
    public void SpawnSkill2VFX()
    {
        if (skill2VFXPrefab && skill2VFXSpawnPoint)
        {
            Instantiate(skill2VFXPrefab, skill2VFXSpawnPoint.position, skill2VFXSpawnPoint.rotation, skill2VFXSpawnPoint);
        }
    }
}
