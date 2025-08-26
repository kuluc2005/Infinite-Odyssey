using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using System.Linq;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("UI Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public GameObject skipButton;

    [Header("Quest & Dialogue Phases")]
    public QuestData questData;
    public string[] notReadyDialogueKeys;
    public string[] introDialogueKeys;
    public string[] completeDialogueKeys;

    [Header("Localization Table")]
    public string localizedTableName = "NPC Level 2";

    [Header("Reward & Portal")]
    public string requiredItemID = "14";
    public GameObject mapPieceReward;
    public Transform rewardSpawnPoint;
    public GameObject portalObject;

    private string[] dialogueKeys;
    private int currentLine = 0;
    private bool isTalking = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentFullText;
    private bool hasGivenReward = false;


    private enum DialogueState { None, Intro, NotReady, Complete }
    private DialogueState currentState = DialogueState.None;

    private vThirdPersonCamera tpCamera;
    private vMeleeCombatInput combatInput;
    private vThirdPersonInput playerInput;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private bool originalUseRootMotion;
    private vItemManager playerInventory;

    public static class DialogueSystemState
    {
        public static bool IsTalkingWithNPC = false;
    }

    void Awake()
    {
        tpCamera = FindFirstObjectByType<vThirdPersonCamera>();
        combatInput = FindFirstObjectByType<vMeleeCombatInput>();
        playerInput = FindFirstObjectByType<vThirdPersonInput>();
        playerAnimator = FindFirstObjectByType<Animator>();
        playerRigidbody = FindFirstObjectByType<Rigidbody>();

        if (playerAnimator)
            originalUseRootMotion = playerAnimator.applyRootMotion;

        if (dialoguePanel == null && UIDialogueManager.Instance != null)
        {
            dialoguePanel = UIDialogueManager.Instance.dialoguePanel;
            dialogueText = UIDialogueManager.Instance.dialogueText;
            continueButton = UIDialogueManager.Instance.continueButton;
            skipButton = UIDialogueManager.Instance.skipButton;
        }
    }

    /// <summary>
    /// Luôn lấy lại reference Player và vItemManager mỗi khi bắt đầu hội thoại để tránh lỗi null khi player spawn động!
    /// </summary>
    // NPCDialogueTrigger.cs
    private bool EnsurePlayerInventory()
    {
        // Ưu tiên: tìm player input của Invector (đây chắc chắn là Player đang điều khiển)
        var input = FindFirstObjectByType<vThirdPersonInput>();
        if (input != null)
        {
            playerInventory = input.GetComponentInChildren<vItemManager>();
            if (playerInventory != null) return true;
        }

        // Fallback: tìm bất kỳ vItemManager nào trong scene (phòng khi cấu trúc khác)
        playerInventory = FindFirstObjectByType<vItemManager>();
        if (playerInventory != null) return true;

        Debug.LogError("<color=red>[NPC-DEBUG] Không tìm thấy vItemManager của Player (không phụ thuộc tag)!</color>");
        return false;
    }


    void Update()
    {
        if (isTalking && Input.GetMouseButtonDown(0) && !isTyping)
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        Debug.Log($"[NPC-DEBUG][Dialogue] === MỞ HỘI THOẠI VỚI NPC ===");

        if (!EnsurePlayerInventory())
        {
            dialoguePanel?.SetActive(false);
            Debug.LogWarning("[NPC-DEBUG] Không thể bắt đầu hội thoại do không có player hoặc inventory!");
            return;
        }

        QuestData runtimeQuest = QuestManager.instance.GetActiveQuest(questData.questID);
        bool isActive = runtimeQuest != null;
        bool isCompleted = QuestManager.instance.IsQuestCompleted(questData.questID);

        Debug.Log($"[NPC-DEBUG][Dialogue] QuestID: {questData.questID}, RequiredItemID: {requiredItemID}");

        var objectivesToCheck = isActive ? runtimeQuest.objectives : questData.objectives;

        foreach (var obj in objectivesToCheck)
        {
            Debug.Log($"[NPC-DEBUG][Dialogue] Objective: type={obj.type}, targetID={obj.targetID}, currentAmount={obj.currentAmount}, requiredAmount={obj.requiredAmount}");
        }

        // Log inventory
        if (playerInventory != null && playerInventory.items != null)
        {
            foreach (var i in playerInventory.items)
            {
                if (i == null) continue;
                Debug.Log($"[NPC-DEBUG][Dialogue] Kho: {i.name} (ID: {i.id}) x{i.amount}");
            }
        }

        currentLine = 0;
        isTalking = true;
        LockControls();
        dialoguePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DialogueSystemState.IsTalkingWithNPC = true;


        // Xác định trạng thái nhiệm vụ
        bool isObjectiveCompleted = objectivesToCheck.All(obj => obj.IsCompleted());

        if (!isActive && !isCompleted)
        {
            dialogueKeys = introDialogueKeys;
            currentState = DialogueState.Intro;
        }
        else if (isActive)
        {
            if (isObjectiveCompleted)
            {
                dialogueKeys = completeDialogueKeys;
                currentState = DialogueState.Complete;
            }
            else
            {
                dialogueKeys = notReadyDialogueKeys;
                currentState = DialogueState.NotReady;
            }
        }
        else if (isCompleted)
        {
            dialogueKeys = completeDialogueKeys;
            currentState = DialogueState.Complete;
        }

        hasGivenReward = false;
        ShowLine(currentLine);
    }

    int GetCurrentItemCount()
    {
        if (playerInventory == null || playerInventory.items == null)
        {
            Debug.LogWarning("[NPC-DEBUG] Không tìm thấy inventory hoặc items!");
            return 0;
        }

        foreach (var i in playerInventory.items)
        {
            if (i == null) continue;
            Debug.Log($"[NPC-DEBUG][Check] Inventory có item: {i.name} (ID: {i.id}) x{i.amount}");
        }

        int total = playerInventory.items
            .Where(i => i != null && i.id.ToString() == requiredItemID)
            .Sum(i => i.amount);

        Debug.Log($"[NPC-DEBUG][Check] Tổng số coin ID = {requiredItemID} trong inventory: {total}");
        return total;
    }

    int GetRequiredAmountFromQuest()
    {
        var objective = questData.objectives.FirstOrDefault(obj => obj.type == ObjectiveType.CollectItem && obj.targetID == requiredItemID);
        return objective != null ? objective.requiredAmount : 1;
    }

    // Sử dụng đúng hàm DestroyItem của Invector để trừ số lượng!
    void RemoveRequiredItems()
    {
        if (playerInventory == null || playerInventory.items == null)
        {
            Debug.LogWarning("[NPC-DEBUG][Remove] Không tìm thấy inventory hoặc items!");
            return;
        }

        int requiredAmount = GetRequiredAmountFromQuest();
        int removed = 0;

        // Log inventory trước khi xóa
        foreach (var i in playerInventory.items)
        {
            if (i == null) continue;
            Debug.Log($"[NPC-DEBUG][Remove-Before] Có item: {i.name} (ID: {i.id}) x{i.amount}");
        }

        foreach (var item in playerInventory.items.Where(i => i != null && i.id.ToString() == requiredItemID).ToList())
        {
            if (removed >= requiredAmount) break;
            int toRemove = Mathf.Min(item.amount, requiredAmount - removed);

            Debug.Log($"[NPC-DEBUG][Remove] Đang xóa {toRemove} ở item ID: {item.id} - Trước khi xóa còn: {item.amount}");

            playerInventory.DestroyItem(item, toRemove);
            removed += toRemove;
        }
        Debug.Log($"[NPC-DEBUG][Remove] Đã xóa {removed} vật phẩm ID {requiredItemID} sau khi hoàn thành nhiệm vụ.");

        // Log inventory sau khi xóa
        foreach (var i in playerInventory.items)
        {
            if (i == null) continue;
            Debug.Log($"[NPC-DEBUG][Remove-After] Còn lại: {i.name} (ID: {i.id}) x{i.amount}");
        }
    }

    void ShowLine(int index)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (index < dialogueKeys.Length)
        {
            typingCoroutine = StartCoroutine(LoadLocalizedLine(dialogueKeys[index]));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator LoadLocalizedLine(string key)
    {
        isTyping = true;

        var table = LocalizationSettings.StringDatabase;
        var localizedString = table.GetLocalizedStringAsync(localizedTableName, key);
        yield return localizedString;

        currentFullText = localizedString.Result;
        dialogueText.text = "";

        foreach (char c in currentFullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        typingCoroutine = null;

        dialogueText.text = currentFullText;
    }

    public void OnContinueClicked()
    {
        if (!isTalking) return;

        if (isTyping)
        {
            // Nếu đang chạy từng chữ → hiển thị full luôn
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentFullText;
            isTyping = false;
            typingCoroutine = null;
        }
        else
        {
            // Nếu đã hiện xong câu → chuyển sang câu tiếp theo
            NextLine();
        }
    }

    void NextLine()
    {
        currentLine++;
        ShowLine(currentLine);
    }

    void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.SetActive(false);
        UnlockControls();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        DialogueSystemState.IsTalkingWithNPC = false;


        QuestData runtimeQuest = QuestManager.instance.GetActiveQuest(questData.questID);

        if (currentState == DialogueState.Intro)
        {
            QuestManager.instance.StartQuest(ScriptableObject.Instantiate(questData));
            //Debug.Log("[NPC] Đã nhận nhiệm vụ sau hội thoại intro.");
        }

        if (currentState == DialogueState.Complete)
        {
            if (!hasGivenReward && runtimeQuest != null)
            {
                if (runtimeQuest.objectives.Any(obj => obj.type == ObjectiveType.CollectItem))
                {
                    RemoveRequiredItems();
                    var invSync = FindFirstObjectByType<InventorySyncManager>();
                    if (invSync != null)
                    {
                        Debug.Log("<color=yellow>[NPC] Đồng bộ kho sau khi trừ vật phẩm</color>");
                        invSync.SaveInventoryToServer();
                    }
                }

                QuestManager.instance.CompleteQuest(runtimeQuest);

                if (runtimeQuest.goldReward > 0)
                {
                    GoldManager.Instance?.AddCoins(runtimeQuest.goldReward);
                    Debug.Log($"[NPC] Thưởng {runtimeQuest.goldReward} vàng cho người chơi.");
                }

                hasGivenReward = true;
            }

            if (mapPieceReward && rewardSpawnPoint && GameObject.Find(mapPieceReward.name) == null)
            {
                Instantiate(mapPieceReward, rewardSpawnPoint.position, Quaternion.identity);
                Debug.Log("[NPC] Spawn bản đồ nhiệm vụ.");
            }

            if (portalObject && !portalObject.activeSelf)
            {
                portalObject.SetActive(true);
                Debug.Log("[NPC] Hiện cổng dịch chuyển.");
            }
        }

        currentLine = 0;
        isTyping = false;
        typingCoroutine = null;
        currentFullText = "";
        currentState = DialogueState.None;
    }


    void LockControls()
    {
        if (tpCamera) tpCamera.enabled = false;
        if (combatInput) combatInput.lockInput = true;
        if (playerInput) playerInput.enabled = false;
        if (playerRigidbody) playerRigidbody.linearVelocity = Vector3.zero;

    }

    void UnlockControls()
    {
        StartCoroutine(DelayedUnlockControls());
    }

    IEnumerator DelayedUnlockControls()
    {
        yield return null;

        if (tpCamera) tpCamera.enabled = true;
        if (combatInput) combatInput.lockInput = false;
        if (playerInput) playerInput.enabled = true;

    }
}