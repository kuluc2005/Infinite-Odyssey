using UnityEngine;
using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;



// --- Dùng cho serialize/deserialize ---
[System.Serializable]
public class ItemSaveData
{
    public int id;
    public int amount;
}

[System.Serializable]
public class ItemSaveDataList
{
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

// --- Dùng cho deserialize từ backend ---

public class InventorySyncManager : MonoBehaviour
{
    [Header("Setup")]
    public vItemManager itemManager;
    public int characterId; // Gán đúng CharacterId của nhân vật (hoặc lấy từ PlayerPrefs)

    void Start()
    {
        if (itemManager == null)
            itemManager = GetComponent<vItemManager>();

        if (characterId <= 0)
            characterId = PlayerPrefs.GetInt("CharacterId", -1);

        Debug.Log("InventorySyncManager Start: characterId = " + characterId);

        if (characterId <= 0)
        {
            Debug.LogError("InventorySyncManager: characterId không hợp lệ! Không load/sync inventory.");
            return;
        }

        StartCoroutine(LoadInventoryFromServer());
        // XÓA các dòng addListener tại đây!
    }



    // ===== Serialize inventory sang JSON =====
    public string SerializeInventoryToJson()
    {
        var items = itemManager.items.Select(i => new ItemSaveData
        {
            id = i.id,
            amount = i.amount
        }).ToList();

        var list = new ItemSaveDataList { items = items };
        return JsonUtility.ToJson(list);
    }

    // ===== Deserialize inventory từ JSON về lại vItemManager =====
    public void DeserializeInventoryFromJson(string json)
    {
        var loaded = JsonUtility.FromJson<ItemSaveDataList>(json);
        if (loaded != null && loaded.items != null)
        {
            itemManager.DestroyAllItems();

            foreach (var data in loaded.items)
            {
                Debug.Log("Try to add item id: " + data.id + ", amount: " + data.amount);
                var itemRef = new ItemReference(data.id);
                itemRef.amount = data.amount;
                bool exist = itemManager.itemListData.items.Any(i => i.id == data.id);
                Debug.Log("Item ID " + data.id + " exists in database: " + exist);
                itemManager.AddItem(itemRef, true);
            }
        }
    }

    // ===== Lưu inventory lên API =====
    public void SaveInventoryToServer()
    {
        string inventoryJson = SerializeInventoryToJson();
        Debug.Log("Will save inventoryJSON: " + inventoryJson);
        StartCoroutine(UpdateProfileInventoryCoroutine(inventoryJson));
    }


    IEnumerator UpdateProfileInventoryCoroutine(string inventoryJson)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();

        if (getReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Lỗi khi lấy profile: " + getReq.error);
            yield break;
        }

        var profileWrapper = JsonUtility.FromJson<PlayerProfileWrapper>(getReq.downloadHandler.text);
        var profile = profileWrapper.data;

        // Gán inventoryJson vào đúng field
        profile.inventoryJSON = inventoryJson;

        // PUT lên server
        string putUrl = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(profile);
        UnityWebRequest putReq = new UnityWebRequest(putUrl, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        putReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
        putReq.downloadHandler = new DownloadHandlerBuffer();
        putReq.SetRequestHeader("Content-Type", "application/json");
        yield return putReq.SendWebRequest();

        if (putReq.result == UnityWebRequest.Result.Success)
            Debug.Log("✅ Inventory đã được đồng bộ lên server!");
        else
            Debug.LogError("❌ Lỗi đồng bộ inventory: " + putReq.error);
    }

    // ===== Load inventory từ API backend =====
    IEnumerator LoadInventoryFromServer()
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();

        string raw = getReq.downloadHandler.text;
        Debug.Log("Profile API response: " + raw);

        // Deserialize JSON như cũ...
        PlayerProfileWrapper profileWrapper = JsonUtility.FromJson<PlayerProfileWrapper>(raw);
        var profile = profileWrapper.data;

        if (!string.IsNullOrEmpty(profile.inventoryJSON) && profile.inventoryJSON != "[]")
        {
            // === Gỡ các sự kiện auto-save trước khi deserialize ===
            itemManager.onAddItem.RemoveAllListeners();
            itemManager.onUseItem.RemoveAllListeners();

            DeserializeInventoryFromJson(profile.inventoryJSON);
            Debug.Log("Đã tải inventory từ server.");

            // === Chỉ đăng ký lại sự kiện auto-save SAU khi inventory đã load xong! ===
            itemManager.onAddItem.AddListener((item) => SaveInventoryToServer());
            itemManager.onUseItem.AddListener((item) => SaveInventoryToServer());
        }
        else
        {
            Debug.Log("Không có inventory để load.");
            // Vẫn nên đăng ký event ở đây
            itemManager.onAddItem.RemoveAllListeners();
            itemManager.onUseItem.RemoveAllListeners();
            itemManager.onAddItem.AddListener((item) => SaveInventoryToServer());
            itemManager.onUseItem.AddListener((item) => SaveInventoryToServer());
        }
    }
}
