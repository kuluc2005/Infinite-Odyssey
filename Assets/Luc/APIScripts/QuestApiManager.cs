using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[System.Serializable]
public class QuestObjectiveProgress
{
    public string type;
    public string targetID;
    public int currentAmount;
    public int requiredAmount;
}

[System.Serializable]
public class PlayerQuestDto
{
    public int playerId;
    public int characterId;
    public string questID;
    public string status;
    public string progressJSON;
}

public class QuestApiManager : MonoBehaviour
{
    public static QuestApiManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }



    public IEnumerator GetPlayerQuests(int playerId, int characterId, Action<string> onResult)
    {
        string url = $"http://localhost:5186/api/quest/list/{playerId}/{characterId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            Debug.Log("[QuestApiManager] Nhận danh sách quest từ API:");
            Debug.Log(request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                onResult?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("[QuestApiManager] Lỗi lấy quest: " + request.error);
                onResult?.Invoke(null);
            }
        }
    }
    public IEnumerator SaveQuest(PlayerQuestDto quest, System.Action<bool> callback = null)
    {
        string url = "http://localhost:5186/api/quest/save";
        string json = JsonUtility.ToJson(quest);
        Debug.Log($"[QuestApiManager] POST SaveQuest: {json}");
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool ok = request.result == UnityWebRequest.Result.Success;
            if (ok)
            {
                Debug.Log($"[QuestApiManager] SaveQuest thành công: {quest.questID}");
            }
            else
            {
                Debug.LogWarning($"[QuestApiManager] SaveQuest thất bại: {quest.questID}, error: {request.error}");
            }
            callback?.Invoke(ok);
        }
    }
}