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

    void Awake()
    {
        tpCamera = FindFirstObjectByType<vThirdPersonCamera>();
        combatInput = FindFirstObjectByType<vMeleeCombatInput>();
        playerInput = FindFirstObjectByType<vThirdPersonInput>();
        playerAnimator = FindFirstObjectByType<Animator>();
        playerRigidbody = FindFirstObjectByType<Rigidbody>();

        if (playerAnimator)
            originalUseRootMotion = playerAnimator.applyRootMotion;

        // 🔥 Gán các UI nếu chưa có
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
    private bool EnsurePlayerInventory()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<vItemManager>();
            if (playerInventory == null)
                playerInventory = player.GetComponentInChildren<vItemManager>();
            if (playerInventory == null)
            {
                Debug.LogError("<color=red>[NPC-DEBUG] Không tìm thấy vItemManager trên Player!</color>");
                return false;
            }
            return true;
        }
        else
        {
            Debug.LogError("<color=red>[NPC-DEBUG] Không tìm thấy object nào tag Player!</color>");
            playerInventory = null;
            return false;
        }
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

        // Luôn lấy lại reference Player mỗi lần bắt đầu hội thoại
        if (!EnsurePlayerInventory())
        {
            dialoguePanel?.SetActive(false);
            Debug.LogWarning("[NPC-DEBUG] Không thể bắt đầu hội thoại do không có player hoặc inventory!");
            return;
        }

        Debug.Log($"[NPC-DEBUG][Dialogue] QuestID: {questData.questID}, RequiredItemID: {requiredItemID}");

        foreach (var obj in questData.objectives)
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

        bool isActive = QuestManager.instance.IsQuestActive(questData.questID);
        bool isCompleted = QuestManager.instance.IsQuestCompleted(questData.questID);

        int collectedAmount = GetCurrentItemCount();
        int requiredAmount = GetRequiredAmountFromQuest();

        // ✅ Nếu chưa nhận nhiệm vụ
        if (!isActive && !isCompleted)
        {
            dialogueKeys = introDialogueKeys;
            currentState = DialogueState.Intro;
        }
        // ✅ Đã nhận nhiệm vụ, nhưng chưa hoàn thành chính thức
        else if (isActive)
        {
            if (collectedAmount >= requiredAmount)
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

        hasGivenReward = false; // Reset lại mỗi lần bắt đầu hội thoại
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

        if (currentState == DialogueState.Intro)
        {
            QuestManager.instance.StartQuest(questData);
            Debug.Log("[NPC] Đã nhận nhiệm vụ sau hội thoại intro.");
        }

        if (currentState == DialogueState.Complete)
        {
            // Chỉ hoàn thành và trao thưởng nếu nhiệm vụ vẫn còn active (chưa hoàn thành)
            if (!hasGivenReward && QuestManager.instance.IsQuestActive(questData.questID))
            {
                RemoveRequiredItems();
                QuestManager.instance.CompleteQuest(questData); // Chỉ hoàn thành 1 lần khi trả
                hasGivenReward = true;
            }

            // Spawn bản đồ nếu chưa có trong scene
            if (mapPieceReward && rewardSpawnPoint && GameObject.Find(mapPieceReward.name) == null)
            {
                Instantiate(mapPieceReward, rewardSpawnPoint.position, Quaternion.identity);
                Debug.Log("[NPC] Spawn bản đồ nhiệm vụ.");
            }

            // Hiện cổng dịch chuyển nếu chưa kích hoạt
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

        if (playerAnimator)
        {
            playerAnimator.applyRootMotion = false;
            // Nếu animator không có parameter này thì thôi
            if (playerAnimator.HasParameterOfType("InputMagnitude", AnimatorControllerParameterType.Float))
                playerAnimator.SetFloat("InputMagnitude", 0f);
            playerAnimator.Play("Idle", 0);
        }
    }

    void UnlockControls()
    {
        if (tpCamera) tpCamera.enabled = true;
        if (combatInput) combatInput.lockInput = false;
        if (playerInput) playerInput.enabled = true;
        if (playerAnimator) playerAnimator.applyRootMotion = originalUseRootMotion;
    }
}

// Hàm extension giúp kiểm tra animator parameter (chống lỗi Parameter does not exist)
public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        return self.parameters.Any(p => p.type == type && p.name == name);
    }
}