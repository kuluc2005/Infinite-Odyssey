using UnityEngine;
using TMPro;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [Header("UI hiện nhiệm vụ")]
    public TextMeshProUGUI questText;

    private string currentQuestID = "";

    void Start()
    {
        InvokeRepeating(nameof(CheckAndUpdateQuestUI), 0.5f, 1.0f);
    }

    void CheckAndUpdateQuestUI()
    {
        if (QuestManager.instance == null || QuestManager.instance.activeQuests.Count == 0)
        {
            questText.text = ""; // Không hiển thị gì nếu chưa có nhiệm vụ
            return;
        }

        // Luôn lấy quest đầu tiên đang hoạt động
        currentQuestID = QuestManager.instance.activeQuests.First().Key;

        UpdateQuestText(currentQuestID);
    }

    void UpdateQuestText(string questID)
    {
        var quest = QuestManager.instance.GetActiveQuest(questID);

        if (quest != null)
        {
            string progressText = $"<b>Nhiệm vụ:</b> {quest.questName}\n";
            foreach (var obj in quest.objectives)
            {
                progressText += $"- {obj.objectiveDescription}: {obj.currentAmount}/{obj.requiredAmount}\n";
            }

            bool readyToComplete = quest.objectives.All(obj => obj.currentAmount >= obj.requiredAmount);
            if (readyToComplete)
            {
                progressText += "<color=yellow>Enough items! Go return the quest to the NPC.</color>\n";
            }

            questText.text = progressText;
        }
        else
        {
            questText.text = "";
        }
    }


    public void ShowSuccess()
    {
        questText.text = "<color=green><b>Mission accomplished!</b></color>";
    }
}