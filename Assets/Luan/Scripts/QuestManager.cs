using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class QuestSaveData
{
    public List<string> completedQuestIDs = new List<string>();
}

// Wrapper cho API Quest
[System.Serializable]
public class PlayerQuestListWrapper
{
    public bool status;
    public PlayerQuestDto[] data;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }

    public Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
    public List<QuestData> completedQuests = new List<QuestData>();
    void Start()
    {
        LoadQuestsFromApi();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Không dùng local nữa:
            // LoadQuestProgress();
        }
    }

    // --- Giao diện nhận nhiệm vụ như cũ ---
    public void StartQuest(QuestData quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.questID)) return;
        foreach (var obj in quest.objectives)
            obj.currentAmount = 0;

        activeQuests.Add(quest.questID, quest);

        Debug.Log($"[DEBUG][StartQuest] Trước khi save: questID={quest.questID}, currentAmount={quest.objectives[0].currentAmount}, requiredAmount={quest.objectives[0].requiredAmount}");
        SaveQuestToApi(quest, "active");
        Debug.Log($"[QuestManager] Bắt đầu nhiệm vụ: {quest.questName}");
        FindFirstObjectByType<QuestPanelToggle>()?.UpdateBadge();
    }


    public void UpdateQuestObjective(ObjectiveType type, string targetID, int amount = 1)
    {
        foreach (var quest in activeQuests.Values)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && obj.targetID == targetID && !obj.IsCompleted())
                {
                    obj.currentAmount += amount;
                    Debug.Log($"[QuestManager] Đã cập nhật mục tiêu: {obj.objectiveDescription} ({obj.currentAmount}/{obj.requiredAmount})");
                    SaveQuestToApi(quest, "active");
                    return;
                }
            }
        }
    }

    private void CheckQuestCompletion(QuestData quest)
    {
        bool allCompleted = quest.objectives.All(obj => obj.IsCompleted());
        if (allCompleted)
        {
            CompleteQuest(quest);
        }
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!activeQuests.ContainsKey(quest.questID)) return;
        activeQuests.Remove(quest.questID);
        completedQuests.Add(quest);

        SaveQuestToApi(quest, "completed");
        Debug.Log($"[QuestManager] Đã hoàn thành nhiệm vụ: {quest.questName}");

        GameObject portal = GameObject.Find($"Portal_{quest.questID}");
        if (portal != null)
        {
            portal.SetActive(true);
            Debug.Log("[QuestManager] Đã mở cổng: " + portal.name);
        }

        QuestUI ui = FindFirstObjectByType<QuestUI>();
        if (ui != null)
        {
            ui.ShowSuccess();
        }

        Debug.Log($"[QuestManager] Thưởng: {quest.goldReward} vàng, +{quest.healthIncreaseReward} HP, +{quest.damageIncreaseReward} sát thương.");

        // Nếu có nhiệm vụ tiếp theo thì mở luôn
        if (quest.nextQuest != null)
        {
            Debug.Log("[QuestManager] Mở nhiệm vụ tiếp theo: " + quest.nextQuest.questName);
            StartQuest(quest.nextQuest);
        }
    }

    public QuestData GetActiveQuest(string questID)
    {
        activeQuests.TryGetValue(questID, out QuestData quest);
        return quest;
    }

    public bool IsQuestActive(string questID)
    {
        return activeQuests.ContainsKey(questID);
    }

    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Any(q => q.questID == questID);
    }

    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        Debug.Log("[QuestManager] Đã reset toàn bộ nhiệm vụ.");
    }

    // ---- PHẦN ĐỒNG BỘ QUEST VỚI API ----

    public void LoadQuestsFromApi()
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        int characterId = PlayerPrefs.GetInt("CharacterId", -1);
        if (playerId > 0 && characterId > 0)
            StartCoroutine(QuestApiManager.Instance.GetPlayerQuests(playerId, characterId, OnLoadedQuestsFromApi));
    }

    // Hàm nhận dữ liệu từ API về và map lại vào game
    void OnLoadedQuestsFromApi(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Không nhận được dữ liệu quest từ API!");
            return;
        }

        var questListWrapper = JsonUtility.FromJson<PlayerQuestListWrapper>(json);

        // Chỉ clear khi dữ liệu thực sự hợp lệ và khác rỗng!
        if (questListWrapper != null && questListWrapper.data != null && questListWrapper.data.Length > 0)
        {
            activeQuests.Clear();
            completedQuests.Clear();

            Debug.Log($"[OnLoadedQuestsFromApi] Nhận {questListWrapper.data.Length} quest từ API:");
            foreach (var questDto in questListWrapper.data)
            {
                Debug.Log($"[OnLoadedQuestsFromApi]   QuestID: {questDto.questID} | status: {questDto.status} | progressJSON: {questDto.progressJSON}");

                QuestData questDataAsset = Resources.Load<QuestData>($"Quests/{questDto.questID}");
                if (questDataAsset == null)
                {
                    Debug.LogWarning($"Không tìm thấy asset QuestData: Quests/{questDto.questID}");
                    continue;
                }

                QuestData questData = ScriptableObject.Instantiate(questDataAsset);

                var objectives = JsonHelper.FromJson<QuestObjectiveProgress>(questDto.progressJSON);
                if (objectives != null)
                {
                    foreach (var obj in objectives)
                    {
                        Debug.Log($"[OnLoadedQuestsFromApi]     Objective: type={obj.type}, targetID={obj.targetID}, currentAmount={obj.currentAmount}, requiredAmount={obj.requiredAmount}");
                    }
                }

                for (int i = 0; i < questData.objectives.Count; i++)
                {
                    var obj = questData.objectives[i];
                    var found = objectives.FirstOrDefault(o => o.targetID == obj.targetID && o.type == obj.type.ToString());
                    obj.currentAmount = found != null ? found.currentAmount : 0;
                }

                if (questDto.status == "completed")
                    completedQuests.Add(questData);
                else
                    activeQuests.Add(questData.questID, questData);
            }
            Debug.Log("[QuestManager] Đã load quest từ API xong.");
        }
        else
        {
            Debug.LogWarning("Quest API trả về rỗng hoặc không hợp lệ! Giữ nguyên danh sách quest local.");
        }

        FindFirstObjectByType<QuestPanelToggle>()?.UpdateBadge();
    }




    public void SaveQuestToApi(QuestData quest, string status, System.Action<bool> callback = null)
    {
        var objectives = quest.objectives.Select(obj => new QuestObjectiveProgress
        {
            type = obj.type.ToString(),
            targetID = obj.targetID,
            currentAmount = obj.currentAmount,
            requiredAmount = obj.requiredAmount
        }).ToArray();

        Debug.Log($"[SaveQuestToApi] Gửi quest: {quest.questID} | status: {status}");
        foreach (var obj in objectives)
        {
            Debug.Log($"[SaveQuestToApi]   Objective: type={obj.type}, targetID={obj.targetID}, currentAmount={obj.currentAmount}, requiredAmount={obj.requiredAmount}");
        }

        string progressJson = JsonHelper.ToJson(objectives, true);

        PlayerQuestDto dto = new PlayerQuestDto
        {
            playerId = PlayerPrefs.GetInt("PlayerId"),
            characterId = PlayerPrefs.GetInt("CharacterId"),
            questID = quest.questID,
            status = status,
            progressJSON = progressJson
        };

        StartCoroutine(QuestApiManager.Instance.SaveQuest(dto, callback));
    }
}