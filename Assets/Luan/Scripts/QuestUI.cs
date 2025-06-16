using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questText;

    void Start()
    {
        UpdateQuestText();
    }

    public void UpdateQuestText()
    {
        if (QuestManager.instance != null)
        {
            int killed = QuestManager.instance.GetEnemiesKilled();
            int total = QuestManager.instance.totalEnemies;

            if (QuestManager.instance.questCompleted)
            {
                questText.text = "Nhiệm vụ: <color=green>Thành công!</color>";
            }
            else
            {
                questText.text = $"Nhiệm vụ:\nTiêu diệt {total} quái vật\n({killed}/{total})";
            }
        }
    }
}
