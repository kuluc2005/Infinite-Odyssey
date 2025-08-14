using UnityEngine;
using Invector;

public class EnemyHUDAutoRemover : MonoBehaviour
{
    private vHealthController health;
    private EnemyUIController enemyUI;
    private bool hasRemovedHUD = false;

    void Start()
    {
        health = GetComponent<vHealthController>();
        enemyUI = GetComponent<EnemyUIController>();

        if (health == null || enemyUI == null)
        {
            Debug.LogWarning("EnemyHUDAutoRemover thiếu vHealthController hoặc EnemyUIController!");
            enabled = false;
        }
    }

    void Update()
    {
        if (hasRemovedHUD || health == null || enemyUI == null) return;

        if (health.currentHealth <= 0)
        {
            RemoveHUD();
            hasRemovedHUD = true;
        }
    }

    void RemoveHUD()
    {
        if (EnemyHUDManager.Instance != null)
        {
            EnemyHUDManager.Instance.UnregisterEnemy(enemyUI);
            Debug.Log($"Đã gỡ HUD của enemy: {enemyUI.name}");

            if (enemyUI.hudInstance != null)
            {
                Destroy(enemyUI.hudInstance);
            }
        }
    }
}
