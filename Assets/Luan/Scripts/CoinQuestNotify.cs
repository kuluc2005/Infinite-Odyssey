using UnityEngine;

public class CoinQuestNotify : MonoBehaviour
{
    public string itemID = "14";
    public void NotifyQuest()
    {
        QuestManager.instance?.UpdateQuestObjective(ObjectiveType.CollectItem, itemID, 1);
        FindFirstObjectByType<InventorySyncManager>()?.SaveInventoryToServer();
    }
}