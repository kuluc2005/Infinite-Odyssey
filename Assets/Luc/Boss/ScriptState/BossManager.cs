using UnityEngine;
using Invector;
using UnityEngine.AI;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public GameObject skill1Effect;
    public GameObject skill2Effect;
    public int skill1Damage = 25;
    public int skill2Damage = 50;
    public float skill2Interval = 6f;
    public int expReward = 100;

    private int basicAttackCount = 0;
    private int skill1UsageCount = 0;
    private bool isSkill2Active = false;
    private bool hasTriggeredDeath = false;
    private bool isUsingSkill1 = false;

    private Animator animator;
    private vHealthController health;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<vHealthController>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (health == null) return;

        if (health.currentHealth <= 0 && !hasTriggeredDeath)
        {
            hasTriggeredDeath = true; 
            animator.SetBool("IsDead", true);
            RewardExpToPlayer();
            Destroy(gameObject, 3f);
        }
    }


    public void CountBasicAttack()
    {
        // Nếu vừa dùng Skill1 thì không tăng
        if (isUsingSkill1)
        {
            isUsingSkill1 = false; // reset lại cho lần sau
            return;
        }

        basicAttackCount++;

        if (basicAttackCount >= 3)
        {
            basicAttackCount = 0;

            if (skill1UsageCount < 2)
            {
                isUsingSkill1 = true; // đánh dấu đã gọi skill1
                animator.SetTrigger("Attack1");
            }
            else if (!isSkill2Active)
            {
                StartCoroutine(UseSkill2Once());
            }
        }
    }


    public void SpawnSkill1()
    {
        if (health.isDead) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player && skill1Effect)
        {
            Instantiate(skill1Effect, player.transform.position, Quaternion.identity);
            var hp = player.GetComponent<vHealthController>();
            if (hp != null)
            {
                hp.TakeDamage(new vDamage(skill1Damage));
            }
        }

        skill1UsageCount++;
    }

    IEnumerator UseSkill2Once()
    {
        isSkill2Active = true;

        if (agent) agent.enabled = false;
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.ResetTrigger("Attack1");
        animator.SetTrigger("SkillStart");

        yield return new WaitForSeconds(1.5f);


        isSkill2Active = false;
        skill1UsageCount = 0;
        animator.ResetTrigger("SkillStart");

        if (agent) agent.enabled = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < 10f)
            {
                animator.SetBool("isChasing", true);
                animator.SetBool("isAttacking", true);
                agent.SetDestination(player.transform.position);
            }
        }
    }

    public void SpawnSkill2()
    {
        if (health.isDead) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player || !skill2Effect) return;

        float radius = 10f;
        int effectCount = 5;

        for (int i = 0; i < effectCount; i++)
        {
            float angle = i * (360f / effectCount) + Random.Range(-15f, 15f);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Vector3 pos = player.transform.position + offset;
            Instantiate(skill2Effect, pos, Quaternion.identity);
        }
    }

    void RewardExpToPlayer()
    {
        var playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddExp(expReward);
        }
    }
}
