using UnityEngine;
using Invector.vCharacterController.vActions;
using Invector.vCharacterController;

public class NotifyQuestOnDeath : MonoBehaviour
{
    [Tooltip("ID của quái vật này (dùng để khớp với Objective.targetID)")]
    public string enemyID = "Goblin_01";

    void Start()
    {
        var onDead = GetComponent<vOnDeadTrigger>();
        if (onDead != null)
        {
            // Đảm bảo đúng tên event
            onDead.OnDead.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning("NotifyQuestOnDeath: Không tìm thấy vOnDeadTrigger.");
        }
    }

    public void OnEnemyDeath()
    {
        if (QuestManager.instance != null)
        {
            Debug.Log("NotifyQuestOnDeath → Gọi UpdateQuestObjective");
            QuestManager.instance.UpdateQuestObjective(ObjectiveType.KillEnemy, enemyID);
        }
    }
}
