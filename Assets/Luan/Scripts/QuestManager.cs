using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }

    public Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
    public List<QuestData> completedQuests = new List<QuestData>();

    [Header("Quest giết quái")]
    public int totalEnemies = 3;
    private int enemiesKilled = 0;
    public bool questCompleted = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartQuest(QuestData quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.questID)) return;

        activeQuests.Add(quest.questID, quest);
        Debug.Log($"Quest Started: {quest.questName}");
    }

    public void UpdateQuestObjective(ObjectiveType type, string targetID, int amount = 1)
    {
        foreach (var quest in activeQuests.Values)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && obj.targetID == targetID && !obj.IsCompleted())
                {
                    obj.currentAmount += amount;
                    Debug.Log($"Objective Updated: {obj.objectiveDescription} ({obj.currentAmount}/{obj.requiredAmount})");

                    if (obj.IsCompleted())
                    {
                        Debug.Log($"Objective Completed: {obj.objectiveDescription}");
                        CheckQuestCompletion(quest);
                    }

                    return;
                }
            }
        }
    }

    private void CheckQuestCompletion(QuestData quest)
    {
        bool allCompleted = quest.objectives.All(obj => obj.IsCompleted());

        if (allCompleted)
        {
            CompleteQuest(quest);
        }
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!activeQuests.ContainsKey(quest.questID)) return;

        activeQuests.Remove(quest.questID);
        completedQuests.Add(quest);
        Debug.Log($"Quest Completed: {quest.questName}");

        // Hiện cổng nếu là nhiệm vụ chính
        if (quest.questID == "Quest_Man1_TimDaoSi")
        {
            GameObject portal = GameObject.Find("Portal_Man1_To_Man2");
            if (portal != null)
            {
                portal.SetActive(true);
            }
        }

        // Cập nhật UI khi hoàn thành nhiệm vụ
        QuestUI ui = FindObjectOfType<QuestUI>();
        if (ui != null)
        {
            ui.UpdateQuestText();
        }

        // Thưởng (nếu có)
        Debug.Log($"Applied Rewards: {quest.goldReward} Gold, +{quest.healthIncreaseReward} HP, +{quest.damageIncreaseReward} Damage.");
    }

    public void EnemyKilled()
    {
        enemiesKilled++;

        QuestUI ui = FindObjectOfType<QuestUI>();
        if (ui != null)
        {
            ui.UpdateQuestText();
        }

        if (enemiesKilled >= totalEnemies)
        {
            questCompleted = true;
            Debug.Log("Nhiệm vụ hoàn thành!");

            if (ui != null)
            {
                ui.UpdateQuestText();
            }
        }
    }

    public QuestData GetActiveQuest(string questID)
    {
        activeQuests.TryGetValue(questID, out QuestData quest);
        return quest;
    }

    public bool IsQuestActive(string questID)
    {
        return activeQuests.ContainsKey(questID);
    }

    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Any(q => q.questID == questID);
    }

    public int GetEnemiesKilled() => enemiesKilled;
    public bool IsQuestCompletedByEnemyKill() => questCompleted;
}
