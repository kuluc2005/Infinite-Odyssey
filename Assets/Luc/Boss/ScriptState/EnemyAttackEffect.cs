using UnityEngine;
using Invector;

public class EnemyAttackEffect : MonoBehaviour
{
    public GameObject attackEffectPrefab;
    public int skillDamage = 50; // damage 

    public void SpawnAttackEffect()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && attackEffectPrefab != null)
        {
            Vector3 spawnPos = player.transform.position;

            // hieu ung xuat hien tai vi tri cua player
            Instantiate(attackEffectPrefab, spawnPos, Quaternion.identity);

            // 2. gay damage cho player
            var health = player.GetComponent<vHealthController>();
            if (health != null)
            {
                vDamage damage = new vDamage(skillDamage);
                health.TakeDamage(damage);
                Debug.Log("Enemy used SKILL and dealt " + skillDamage + " damage to Player");
            }
        }
    }
}
