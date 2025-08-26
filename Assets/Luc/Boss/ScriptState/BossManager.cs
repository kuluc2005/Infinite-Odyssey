using UnityEngine;
using Invector;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public GameObject skill1Effect;
    public GameObject skill2Effect;
    public int skill1Damage = 25;
    public int skill2Damage = 50;
    public float skill2Interval = 6f;

    private int basicAttackCount = 0;
    private int skill1UsageCount = 0;
    private bool isSkill2Active = false;
    private bool hasTriggeredDeath = false;
    private bool isUsingSkill1 = false;

    private Animator animator;
    private vHealthController health;
    private NavMeshAgent agent;

    vHealthController[] GetAlivePlayers()
    {
        var gos = GameObject.FindGameObjectsWithTag("Player");
        var list = new List<vHealthController>();
        foreach (var go in gos)
        {
            var hp = go.GetComponentInParent<vHealthController>();
            if (hp != null && !hp.isDead) list.Add(hp);
        }
        return list.ToArray();
    }

    vHealthController GetNearestPlayer()
    {
        var players = GetAlivePlayers();
        vHealthController best = null;
        float bestDist = float.MaxValue;
        foreach (var hp in players)
        {
            float d = (hp.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = hp; }
        }
        return best;
    }

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
            Destroy(gameObject, 3f);
        }
    }

    public void CountBasicAttack()
    {
        if (isUsingSkill1)
        {
            isUsingSkill1 = false;
            return;
        }

        basicAttackCount++;

        if (basicAttackCount >= 3)
        {
            basicAttackCount = 0;

            if (skill1UsageCount < 2)
            {
                isUsingSkill1 = true;
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

        var target = GetNearestPlayer();
        if (target)
        {
            if (skill1Effect)
                Instantiate(skill1Effect, target.transform.position, Quaternion.identity);
            target.TakeDamage(new vDamage(skill1Damage));
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

        var target = GetNearestPlayer();
        if (target)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < 10f)
            {
                animator.SetBool("isChasing", true);
                animator.SetBool("isAttacking", true);
                agent.SetDestination(target.transform.position);
            }
        }

    }

    public void SpawnSkill2()
    {
        if (health.isDead) return;

        var players = GetAlivePlayers();
        if (players.Length == 0 || !skill2Effect) return;

        float radius = 10f;
        int effectCount = 3;

        foreach (var hp in players)
        {
            for (int i = 0; i < effectCount; i++)
            {
                float angle = i * (360f / effectCount) + Random.Range(-15f, 15f);
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                Vector3 pos = hp.transform.position + offset;
                Instantiate(skill2Effect, pos, Quaternion.identity);
            }
        }
    }

}
