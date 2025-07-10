using UnityEngine;
using System.Collections.Generic; // Để dùng List

// Tạo menu để dễ dàng tạo QuestData mới trong Editor
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questID; // ID duy nhất cho nhiệm vụ (ví dụ: "M1_TimDaoSi")
    public string questName; // Tên hiển thị của nhiệm vụ
    [TextArea(3, 10)]
    public string questDescription; // Mô tả nhiệm vụ chi tiết

    public string targetTag;         // Tag của quái vật cần tiêu diệt
    public int targetKillCount = 1;  // Số lượng cần tiêu diệt


    [Header("Quest Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Quest Rewards")]
    public int goldReward; // Phần thưởng vàng
    public float healthIncreaseReward; // Tăng máu (như Lang Vương Nha)
    public float damageIncreaseReward; // Tăng sát thương
    // Thêm các loại phần thưởng khác nếu có (ví dụ: itemID, exp)

    [Header("Dialogue Related")]
    public List<DialogueLine> startDialogue; // Các dòng hội thoại khi nhận nhiệm vụ
    public List<DialogueLine> progressDialogue; // Hội thoại khi nhiệm vụ đang diễn ra
    public List<DialogueLine> completeDialogue; // Hội thoại khi hoàn thành nhiệm vụ

    [Header("Next Quest (Optional)")]
    public QuestData nextQuest; // Nhiệm vụ tiếp theo sau khi hoàn thành nhiệm vụ này



    // Các trạng thái của nhiệm vụ có thể được quản lý bên ngoài, nhưng đôi khi hữu ích để có trong Data nếu nhiệm vụ có điều kiện đặc biệt
}

// Cấu trúc cho mỗi mục tiêu trong nhiệm vụ
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

// Cấu trúc cho mỗi dòng hội thoại (giống như hình bạn gửi)
[System.Serializable]
public class DialogueLine
{
    public string speakerName; // Tên người nói (ví dụ: "Lão tu đạo sĩ", "Odyn")
    [TextArea(1, 3)]
    public string dialogueText; // Nội dung hội thoại
    public float displayDuration = 3f; // Thời gian hiển thị dòng hội thoại
    // Thêm các trường khác như animation, sound clip, v.v.
}