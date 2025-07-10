using UnityEngine;

public class BossAttackEffect : MonoBehaviour
{
    public GameObject leftHandEffect;
    public GameObject rightHandEffect;
    public Transform leftSlashPoint;
    public Transform rightSlashPoint;
    public int damageAmount = 20;

    public void SpawnLeftAttackEffect()
    {
        GameObject fx = Instantiate(leftHandEffect, leftSlashPoint.position, leftSlashPoint.rotation);
        Destroy(fx, 2f);
    }

    public void SpawnRightAttackEffect()
    {
        GameObject fx = Instantiate(rightHandEffect, rightSlashPoint.position, rightSlashPoint.rotation);
        Destroy(fx, 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatss stats = other.GetComponent<PlayerStatss>();
            if (stats != null)
            {
                stats.TakeDamage(damageAmount);
            }
        }
    }
}
