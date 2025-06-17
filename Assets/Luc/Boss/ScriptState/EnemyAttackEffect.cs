using UnityEngine;
using Invector;
using System.Collections;

public class EnemyAttackEffect : MonoBehaviour
{
    private bool hasEnteredSkillMode = false;
    private bool isSpammingSkill2 = false;
    private bool hasTriggeredDeath = false;

    [Header("Skill Prefabs")]
    public GameObject skill1Effect; // Skill sau 3 đòn đánh thường
    public GameObject skill2Effect; // Skill khi còn ≤ 30 máu

    [Header("Settings")]
    public int skill1Damage = 25;
    public int skill2Damage = 50;
    public float skill2Interval = 6f;

    // -------------------------------
    // SKILL 1: Gọi từ Animator sau 3 đòn đánh thường
    // -------------------------------
    public void SpawnSkill1()
    {
        if (GetComponent<vHealthController>().isDead) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && skill1Effect != null)
        {
            Vector3 pos = player.transform.position;
            Instantiate(skill1Effect, pos, Quaternion.identity);

            var health = player.GetComponent<vHealthController>();
            if (health != null)
            {
                vDamage damage = new vDamage(skill1Damage);
                health.TakeDamage(damage);
                Debug.Log("Boss dùng Skill1 và gây " + skill1Damage + " damage");
            }
        }
    }

    // -------------------------------
    // SKILL 2: Gọi animation lặp lại khi máu ≤ 30
    // -------------------------------
    public void StartSpamSkill2()
    {
        if (!isSpammingSkill2)
            StartCoroutine(SpamSkill2Coroutine());
    }

    IEnumerator SpamSkill2Coroutine()
    {
        isSpammingSkill2 = true;
        Animator anim = GetComponent<Animator>();
        vHealthController health = GetComponent<vHealthController>();

        while (health != null && !health.isDead)
        {
            yield return new WaitForSeconds(3f); // delay trước mỗi lần dùng skill
            if (anim != null && !health.isDead)
            {
                anim.SetTrigger("SkillStart");
            }
            yield return new WaitForSeconds(skill2Interval);
        }
    }

    // Gọi từ animation event
    public void SpawnSkill2()
    {
        // Nếu object đang bị destroy hoặc đã đánh dấu chết, thì không spawn nữa
        if (this == null || gameObject == null) return; // tránh gọi sau khi bị destroy
        if (GetComponent<vHealthController>()?.isDead == true) return;
        if (hasTriggeredDeath) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || skill2Effect == null) return;

        float radius = 10f;
        int effectCount = 5;

        for (int i = 0; i < effectCount; i++)
        {
            float angle = i * (360f / effectCount) + Random.Range(-15f, 15f);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            Vector3 spawnPos = player.transform.position + offset;

            Instantiate(skill2Effect, spawnPos, Quaternion.identity);
        }

        Debug.Log("Boss dùng Skill2!");
    }

    void Update()
    {
        var health = GetComponent<vHealthController>();

        // Kích hoạt skill2 khi máu yếu
        if (health != null && !hasEnteredSkillMode && health.currentHealth <= 30 && !health.isDead)
        {
            hasEnteredSkillMode = true;
            EnterSkill2Mode();
        }

        // Chết khi máu = 0
        if (health != null && health.currentHealth <= 0 && !hasTriggeredDeath)
        {
            hasTriggeredDeath = true;
            var anim = GetComponent<Animator>();
            if (anim != null) anim.SetBool("IsDead", true);

            var trigger = GetComponent<EnemyDeathTrigger>();
            if (trigger != null) trigger.TriggerDeath();

            Destroy(gameObject, 3f); // tự hủy sau 3s
        }
    }

    void EnterSkill2Mode()
    {
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        var anim = GetComponent<Animator>();
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);

        anim.SetTrigger("SkillStart"); 
        //StartSpamSkill2();             
        Debug.Log("Boss bắt đầu spam Skill2!");
    }
}
