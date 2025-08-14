using UnityEngine;

public class EnemyZoneRegister : MonoBehaviour
{
    public EnemySpawner1 spawner; // g�n khi spawn

    public void NotifyDeath()
    {
        if (spawner) spawner.OnEnemyKilled(gameObject);
    }
}
