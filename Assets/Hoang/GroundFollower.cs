using UnityEngine;

public class GroundFollower : MonoBehaviour
{
    public LayerMask groundLayer;
    public float groundOffset = 0.0f;
    public float rayDistance = 5f;

    void Update()
    {
        Vector3 start = transform.position + Vector3.up;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 2f))
        {
            Debug.Log("Hit: " + hit.collider.name + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
        }
    }

}

