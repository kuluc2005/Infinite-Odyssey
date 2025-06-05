using UnityEngine;
using Invector;

public class EnemyDamageDealer : MonoBehaviour
{
    public int damageAmount = 20;

    public void DealDamageToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var health = player.GetComponent<vHealthController>();
            if (health != null)
            {
                vDamage damage = new vDamage(damageAmount);
                health.TakeDamage(damage);
                Debug.Log("Enemy dealt damage to player: " + damageAmount);
            }
        }
    }
}
