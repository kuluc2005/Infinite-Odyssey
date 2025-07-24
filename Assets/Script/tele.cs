using UnityEngine;

public class Tele : MonoBehaviour
{
    public Transform teleportDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Đảm bảo Player có tag "Player"
        {
            other.transform.position = teleportDestination.position;
            Debug.Log("Teleported!");
        }
    }
}
