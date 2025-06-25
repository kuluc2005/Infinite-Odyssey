using UnityEngine;
using TMPro;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [Header("UI hiện nhiệm vụ")]
    public TextMeshProUGUI questText;

    [Tooltip("ID nhiệm vụ cần hiển thị (có thể để trống để tự lấy quest đầu tiên)")]
    public string questID = "";

    void Start()
    {
        // Nếu không chỉ định questID → lấy quest đang active đầu tiên (nếu có)
        if (string.IsNullOrEmpty(questID) && QuestManager.instance != null && QuestManager.instance.activeQuests.Count > 0)
        {
            questID = QuestManager.instance.activeQuests.First().Key;
        }

        UpdateQuestText();
    }

    public void UpdateQuestText()
    {
        if (QuestManager.instance == null || string.IsNullOrEmpty(questID))
        {
            questText.text = "Chưa có nhiệm vụ.";
            return;
        }

        var quest = QuestManager.instance.GetActiveQuest(questID);

        if (quest != null && !QuestManager.instance.IsQuestCompleted(questID))
        {
            // ✅ Hiển thị đơn giản, giống mẫu viết tay
            string progressText = $"<b>Nhiệm vụ:</b> {quest.questName}\n";
            foreach (var obj in quest.objectives)
            {
                progressText += $"{obj.currentAmount}/{obj.requiredAmount}";
            }

            questText.text = progressText;
        }
        else if (QuestManager.instance.IsQuestCompleted(questID))
        {
            questText.text = "<b>Nhiệm vụ:</b> <color=green>Thành công!</color>";
        }
        else
        {
            questText.text = "Chưa nhận nhiệm vụ.";
        }
    }


    public void ShowSuccess()
    {
        questText.text = "<color=green><b>Nhiệm vụ hoàn thành!</b></color>";
    }
}
