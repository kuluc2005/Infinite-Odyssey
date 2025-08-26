using UnityEngine;
using Invector;

public class EnemyDamageDealer : MonoBehaviour
{
    [Header("Damage")]
    public int damageAmount = 20;

    [Header("Hit Area (trước mặt boss)")]
    public Transform hitOrigin;         
    public float hitRange = 2.8f;      
    public float hitRadius = 1.2f;     
    public LayerMask playerHitMask = ~0; 

    public void DealDamageToPlayer()
    {
        Vector3 origin = hitOrigin ? hitOrigin.position : (transform.position + Vector3.up * 1f);

        var center = origin + transform.forward * (hitRange * 0.5f);
        var hits = Physics.OverlapSphere(center, hitRadius, playerHitMask, QueryTriggerInteraction.Ignore);

        bool didAny = false;

        foreach (var col in hits)
        {
            if (!col || !col.CompareTag("Player")) continue;

            var health = col.GetComponentInParent<vHealthController>();
            if (health != null && !health.isDead)
            {
                var dmg = new vDamage(damageAmount)
                {
                    sender = transform,
                    hitPosition = col.ClosestPoint(origin),
                    hitReaction = true
                };
                health.TakeDamage(dmg);
                didAny = true;
            }
        }

        if (!didAny)
        {
            var nearest = FindNearestAlivePlayer();
            if (nearest != null)
            {
                float dist = Vector3.Distance(nearest.transform.position, origin);
                if (dist <= hitRange + hitRadius)
                {
                    var dmg = new vDamage(damageAmount)
                    {
                        sender = transform,
                        hitPosition = nearest.transform.position,
                        hitReaction = true
                    };
                    nearest.TakeDamage(dmg);
                }
            }
        }
    }

    // ===== Helpers =====
    private vHealthController FindNearestAlivePlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        vHealthController best = null;
        float bestDist = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var go in players)
        {
            if (!go) continue;
            var hp = go.GetComponentInParent<vHealthController>();
            if (hp == null || hp.isDead) continue;

            float d = (hp.transform.position - p).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = hp;
            }
        }
        return best;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = hitOrigin ? hitOrigin.position : (transform.position + Vector3.up * 1f);
        Vector3 center = origin + transform.forward * (hitRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
#endif
}
