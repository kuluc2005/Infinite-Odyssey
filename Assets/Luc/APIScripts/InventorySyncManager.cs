using UnityEngine;
using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;



// --- Dùng cho serialize/deserialize ---

[System.Serializable]
public class ItemAttributeSave
{
    public string name;   // ví dụ: "Damage", "StaminaCost"
    public int value;
}

[System.Serializable]
public class ItemSaveData
{
    public int id;
    public int amount;
    public List<ItemAttributeSave> attrs = new List<ItemAttributeSave>();
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

        // --- Đăng ký auto-save inventory khi có thay đổi ---
        itemManager.onAddItem.RemoveAllListeners();
        itemManager.onUseItem.RemoveAllListeners();
        itemManager.onAddItem.AddListener((item) => SaveInventoryToServer());
        itemManager.onUseItem.AddListener((item) => SaveInventoryToServer());

        Debug.Log("InventorySyncManager Start: characterId = " + characterId);

        if (characterId <= 0)
        {
            Debug.LogError("InventorySyncManager: characterId không hợp lệ! Không load/sync inventory.");
            return;
        }

        StartCoroutine(LoadInventoryFromServer());
    }



    // ===== Serialize inventory sang JSON =====
    public string SerializeInventoryToJson()
    {
        var items = itemManager.items.Select(i =>
        {
            var data = new ItemSaveData { id = i.id, amount = i.amount };

            var dmg = i.GetItemAttribute(vItemAttributes.Damage);
            if (dmg != null) data.attrs.Add(new ItemAttributeSave { name = vItemAttributes.Damage.ToString(), value = dmg.value });

            var stam = i.GetItemAttribute(vItemAttributes.StaminaCost);
            if (stam != null) data.attrs.Add(new ItemAttributeSave { name = vItemAttributes.StaminaCost.ToString(), value = stam.value });

            return data;
        }).ToList();

        var list = new ItemSaveDataList { items = items };
        return JsonUtility.ToJson(list);
    }

    // ===== Deserialize inventory từ JSON về lại vItemManager =====
    public void DeserializeInventoryFromJson(string json)
    {
        var loaded = JsonUtility.FromJson<ItemSaveDataList>(json);
        if (loaded == null || loaded.items == null) return;

        itemManager.DestroyAllItems();

        foreach (var data in loaded.items)
        {
            var itemRef = new ItemReference(data.id) { amount = data.amount };
            itemManager.AddItem(itemRef, true);

            // tìm instance trong inventory để set lại chỉ số
            var inst = itemManager.items.FirstOrDefault(x => x.id == data.id);
            if (inst == null || data.attrs == null) continue;

            foreach (var a in data.attrs)
            {
                if (System.Enum.TryParse<vItemAttributes>(a.name, out var attrEnum))
                {
                    var attr = inst.GetItemAttribute(attrEnum);
                    if (attr != null) attr.value = a.value;
                }
            }
        }
    }

    // ===== Lưu inventory lên API =====
    public void SaveInventoryToServer()
    {
        string inventoryJson = SerializeInventoryToJson();
        Debug.Log("<color=yellow>[DEBUG][Inventory] SaveInventoryToServer ĐƯỢC GỌI!</color> " + inventoryJson);
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
            Debug.Log("Inventory đã được đồng bộ lên server!");
        else
            Debug.LogError("Lỗi đồng bộ inventory: " + putReq.error);
    }

    // ===== Load inventory từ API backend =====
    IEnumerator LoadInventoryFromServer()
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();

        string raw = getReq.downloadHandler.text;
        Debug.Log("Profile API response: " + raw);

        PlayerProfileWrapper profileWrapper = JsonUtility.FromJson<PlayerProfileWrapper>(raw);
        var profile = profileWrapper.data;

        if (!string.IsNullOrEmpty(profile.inventoryJSON) && profile.inventoryJSON != "[]")
        {
            // Không cần đăng ký event lại ở đây!
            DeserializeInventoryFromJson(profile.inventoryJSON);
            Debug.Log("Đã tải inventory từ server.");
        }
        else
        {
            Debug.Log("Không có inventory để load.");
        }
    }

}
