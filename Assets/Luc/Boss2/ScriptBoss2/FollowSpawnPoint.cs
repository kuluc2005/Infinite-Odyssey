using UnityEngine;

public class FollowSpawnPoint : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        if (target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}
