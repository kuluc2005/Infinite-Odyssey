using UnityEngine;
using Invector;

public class FireballProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 30;

    private Transform target;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!target) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    
        transform.LookAt(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<vHealthController>();
            if (health != null)
            {
                health.TakeDamage(new vDamage(damage));
            }

            Destroy(gameObject);
        }
    }
}
