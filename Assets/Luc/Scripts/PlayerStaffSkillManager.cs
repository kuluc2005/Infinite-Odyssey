using UnityEngine;
using System.Collections;

public class PlayerStaffSkillManager : MonoBehaviour
{
    public Animator animator;

    [Header("Skill 1")]
    public GameObject skill1Prefab;
    public Transform spawnPoint1;

    [Header("Skill 2")]
    public GameObject skill2ProjectilePrefab;
    public Transform skill2SpawnPoint;

    [Header("Skill 3")]
    public GameObject skill3AuraPrefab;

    [Header("Skill 4")]
    public GameObject skill4Prefab;

    [Header("Skill 5")]
    public GameObject skill5Prefab;
    public Transform[] spawnPoints5; // 🔥 danh sách 3 vị trí

    [Header("Movement Control")]
    public MonoBehaviour playerMovementScript;

    [Header("Cooldowns")]
    public float[] skillCooldowns = new float[5]; // Skill 1–5
    private float[] cooldownTimers = new float[5];

    void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
            if (cooldownTimers[i] > 0) cooldownTimers[i] -= Time.deltaTime;

        // Skill 1 (Delay 1s)
        if (Input.GetKeyDown(KeyCode.Alpha2) && cooldownTimers[0] <= 0f)
        {
            animator.SetTrigger("Skill1");
            cooldownTimers[0] = skillCooldowns[0];
            StartCoroutine(DelayedSkill1());
        }

        // Skill 2 (Delay 0.5s)
        if (Input.GetKeyDown(KeyCode.Alpha3) && cooldownTimers[1] <= 0f)
        {
            animator.SetTrigger("Skill2");
            cooldownTimers[1] = skillCooldowns[1];
            StartCoroutine(DelayedSkill2());
        }

        // Skill 3 (Delay 1.2s + đứng yên)
        if (Input.GetKeyDown(KeyCode.Alpha4) && cooldownTimers[2] <= 0f)
        {
            animator.SetTrigger("Skill3");
            cooldownTimers[2] = skillCooldowns[2];
            if (playerMovementScript)
                playerMovementScript.enabled = false;
            StartCoroutine(DelayedSkill3());
        }

        // Skill 4 (Delay 1s)
        if (Input.GetKeyDown(KeyCode.Alpha5) && cooldownTimers[3] <= 0f)
        {
            animator.SetTrigger("Skill4");
            cooldownTimers[3] = skillCooldowns[3];
            StartCoroutine(DelayedSkill4());
        }

        // Skill 5 (Delay 1s + đứng yên 5s)
        if (Input.GetKeyDown(KeyCode.Alpha6) && cooldownTimers[4] <= 0f)
        {
            animator.SetTrigger("Skill5");
            cooldownTimers[4] = skillCooldowns[4];
            StartCoroutine(Skill5Routine());
        }
    }

    // === Delayed Skill Coroutines ===

    private IEnumerator DelayedSkill1()
    {
        yield return new WaitForSeconds(1f);
        if (skill1Prefab && spawnPoint1)
        {
            GameObject obj = Instantiate(skill1Prefab, spawnPoint1.position, spawnPoint1.rotation);
            Destroy(obj, 1.5f); // Tồn tại 1.5s
        }
    }

    private IEnumerator DelayedSkill2()
    {
        yield return new WaitForSeconds(0.5f);
        if (skill2ProjectilePrefab && skill2SpawnPoint)
        {
            float angleOffset = 15f;
            for (int i = -1; i <= 1; i++)
            {
                Quaternion rotation = Quaternion.Euler(skill2SpawnPoint.eulerAngles + new Vector3(0, i * angleOffset, 0));
                GameObject obj = Instantiate(skill2ProjectilePrefab, skill2SpawnPoint.position, rotation);
                Destroy(obj, 4f); // Tồn tại 4s
            }
        }
    }

    private IEnumerator DelayedSkill3()
    {
        yield return new WaitForSeconds(1.2f);
        if (skill3AuraPrefab)
        {
            GameObject aura = Instantiate(skill3AuraPrefab, transform.position, Quaternion.identity);
            aura.transform.SetParent(transform);
            Destroy(aura, 8f); // Tồn tại 8s
        }
        if (playerMovementScript)
            playerMovementScript.enabled = true;
    }

    private IEnumerator DelayedSkill4()
    {
        yield return new WaitForSeconds(1f);
        if (skill4Prefab)
        {
            GameObject obj = Instantiate(skill4Prefab, transform.position, Quaternion.identity);
            obj.transform.SetParent(transform);
            Destroy(obj, 5f); // Tồn tại 5s
        }
    }

    private IEnumerator Skill5Routine()
    {
        if (playerMovementScript) playerMovementScript.enabled = false;

        yield return new WaitForSeconds(1f);

        foreach (Transform point in spawnPoints5)
        {
            if (point != null && skill5Prefab)
            {
                GameObject obj = Instantiate(skill5Prefab, point.position, point.rotation);

                // 📏 Phóng to x3
                obj.transform.localScale *= 6f;

                // 🐌 Giảm speed playback của tất cả Particle System trong prefab
                ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in particles)
                {
                    var main = ps.main;
                    main.simulationSpeed = 0.5f; // 🔥 playback chậm 2 lần
                }

                // ❌ Không cho prefab bay/di chuyển ⇒ giữ nguyên vị trí
                // (không cần thêm gì nếu prefab vốn đứng yên)

                Destroy(obj, 7f);
            }
        }

        yield return new WaitForSeconds(4f); // tổng 5s đứng yên

        if (playerMovementScript) playerMovementScript.enabled = true;
    }



}
