using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("UI hiện nhiệm vụ")]
    public TextMeshProUGUI questText;

    [Tooltip("ID của nhiệm vụ cần hiển thị")]
    public string questID = "Quest_Man1_TimDaoSi"; // ← Bạn có thể đổi trong Inspector
 

    void Start()
    {
        UpdateQuestText();
    }

    void Update()
    {
        // Tự động cập nhật mỗi frame – hoặc có thể gọi thủ công nếu muốn tối ưu
        UpdateQuestText();
    }

    public void UpdateQuestText()
    {
        if (QuestManager.instance == null || string.IsNullOrEmpty(questID))
        {
            questText.text = "Không có nhiệm vụ nào.";
            return;
        }

        var quest = QuestManager.instance.GetActiveQuest(questID);

        if (quest != null && !QuestManager.instance.IsQuestCompleted(questID))
        {
            // Hiển thị tiến độ từng objective
            string progressText = "Nhiệm vụ:\n";
            foreach (var obj in quest .objectives)
            {
                progressText += $"- {obj.objectiveDescription}: {obj.currentAmount}/{obj.requiredAmount}\n";
            }

            questText.text = progressText;
        }
        else if (QuestManager.instance.IsQuestCompleted(questID))
        {
            questText.text = "Nhiệm vụ: <color=green>Thành công!</color>";
        }
        else
        {
            questText.text = "Chưa nhận nhiệm vụ.";
        }
    }

    public void ShowSuccess()
    {
        questText.text = "Nhiệm vụ: <color=green>Thành công!</color>";
    }
}
