using UnityEngine;
using UnityEngine.Events;

public class QuestRunner : MonoBehaviour
{
    public QuestData quest;
    private int currentKillCount = 0;
    private bool questFinished = false;

    // Gọi hàm này mỗi khi enemy bị tiêu diệt
    public void OnEnemyKilled(string enemyTag)
    {
        if (quest == null || questFinished) return;

        // So sánh tag của quái vật với mục tiêu của quest
        if (!string.IsNullOrEmpty(quest.targetTag) && enemyTag == quest.targetTag)
        {
            currentKillCount++;
            Debug.Log($"Đã tiêu diệt: {currentKillCount}/{quest.targetKillCount}");

            if (currentKillCount >= quest.targetKillCount)
            {
                CompleteQuest();
            }
        }
    }

    private void CompleteQuest()
    {
        questFinished = true;
        Debug.Log($"Quest Completed: {quest.questName}");

        // Nếu có hệ thống phần thưởng hoặc UI khác thì có thể thêm vào đây

        // Gọi sự kiện hoàn thành nếu có
        QuestManager.instance.CompleteQuest(quest);
    }
}
