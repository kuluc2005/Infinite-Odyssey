using UnityEngine;
using Invector;

public class FireBreathDamageTrigger : MonoBehaviour
{
    public int damagePerSecond = 10;
    public float tickInterval = 1f;

    private float timer = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer >= tickInterval)
            {
                var hp = other.GetComponent<vHealthController>();
                if (hp != null)
                {
                    hp.TakeDamage(new vDamage(damagePerSecond));
                }
                timer = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer = 0f; // reset khi rời khỏi vùng
        }
    }
}
