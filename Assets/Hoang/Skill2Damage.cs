using UnityEngine;

public class Skill2Damage : MonoBehaviour
{
    public GameObject hitVFXPrefab;
    public float skillDuration = 2f;

    void Start()
    {
        Destroy(gameObject, skillDuration); // VFX tự hủy sau X giây
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Hiện hiệu ứng hit liên tục
            Instantiate(hitVFXPrefab, other.transform.position, Quaternion.identity);
        }
    }
}
