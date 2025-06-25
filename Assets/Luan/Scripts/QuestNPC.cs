using UnityEngine;
using Invector.vItemManager;
using System.Linq;

public class QuestNPC : MonoBehaviour
{
    [Header("Thông tin nhiệm vụ")]
    public QuestData questData;

    [Tooltip("ID của item yêu cầu trong ItemListData (ví dụ: 13 cho Map)")]
    public int requiredItemID = 13;

    private bool playerInRange = false;
    private vItemManager playerInventory;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (questData == null || QuestManager.instance == null) return;

            // 1. Nhận nhiệm vụ nếu chưa nhận
            if (!QuestManager.instance.IsQuestActive(questData.questID))
            {
                QuestManager.instance.StartQuest(questData);
                Debug.Log($"[NPC] ✅ Đã nhận nhiệm vụ: {questData.questName}");
            }
            // 2. Nếu đang làm và chưa hoàn thành
            else if (!QuestManager.instance.IsQuestCompleted(questData.questID))
            {
                if (HasItemInInventory())
                {
                    //RemoveItemFromInventory();
                    QuestManager.instance.UpdateQuestObjective(ObjectiveType.CollectItem, requiredItemID.ToString(), 1);
                    Debug.Log($"[NPC] ✅ Đã giao item ID {requiredItemID} và cập nhật nhiệm vụ.");
                }
                // ✅ Cập nhật UI
                QuestUI ui = FindObjectOfType<QuestUI>();
                if (ui != null)
                {
                    ui.ShowSuccess(); // hoặc ui.UpdateQuestText();
                }
                else
                {
                    Debug.LogWarning($"[NPC] ❌ Không tìm thấy item ID {requiredItemID} trong túi.");
                }
            }
            // 3. Nếu đã hoàn thành rồi
            else
            {
                Debug.Log("[NPC] ✅ Nhiệm vụ đã hoàn thành trước đó.");
            }
        }
    }

    // Kiểm tra có item trong inventory không
    bool HasItemInInventory()
    {
        if (playerInventory == null) return false;

        return playerInventory.items.Any(item => item != null && item.id == requiredItemID && item.amount > 0);
    }

    // Xóa 1 item khỏi inventory
    //void RemoveItemFromInventory()
    //{
    //    var item = playerInventory.items.FirstOrDefault(i => i != null && i.id == requiredItemID);
    //    if (item != null)
    //    {
    //        playerInventory.RemoveItem(item, 1);
    //        Debug.Log($"[NPC] Đã xóa 1 item ID {requiredItemID} khỏi túi.");
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<vItemManager>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }
}
