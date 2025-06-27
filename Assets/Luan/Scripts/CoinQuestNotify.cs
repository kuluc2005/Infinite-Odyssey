using UnityEngine;

public class CoinQuestNotify : MonoBehaviour
{
    public string itemID = "14";

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.UpdateQuestObjective(ObjectiveType.CollectItem, "14", 1);
        }
    }
}
