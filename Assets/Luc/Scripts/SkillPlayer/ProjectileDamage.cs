using UnityEngine;
using Invector;
using System.Collections;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Base Hit")]
    public int damage = 300;

    [Header("Bonus Hit")]
    public int bonusDamage = 300;                
    public GameObject bonusEffectPrefab;           
    [Tooltip("Nên > 0 để tránh 2 hit trùng frame. 0.08–0.2s là hợp lý.")]
    public float bonusDelay = 0.12f;

    [Header("Cleanup")]
    [Tooltip("Hủy đạn sau khi đã gây xong bonus hit.")]
    public float destroyAfterBonus = 0.05f;

    private bool consumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag("Enemy")) return;

        var health = other.GetComponentInParent<vHealthController>();
        if (health == null) return;

        consumed = true;
        StartCoroutine(HandleTwoHits(health));
    }

    private IEnumerator HandleTwoHits(vHealthController health)
    {
        // -------- Hit 1 --------
        var dmg1 = new vDamage
        {
            damageValue = damage,
            sender = transform,
            hitPosition = health.transform.position,
            hitReaction = true
        };
        health.TakeDamage(dmg1);

        if (bonusEffectPrefab)
            Instantiate(bonusEffectPrefab, health.transform.position, Quaternion.identity);

        if (bonusDelay > 0f)
            yield return new WaitForSeconds(bonusDelay);

        // -------- Hit 2 (bonus) --------
        if (health)
        {
            var dmg2 = new vDamage
            {
                damageValue = bonusDamage,
                sender = transform,
                hitPosition = health.transform.position,
                hitReaction = true
            };
            health.TakeDamage(dmg2);
            // Debug.Log("[Skill1] Bonus Hit applied: " + bonusDamage);
        }

        // -------- Cleanup --------
        if (destroyAfterBonus > 0f)
            yield return new WaitForSeconds(destroyAfterBonus);

        Destroy(gameObject);
    }
}
