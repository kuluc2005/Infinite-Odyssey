using UnityEngine;

public class OrbitalFireballs : MonoBehaviour
{
    public Transform targetPlayer;
    public float radius = 2f;
    public float rotateSpeed = 50f;
    public float heightOffset = 1f;
    public float homingSpeed = 5f;
    public int damage = 20;


    public float floatAmplitude = 0.3f; // Biên độ nhấp nhô 
    public float floatFrequency = 2f;   // Tốc độ nhấp nhô
    private float baseHeightOffset;
    private float floatTimeOffset; // để lệch pha mỗi quả cầu




    private float angle;
    private bool isFlying = false;
    private Transform targetEnemy;

    public void Init(Transform player, float startAngle, float radius, float height = 1f)
    {
        targetPlayer = player;
        this.radius = radius;
        heightOffset = height;
        baseHeightOffset = height;
        angle = startAngle;
        floatTimeOffset = Random.Range(0f, Mathf.PI * 2f); 
        UpdatePosition();
    }


    void Update()
    {
        if (isFlying && targetEnemy != null)
        {
            Debug.Log("✈️ Quả cầu đang bay tới: " + targetEnemy.name);
            Vector3 dir = (targetEnemy.position + Vector3.up * 1f - transform.position).normalized;
            transform.position += dir * homingSpeed * Time.deltaTime;
        }
        else if (targetPlayer != null)
        {
            angle += rotateSpeed * Time.deltaTime;
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;

        float yOffset = baseHeightOffset + Mathf.Sin(Time.time * floatFrequency + floatTimeOffset) * floatAmplitude;

        offset.y = yOffset;

        transform.position = targetPlayer.position + offset;
    }


    public void FlyToTarget(Transform enemy)
    {
        targetEnemy = enemy;
        isFlying = true;
        Debug.Log("🔥 Quả cầu nhận mục tiêu: " + enemy.name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Invector.vHealthController>();
            if (health != null)
            {
                Invector.vDamage damage = new Invector.vDamage
                {
                    damageValue = this.damage,
                    sender = transform,
                    hitPosition = transform.position,
                    hitReaction = true
                };

                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
