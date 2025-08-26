using UnityEngine;
using Invector;

public class Skill2DamageTrigger : MonoBehaviour
{
    public int damageAmount = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<vHealthController>();
            if (health != null)
            {
                vDamage damage = new vDamage(damageAmount);
                health.TakeDamage(damage);
                Debug.Log("Skill2 HIT Player! Gây " + damageAmount + " damage");

                Destroy(gameObject);
            }
        }
    }
}
