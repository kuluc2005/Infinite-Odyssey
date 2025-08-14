using UnityEngine;

public class EnemyZoneRegister : MonoBehaviour
{
    public EnemySpawner1 spawner; // gán khi spawn

    public void NotifyDeath()
    {
        if (spawner) spawner.OnEnemyKilled(gameObject);
    }
}
