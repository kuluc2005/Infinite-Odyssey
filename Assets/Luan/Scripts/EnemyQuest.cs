using UnityEngine;

public class EnemyQuestNotifier : MonoBehaviour
{
    public string enemyID = "Enemy";

    void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.UpdateQuestObjective(ObjectiveType.KillEnemy, enemyID, 1);
            Debug.Log($"[EnemyQuestNotifier] Đã cập nhật nhiệm vụ kill: {enemyID}");
        }
    }
}
