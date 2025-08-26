using System.Collections.Generic;
using UnityEngine;

public class EnemyHUDManager : MonoBehaviour
{
    public static EnemyHUDManager Instance;

    [Header("HUD Prefab")]
    public GameObject enemyHUDPrefab;

    [Header("Vùng chứa HUD")]
    public RectTransform hudParent;

    [Header("Khoảng cách giữa các HUD")]
    public float verticalSpacing = 60f;

    private List<EnemyUIController> activeEnemies = new List<EnemyUIController>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy(EnemyUIController enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            RearrangeHUD();
        }
    }

    public void UnregisterEnemy(EnemyUIController enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            RearrangeHUD();
        }
    }

    void RearrangeHUD()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var enemy = activeEnemies[i];
            if (enemy != null && enemy.hudInstance != null)
            {
                RectTransform rect = enemy.hudInstance.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -i * verticalSpacing);
            }
        }
    }
}
