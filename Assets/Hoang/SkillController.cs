using UnityEngine;

public class SkillController : MonoBehaviour
{
    public Animator animator;

    [Header("Aura Settings")]
    public GameObject skill2AuraPrefab;
    public Transform auraSpawnPoint;

    [Header("Skill2 Settings")]
    public GameObject skill2VFXPrefab;
    public Transform skill2SpawnPoint;
    public float skill2Delay = 1f; // sau 1s mới spawn

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSkill2();
        }
    }

    void UseSkill2()
    {
        // Gọi animation
        animator.SetTrigger("Skill2");

        // Hiện Aura
        if (skill2AuraPrefab && auraSpawnPoint)
        {
            GameObject aura = Instantiate(skill2AuraPrefab, auraSpawnPoint.position, auraSpawnPoint.rotation, transform);
            Destroy(aura, 2f); // tự hủy sau 2s
        }

        // Gọi skill sau 1s
        Invoke(nameof(SpawnSkill2VFX), skill2Delay);
    }

    void SpawnSkill2VFX()
    {
        if (skill2VFXPrefab && skill2SpawnPoint)
        {
            Instantiate(skill2VFXPrefab, skill2SpawnPoint.position, skill2SpawnPoint.rotation);
        }
    }
}
