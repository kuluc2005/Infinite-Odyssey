using UnityEngine;
using Invector.vItemManager;
using System.Linq;

public class QuestNPC : MonoBehaviour
{
    public QuestData questData;

    [Tooltip("ID vật phẩm cần để hoàn thành nhiệm vụ")]
    public string requiredItemID = "14";

    private bool playerInRange = false;
    private vItemManager playerInventory;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!QuestManager.instance.IsQuestActive(questData.questID))
            {
                // Nhận nhiệm vụ lần đầu
                QuestManager.instance.StartQuest(questData);
                Debug.Log("[NPC] Đã nhận nhiệm vụ từ NPC.");
            }
            else if (!QuestManager.instance.IsQuestCompleted(questData.questID))
            {
                if (HasEnoughItems())
                {
                    //RemoveRequiredItems();
                    QuestManager.instance.UpdateQuestObjective(ObjectiveType.CollectItem, requiredItemID, GetItemCount());
                    Debug.Log("[NPC] ✅ Đã giao đủ vật phẩm nhiệm vụ.");
                }
                else
                {
                    Debug.Log("[NPC] ❌ Chưa đủ vật phẩm. Cần thêm " + GetRequiredAmount() + " đồng vàng.");
                }
            }
            else
            {
                Debug.Log("[NPC] Nhiệm vụ đã hoàn thành trước đó.");
            }
        }
    }

    bool HasEnoughItems()
    {
        return GetItemCount() >= GetRequiredAmount();
    }

    int GetItemCount()
    {
        if (playerInventory == null) return 0;

        var item = playerInventory.items.FirstOrDefault(i => i != null && i.id.ToString() == requiredItemID);
        return item != null ? item.amount : 0;
    }

    int GetRequiredAmount()
    {
        var obj = questData.objectives.FirstOrDefault(o => o.type == ObjectiveType.CollectItem && o.targetID == requiredItemID);
        return obj != null ? obj.requiredAmount : 1;
    }

    //void RemoveRequiredItems()
    //{
    //    if (playerInventory == null) return;

    //    var item = playerInventory.items.FirstOrDefault(i => i != null && i.id.ToString() == requiredItemID);
    //    int removeAmount = GetRequiredAmount();

    //    if (item != null && item.amount >= removeAmount)
    //    {
    //        for (int i = 0; i < removeAmount; i++)
    //        {
    //            playerInventory.RemoveItem(item, true); // xóa từng item 1
    //        }

    //        Debug.Log($"[NPC] Đã xóa {removeAmount} vật phẩm ID {requiredItemID} khỏi túi.");
    //    }
    //}

    // QuestNPC.cs
    void OnTriggerEnter(Collider other)
    {
        var inv = other.GetComponentInParent<vItemManager>();
        if (inv != null) 
        {
            playerInRange = true;
            playerInventory = inv;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var inv = other.GetComponentInParent<vItemManager>();
        if (inv != null)
        {
            playerInRange = false;
            playerInventory = null;
        }
    }
}
