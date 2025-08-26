using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireballProjectileplayer : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    public float damage = 50f;

    public string enemyTag = "Enemy";

    Rigidbody rb;
    Vector3 moveDir;
    Transform sender; 

    public void Init(Vector3 direction, float spd, float dmg, Transform owner)
    {
        moveDir = direction.normalized;
        speed = spd;
        damage = dmg;
        sender = owner;

        if (!rb) rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = moveDir * speed;
        }

        // Hướng nhìn của mesh
        transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

        Destroy(gameObject, lifeTime);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Nếu chưa Init (spawn trực tiếp trong scene), vẫn tự chạy thẳng theo forward hiện tại
        if (moveDir == Vector3.zero) moveDir = transform.forward;
    }

    void Update()
    {
        if (!rb)
        {
            // Không có Rigidbody thì tự Translate
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Bỏ qua va chạm với người bắn
        if (sender && other.transform == sender) return;

        // Chỉ xử lý đụng Enemy
        if (!string.IsNullOrEmpty(enemyTag) && !other.CompareTag(enemyTag)) return;

        // Gây damage theo Invector
        var health = other.GetComponent<Invector.vHealthController>();
        if (health)
        {
            var dmg = new Invector.vDamage
            {
                damageValue = Mathf.RoundToInt(damage),
                sender = sender ? sender : transform,
                hitPosition = other.bounds.center,
                hitReaction = true
            };
            health.TakeDamage(dmg);
        }

        // Nổ/huỷ
        Destroy(gameObject);
    }
}
