using UnityEngine;
using Invector;

public class EnemyReward : MonoBehaviour
{
    [Header("Reward Settings")]
    public int expReward = 50;     //EXP nhận được khi giết enemy
    public int goldReward = 10;    //Vàng nhận được khi giết enemy

    private vHealthController healthController;
    private bool rewardGiven = false;

    void Start()
    {
        healthController = GetComponent<vHealthController>();

        if (healthController != null)
        {
            healthController.onDead.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning($"{name} không có vHealthController, EnemyReward sẽ không hoạt động!");
        }
    }

    void OnEnemyDeath(GameObject sender)
    {
        if (rewardGiven) return; //Đảm bảo chỉ thưởng 1 lần
        rewardGiven = true;

        //Thưởng EXP cho người chơi
        var playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddExp(expReward);
            Debug.Log($"Player nhận {expReward} EXP từ {name}");
        }

        // Thưởng vàng cho người chơi (sử dụng GoldManager)
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
