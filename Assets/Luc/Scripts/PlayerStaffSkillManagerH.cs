using UnityEngine;
using System.Collections;
using Invector.vCharacterController;
using Invector.vMelee;

public class PlayerStaffSkillManagerH : MonoBehaviour
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
    public Transform[] spawnPoints5;

    [Header("Movement Control")]
    public MonoBehaviour playerMovementScript;

    [Header("Cooldowns")]
    public float[] skillCooldowns = new float[5];
    private float[] cooldownTimers = new float[5];

    private vMeleeManager meleeManager;

    void Start()
    {
        meleeManager = GetComponent<vMeleeManager>();

        skillCooldowns[0] = 5f;
        skillCooldowns[1] = 10f;
        skillCooldowns[2] = 10f;
        skillCooldowns[3] = 15f;
        skillCooldowns[4] = 30f;
    }

    void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
            if (cooldownTimers[i] > 0) cooldownTimers[i] -= Time.deltaTime;

        // ✅ Skill 1 chỉ dùng khi có vũ khí
        if (Input.GetKeyDown(KeyCode.Alpha2) && cooldownTimers[0] <= 0f)
        {
            if (meleeManager != null && meleeManager.CurrentActiveAttackWeapon != null)
            {
                animator.SetTrigger("Skill1");
                cooldownTimers[0] = skillCooldowns[0];
                StartCoroutine(DelayedSkill1());
            }
            else
            {
                Debug.LogWarning("⚠️ Bạn chưa trang bị vũ khí! Hãy vào kho đồ (I) để trang bị Staff trước khi dùng Skill 1.");
            }
        }


        if (Input.GetKeyDown(KeyCode.Alpha3) && cooldownTimers[1] <= 0f)
        {
            animator.SetTrigger("Skill2");
            cooldownTimers[1] = skillCooldowns[1];
            StartCoroutine(DelayedSkill2());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && cooldownTimers[2] <= 0f)
        {
            animator.SetTrigger("Skill3");
            cooldownTimers[2] = skillCooldowns[2];
            LockPlayerInput(true);
            StartCoroutine(DelayedSkill3());
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) && cooldownTimers[3] <= 0f)
        {
            animator.SetTrigger("Skill4");
            cooldownTimers[3] = skillCooldowns[3];
            StartCoroutine(DelayedSkill4());
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) && cooldownTimers[4] <= 0f)
        {
            animator.SetTrigger("Skill5");
            cooldownTimers[4] = skillCooldowns[4];
            StartCoroutine(Skill5Routine());
        }
    }

    private IEnumerator DelayedSkill1()
    {
        yield return new WaitForSeconds(1f);
        if (skill1Prefab && spawnPoint1)
        {
            GameObject obj = Instantiate(skill1Prefab, spawnPoint1.position, spawnPoint1.rotation);
            Destroy(obj, 1.5f);
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
                Destroy(obj, 4f);
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
            Destroy(aura, 8f);
        }

        LockPlayerInput(false);
    }

    private IEnumerator DelayedSkill4()
    {
        yield return new WaitForSeconds(1f);
        if (skill4Prefab)
        {
            GameObject obj = Instantiate(skill4Prefab, transform.position, Quaternion.identity);
            obj.transform.SetParent(transform);
            Destroy(obj, 5f);
        }
    }

    private IEnumerator Skill5Routine()
    {
        LockPlayerInput(true);
        yield return new WaitForSeconds(1f);

        foreach (Transform point in spawnPoints5)
        {
            if (point != null && skill5Prefab)
            {
                GameObject obj = Instantiate(skill5Prefab, point.position, point.rotation);
                obj.transform.localScale *= 6f;

                ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in particles)
                {
                    var main = ps.main;
                    main.simulationSpeed = 0.5f;
                }

                Destroy(obj, 7f);
            }
        }

        yield return new WaitForSeconds(4f);
        LockPlayerInput(false);
    }

    void LockPlayerInput(bool isLocked)
    {
        if (playerMovementScript is vThirdPersonInput input)
        {
            input.cc.lockMovement = isLocked;
            input.cc.lockRotation = isLocked;

            if (isLocked)
            {
                input.cc._rigidbody.linearVelocity = Vector3.zero;
                input.cc.animator.applyRootMotion = true;
            }
            else
            {
                input.cc.animator.applyRootMotion = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (playerMovementScript is vThirdPersonInput input)
        {
            if (input.cc.lockMovement)
            {
                input.cc._rigidbody.linearVelocity = Vector3.zero;
            }
        }
    }
}
