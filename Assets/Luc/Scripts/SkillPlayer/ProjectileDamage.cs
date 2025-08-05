using UnityEngine;
using Invector;

public class ProjectileDamage : MonoBehaviour
{
    public int damage = 300;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<vHealthController>();
            if (health != null)
            {
                vDamage dmg = new vDamage
                {
                    damageValue = damage,
                    sender = transform,
                    hitPosition = transform.position,
                    hitReaction = true
                };
                health.TakeDamage(dmg);
            }

            Destroy(gameObject);
        }
    }
}
