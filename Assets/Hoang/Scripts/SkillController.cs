using UnityEngine;

public class SkillController : MonoBehaviour
{
    public Animator animator;

    [Header("Skill2 Settings")]
    public GameObject skill2AuraPrefab;
    public Transform auraSpawnPoint;

    public GameObject skill2VFXPrefab;
    public Transform skill2SpawnPoint;

    public float skill2Delay = 1f; // thời gian chờ trước khi skill xuất hiện



    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSkill2();
        }
    }

    void UseSkill2()
    {
        animator.SetTrigger("Skill2");

        // Spawn aura quanh người
        if (skill2AuraPrefab && auraSpawnPoint)
        {
            GameObject aura = Instantiate(skill2AuraPrefab, auraSpawnPoint.position, auraSpawnPoint.rotation, transform);
            Destroy(aura, 2f); // tự huỷ sau 2s
        }

        // Sau 1s thì mới spawn skill
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
