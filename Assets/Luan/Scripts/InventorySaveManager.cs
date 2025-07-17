using System.Collections.Generic;
using UnityEngine;
using Invector.vItemManager;
using System.IO;
using Invector.vCharacterController;

[System.Serializable]
public class SavedItemData
{
    public int itemID;
    public int amount;
}

public class InventorySaveManager : MonoBehaviour
{
    public static InventorySaveManager instance;

    [Header("Thông tin đã lưu")]
    public List<SavedItemData> savedItems = new List<SavedItemData>();

    private string savePath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.dataPath + "/InventoryData.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Save Inventory To File")]
    public void SaveInventoryToFile()
    {
        string json = JsonUtility.ToJson(new SavedItemWrapper(savedItems), true);
        File.WriteAllText(savePath, json);
        Debug.Log("✅ Đã lưu Inventory vào file: " + savePath);
    }

    [ContextMenu("Load Inventory From File")]
    public void LoadInventoryFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            savedItems = JsonUtility.FromJson<SavedItemWrapper>(json).items;
            Debug.Log("✅ Đã load Inventory từ file.");
        }
    }

    public void SaveInventory(vItemManager itemManager)
    {
        savedItems.Clear();

        foreach (var item in itemManager.items)
        {
            if (item != null)
            {
                SavedItemData data = new SavedItemData
                {
                    itemID = item.id,
                    amount = item.amount
                };
                savedItems.Add(data);
            }
        }

        Debug.Log("[InventorySaveManager] ✅ Đã lưu " + savedItems.Count + " vật phẩm.");
    }

    public void LoadInventory(vItemManager itemManager)
    {
        itemManager.items.Clear();

        foreach (var data in savedItems)
        {
            var originalItem = itemManager.itemListData.items.Find(i => i.id == data.itemID);
            if (originalItem != null)
            {
                // ✅ Tạo bản sao đúng chuẩn
                var newItem = ScriptableObject.Instantiate(originalItem) as vItem;
                newItem.amount = data.amount;

                // ✅ Thêm vào inventory
                //itemManager.AddItem(newItem);

                // ✅ Nếu là vũ khí hoặc trang bị → tự động equip lại
                //if (newItem.type.ToString() == "Weapon" || newItem.type.ToString() == "Equipment")
                //{
                //    itemManager.EquipItemToEquipSlot(newItem, -1, true);
                //    Debug.Log("✅ Đã tự trang bị lại: " + newItem.name);
                //}
            }
            else
            {
                Debug.LogWarning("[InventorySaveManager] ⚠ Không tìm thấy item ID: " + data.itemID);
            }

        }

        Debug.Log("[InventorySaveManager] ✅ Đã tải lại " + savedItems.Count + " vật phẩm.");
    }




    [System.Serializable]
    private class SavedItemWrapper
    {
        public List<SavedItemData> items;

        public SavedItemWrapper(List<SavedItemData> items)
        {
            this.items = items;
        }
    }
}
