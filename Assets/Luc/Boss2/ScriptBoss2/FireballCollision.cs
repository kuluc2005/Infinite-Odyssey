using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    public GameObject explosionEffectPrefab;

    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Ground"))
        {
            hasExploded = true;

            if (explosionEffectPrefab != null)
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            Invoke(nameof(DestroySelf), 0.2f);
        }
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
