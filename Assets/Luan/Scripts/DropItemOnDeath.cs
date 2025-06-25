using Invector.vCharacterController;
using UnityEngine;

public class DropItemOnDeath : MonoBehaviour
{
    [Header("Prefab mảnh bản đồ")]
    public GameObject mapPiecePrefab;

    [Header("Vị trí lệch khi spawn")]
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

    void Start()
    {
        var onDead = GetComponent<vOnDeadTrigger>();
        if (onDead != null)
        {
            onDead.OnDead.AddListener(DropMapPiece);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy vOnDeadTrigger trên enemy.");
        }
    }

    void DropMapPiece()
    {
        if (mapPiecePrefab != null)
        {
            Instantiate(mapPiecePrefab, transform.position + spawnOffset, Quaternion.identity);
            Debug.Log("Enemy chết và đã rơi mảnh bản đồ.");
        }
        else
        {
            Debug.LogWarning("Chưa gán prefab mảnh bản đồ.");
        }
    }
}
