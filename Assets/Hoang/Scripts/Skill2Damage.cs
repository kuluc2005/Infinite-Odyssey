using UnityEngine;

public class Skill2Damage : MonoBehaviour
{
    public GameObject hitVFXPrefab;
    public float skillDuration = 3f;

    void Start()
    {
        Destroy(gameObject, skillDuration); // skill tự huỷ sau X giây
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Hiệu ứng trúng đòn (liên tục mỗi frame)
            Instantiate(hitVFXPrefab, other.transform.position, Quaternion.identity);
        }
    }
}
