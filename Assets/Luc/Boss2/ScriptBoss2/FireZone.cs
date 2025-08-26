using UnityEngine;
using Invector;                 

public class FireZone : MonoBehaviour
{
    public float damagePerSecond = 10f;
    public float duration = 10f;

    private void Start()
    {
        Destroy(gameObject, duration); // Tự huỷ sau 10s
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            vHealthController health = other.GetComponent<vHealthController>();
            if (health != null)
            {
                // Tạo gói sát thương theo chuẩn Invector
                vDamage damage = new vDamage
                {
                    damageValue = damagePerSecond * Time.deltaTime
                };

                health.TakeDamage(damage);
            }
        }
    }
}
