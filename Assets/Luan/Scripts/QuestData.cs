using UnityEngine;
using System.Collections.Generic; // Để dùng List

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questID; 
    public string questName; 
    [TextArea(3, 10)]
    public string questDescription; 

    public string targetTag;         
    public int targetKillCount = 1; 


    [Header("Quest Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Quest Rewards")]
    public int goldReward; 
    public float healthIncreaseReward; 
    public float damageIncreaseReward; 

    [Header("Dialogue Related")]
    public List<DialogueLine> startDialogue; 
    public List<DialogueLine> progressDialogue; 
    public List<DialogueLine> completeDialogue; 

    [Header("Next Quest (Optional)")]
    public QuestData nextQuest; 



}

[System.Serializable]
public class QuestObjective
{
    public string objectiveDescription; // Mô tả mục tiêu (ví dụ: "Đánh bại Lang Vương")
    public ObjectiveType type; // Loại mục tiêu (Kill, Collect, Talk, GoToLocation)
    public string targetID; // ID của mục tiêu (ví dụ: "LangVuong", "LangVuongNha")
    public int requiredAmount; // Số lượng cần thiết (ví dụ: 1 con Lang Vương, 1 cái răng)
    [HideInInspector] public int currentAmount; // Số lượng hiện tại (được cập nhật trong quá trình chơi)

    public bool IsCompleted()
    {
        return currentAmount >= requiredAmount;
    }
}

public enum ObjectiveType
{
    KillEnemy,
    CollectItem,
    TalkToNPC,
    GoToLocation
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName; // Tên người nói (ví dụ: "Lão tu đạo sĩ", "Odyn")
    [TextArea(1, 3)]
    public string dialogueText; // Nội dung hội thoại
    public float displayDuration = 3f; // Thời gian hiển thị dòng hội thoại
}