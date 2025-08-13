using UnityEngine;
using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.Text;

[System.Serializable] public class ItemAttributeSave { public string name; public int value; }
[System.Serializable] public class ItemSaveData { public int id; public int amount; public List<ItemAttributeSave> attrs = new List<ItemAttributeSave>(); }
[System.Serializable] public class ItemSaveDataList { public List<ItemSaveData> items = new List<ItemSaveData>(); }

public class InventorySyncManager : MonoBehaviour
{
    public vItemManager itemManager;
    public int characterId;

    [Header("Auto Save")]
    public float checkInterval = 0.5f;
    public float minSaveInterval = 0.7f;

    private bool isRestoring = false;
    private bool forceDirty = false;
    private string lastSnapshot = "";
    private float lastSaveTime = -999f;
    private Coroutine monitorCo;

    void Start()
    {
        if (itemManager == null) itemManager = GetComponent<vItemManager>();
        if (characterId <= 0) characterId = PlayerPrefs.GetInt("CharacterId", -1);

        itemManager.onAddItem.RemoveAllListeners();
        itemManager.onUseItem.RemoveAllListeners();
        itemManager.onAddItem.AddListener(_ => MarkDirty());
        itemManager.onUseItem.AddListener(_ => MarkDirty());

        if (characterId <= 0) return;

        StartCoroutine(LoadInventoryFromServer());

        if (monitorCo == null)
            monitorCo = StartCoroutine(AutoMonitor());
    }

    public void MarkDirty()
    {
        forceDirty = true;
    }

    IEnumerator AutoMonitor()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(checkInterval);
            if (isRestoring) continue;

            string snap = Snapshot();
            bool changed = snap != lastSnapshot;

            if ((changed || forceDirty) && (Time.unscaledTime - lastSaveTime) >= minSaveInterval)
            {
                SaveInventoryToServer_Internal(snap);
            }

            forceDirty = false;
        }
    }

    string Snapshot()
    {
        var sb = new StringBuilder();
        var list = itemManager.items.OrderBy(i => i.id).ToList();
        foreach (var i in list)
        {
            int dmg = GetAttr(i, vItemAttributes.Damage);
            int stam = GetAttr(i, vItemAttributes.StaminaCost);
            sb.Append(i.id).Append('|').Append(i.amount).Append('|').Append(dmg).Append('|').Append(stam).Append(';');
        }
        return sb.ToString();
    }

    int GetAttr(vItem item, vItemAttributes attr)
    {
        var a = item.GetItemAttribute(attr);
        return a != null ? a.value : 0;
    }

    public string SerializeInventoryToJson()
    {
        var items = itemManager.items.Select(i =>
        {
            var d = new ItemSaveData { id = i.id, amount = i.amount };
            var dmg = i.GetItemAttribute(vItemAttributes.Damage);
            if (dmg != null) d.attrs.Add(new ItemAttributeSave { name = vItemAttributes.Damage.ToString(), value = dmg.value });
            var stam = i.GetItemAttribute(vItemAttributes.StaminaCost);
            if (stam != null) d.attrs.Add(new ItemAttributeSave { name = vItemAttributes.StaminaCost.ToString(), value = stam.value });
            return d;
        }).ToList();
        return JsonUtility.ToJson(new ItemSaveDataList { items = items });
    }

    public void DeserializeInventoryFromJson(string json)
    {
        var loaded = JsonUtility.FromJson<ItemSaveDataList>(json);
        if (loaded == null || loaded.items == null) return;

        isRestoring = true;

        itemManager.DestroyAllItems();

        foreach (var data in loaded.items)
        {
            var itemRef = new ItemReference(data.id) { amount = data.amount };
            itemManager.AddItem(itemRef, true);

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

        lastSnapshot = Snapshot();
        isRestoring = false;
    }

    public void SaveInventoryToServer()
    {
        var snap = Snapshot();
        SaveInventoryToServer_Internal(snap);
    }

    void SaveInventoryToServer_Internal(string snapshot)
    {
        string inventoryJson = SerializeInventoryToJson();

        if (ProfileManager.CurrentProfile != null)
            ProfileManager.CurrentProfile.inventoryJSON = inventoryJson;

        lastSnapshot = snapshot;
        lastSaveTime = Time.unscaledTime;

        StartCoroutine(UpdateProfileInventoryCoroutine(inventoryJson));
    }

    IEnumerator UpdateProfileInventoryCoroutine(string inventoryJson)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        var getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();
        if (getReq.result != UnityWebRequest.Result.Success) yield break;

        var wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(getReq.downloadHandler.text);
        var profile = wrapper.data;
        profile.inventoryJSON = inventoryJson;

        string putUrl = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(profile);
        var putReq = new UnityWebRequest(putUrl, "PUT");
        putReq.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        putReq.downloadHandler = new DownloadHandlerBuffer();
        putReq.SetRequestHeader("Content-Type", "application/json");
        yield return putReq.SendWebRequest();
    }

    IEnumerator LoadInventoryFromServer()
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        var getReq = UnityWebRequest.Get(url);
        yield return getReq.SendWebRequest();

        var wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(getReq.downloadHandler.text);
        var profile = wrapper.data;

        if (!string.IsNullOrEmpty(profile.inventoryJSON) && profile.inventoryJSON != "[]")
            DeserializeInventoryFromJson(profile.inventoryJSON);
        else
            lastSnapshot = Snapshot();

        var ui = FindFirstObjectByType<InventoryUpgradeUI>();
        if (ui != null) ui.ForceRefresh();
    }

    public void ForceReloadFromServer()
    {
        if (characterId > 0) StartCoroutine(LoadInventoryFromServer());
    }
}
