using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }

    public Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
    public List<QuestData> completedQuests = new List<QuestData>();

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

        // ✅ Reset tất cả progress trước khi bắt đầu
        foreach (var obj in quest.objectives)
        {
            obj.currentAmount = 0;
        }

        activeQuests.Add(quest.questID, quest);
        Debug.Log($"[QuestManager] Bắt đầu nhiệm vụ: {quest.questName}");

        QuestUI ui = FindObjectOfType<QuestUI>();
        if (ui != null)
        {
            ui.UpdateQuestText();
        }
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
                    Debug.Log($"[QuestManager] Đã cập nhật mục tiêu: {obj.objectiveDescription} ({obj.currentAmount}/{obj.requiredAmount})");

                    if (obj.IsCompleted())
                    {
                        Debug.Log($"[QuestManager] Mục tiêu hoàn thành: {obj.objectiveDescription}");
                        CheckQuestCompletion(quest);
                    }

                    // Cập nhật UI sau khi mỗi cập nhật
                    QuestUI ui = FindObjectOfType<QuestUI>();
                    if (ui != null)
                    {
                        ui.UpdateQuestText();
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

        Debug.Log($"[QuestManager] Đã hoàn thành nhiệm vụ: {quest.questName}");

        // Mở cổng nếu có portal tương ứng (tên phải khớp)
        GameObject portal = GameObject.Find($"Portal_{quest.questID}");
        if (portal != null)
        {
            portal.SetActive(true);
            Debug.Log("[QuestManager] Đã mở cổng: " + portal.name);
        }

        // Cập nhật UI
        QuestUI ui = FindObjectOfType<QuestUI>();
        if (ui != null)
        {
            ui.ShowSuccess();
        }

        // Ghi log phần thưởng
        Debug.Log($"[QuestManager] Thưởng: {quest.goldReward} vàng, +{quest.healthIncreaseReward} HP, +{quest.damageIncreaseReward} sát thương.");
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
}
