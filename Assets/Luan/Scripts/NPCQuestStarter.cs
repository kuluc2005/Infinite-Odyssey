using UnityEngine;

public class NPCQuestStarter : MonoBehaviour
{
    public QuestRunner questRunner;

    public void StartQuestFromTrigger()
    {
        if (questRunner != null && questRunner.quest != null)
        {
            Debug.Log("NPCQuestStarter → Bắt đầu nhiệm vụ từ trigger");
            QuestManager.instance.StartQuest(questRunner.quest);
        }
        else
        {
            Debug.LogWarning("NPCQuestStarter: Thiếu questRunner hoặc quest.");
        }
    }
}
