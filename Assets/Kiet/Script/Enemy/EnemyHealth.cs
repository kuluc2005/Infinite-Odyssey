using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public Slider healthSlider;

    [Header("Animator Triggers")]
    public string hitTrigger = "hit_1";
    public string deathTrigger = "death";

    int current;
    Animator anim;
    NavMeshAgent agent;
    Collider[] cols;
    EnemyZoneRegister register;

    void Awake()
    {
        current = maxHealth;
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cols = GetComponentsInChildren<Collider>(true);
        register = GetComponent<EnemyZoneRegister>();

        if (healthSlider)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (current <= 0) return;

        current -= amount;
        current = Mathf.Clamp(current, 0, maxHealth);

        if (healthSlider)
            healthSlider.value = current;

        if (current > 0)
        {
            if (anim) anim.SetTrigger(hitTrigger);
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        current = 0;
        if (anim) anim.SetTrigger(deathTrigger);

        if (agent) { agent.isStopped = true; agent.ResetPath(); agent.enabled = false; }
        foreach (var c in cols) if (c) c.enabled = false;

        if (register) register.NotifyDeath();
        Destroy(gameObject, 1f);
    }
}

public interface IDamageable { void TakeDamage(int amount); }
