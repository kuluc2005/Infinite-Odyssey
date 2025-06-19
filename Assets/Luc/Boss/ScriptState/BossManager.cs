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
    private bool hasEnteredSkillMode = false;
    private bool hasTriggeredDeath = false;

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
        if (health == null || hasTriggeredDeath) return;

        if (health.currentHealth <= 0 && !hasTriggeredDeath)
        {
            hasTriggeredDeath = true;
            animator.SetBool("IsDead", true);

            RewardExpToPlayer();
            Destroy(gameObject, 3f);
        }

        if (health.currentHealth <= 30 && !hasEnteredSkillMode)
        {
            hasEnteredSkillMode = true;
            EnterSkill2Mode();
        }
    }

    public void CountBasicAttack()
    {
        basicAttackCount++;
        if (basicAttackCount >= 3)
        {
            animator.SetTrigger("Attack1");
            basicAttackCount = 0;
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

    void EnterSkill2Mode()
    {
        if (agent) agent.enabled = false;
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("SkillStart");
        StartCoroutine(SpamSkill2());
    }

    IEnumerator SpamSkill2()
    {
        while (!health.isDead)
        {
            yield return new WaitForSeconds(3f);
            animator.SetTrigger("SkillStart");
            yield return new WaitForSeconds(skill2Interval);
        }
    }

    void RewardExpToPlayer()
    {
        var playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddExp(expReward);
            Debug.Log("Player nhận " + expReward + " EXP khi giết boss");
        }
    }
}
