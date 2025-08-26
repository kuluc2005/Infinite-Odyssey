using UnityEngine;
using Invector;

public class EnemyReward : MonoBehaviour
{
    [Header("Reward Settings")]
    public int expReward = 50;
    public int goldReward = 10;

    private vHealthController healthController;
    private bool rewardGiven = false;

    void Start()
    {
        healthController = GetComponent<vHealthController>();
        if (healthController != null) healthController.onDead.AddListener(OnEnemyDeath);
        else Debug.LogWarning($"{name} không có vHealthController, EnemyReward sẽ không hoạt động!");
    }

    void OnEnemyDeath(GameObject sender)
    {
        if (rewardGiven) return;
        rewardGiven = true;

        // Tìm PlayerStats chắc kèo (kể cả khi không nằm trên root có Tag)
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) playerStats = go.GetComponentInChildren<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.AddExp(expReward);
            Debug.Log($"Player nhận {expReward} EXP từ {name}");
        }
        else
        {
            Debug.LogWarning("[EnemyReward] Không tìm thấy PlayerStats trong scene!");
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddCoins(goldReward);
            Debug.Log($"Player nhận {goldReward} vàng từ {name}");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GoldManager, không cộng vàng được!");
        }
    }
}
