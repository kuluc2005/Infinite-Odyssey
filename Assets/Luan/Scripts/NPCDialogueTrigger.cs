// ✅ NPCDialogueTrigger.cs - Hoàn chỉnh hệ thống hội thoại và nhiệm vụ thu thập vật phẩm (2 đồng vàng cổ)

using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("UI Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public GameObject skipButton;
    public GameObject npcImage;
    public GameObject playerImage;

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
    private bool hasSeenCompleteDialogue = false;
    private bool isReadyToComplete = false;
    private bool readyToCompleteDialogueShown = false;

    void Awake()
    {
        tpCamera = FindObjectOfType<vThirdPersonCamera>();
        combatInput = FindObjectOfType<vMeleeCombatInput>();
        playerInput = FindObjectOfType<vThirdPersonInput>();
        playerAnimator = FindObjectOfType<Animator>();
        playerRigidbody = FindObjectOfType<Rigidbody>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInventory = player.GetComponent<vItemManager>();

        if (playerAnimator)
            originalUseRootMotion = playerAnimator.applyRootMotion;
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
        if (isTalking) return;

        currentLine = 0;
        isTalking = true;
        LockControls();
        dialoguePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        npcImage.SetActive(true);
        playerImage.SetActive(false);

        bool isActive = QuestManager.instance.IsQuestActive(questData.questID);
        bool isCompleted = QuestManager.instance.IsQuestCompleted(questData.questID);

        int collectedAmount = GetCurrentItemCount();
        int requiredAmount = GetRequiredAmountFromQuest();

        // ✅ Nếu chưa nhận nhiệm vụ
        if (!isActive && !isCompleted)
        {
            dialogueKeys = introDialogueKeys;
            currentState = DialogueState.Intro;
            isReadyToComplete = false;
        }
        // ✅ Đã nhận nhiệm vụ, nhưng chưa hoàn thành chính thức
        else if (isActive)
        {
            if (collectedAmount >= requiredAmount)
            {
                dialogueKeys = completeDialogueKeys;
                currentState = DialogueState.Complete;
                isReadyToComplete = true; // ✅ Đã sẵn sàng hoàn thành
            }
            else
            {
                dialogueKeys = notReadyDialogueKeys;
                currentState = DialogueState.NotReady;
                isReadyToComplete = false;
            }
        }
        else if (isCompleted)
        {
            dialogueKeys = completeDialogueKeys;
            currentState = DialogueState.Complete;
            isReadyToComplete = false; // Không cần thưởng nữa
        }


        ShowLine(currentLine);
    }


    int GetCurrentItemCount()
    {
        if (playerInventory == null || playerInventory.items == null)
            return 0;

        var item = playerInventory.items.FirstOrDefault(i => i != null && i.id.ToString() == requiredItemID);
        int count = item != null ? item.amount : 0;

        Debug.Log($"[NPC DEBUG] Đang có {count} vật phẩm có ID = {requiredItemID}");
        return count;
    }


    int GetRequiredAmountFromQuest()
    {
        var objective = questData.objectives.FirstOrDefault(obj => obj.type == ObjectiveType.CollectItem && obj.targetID == requiredItemID);
        return objective != null ? objective.requiredAmount : 1;
    }

    void RemoveRequiredItems()
    {
        if (playerInventory == null || playerInventory.items == null) return;

        var item = playerInventory.items.FirstOrDefault(i => i != null && i.id.ToString() == requiredItemID);
        int requiredAmount = GetRequiredAmountFromQuest();

        if (item != null && item.amount >= requiredAmount)
        {
            for (int i = 0; i < requiredAmount; i++)
                //playerInventory.RemoveItem(item, true);

            Debug.Log($"[NPC] Đã xóa {requiredAmount} vật phẩm ID {requiredItemID} sau khi hoàn thành nhiệm vụ.");
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

        npcImage.SetActive(currentLine % 2 == 0);
        playerImage.SetActive(currentLine % 2 != 0);

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
        if (!isTyping || string.IsNullOrEmpty(currentFullText)) return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentFullText;
        isTyping = false;
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
        npcImage.SetActive(false);
        playerImage.SetActive(false);
        UnlockControls();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentState == DialogueState.Intro)
        {
            QuestManager.instance.StartQuest(questData);
            Debug.Log("[NPC] Đã nhận nhiệm vụ sau hội thoại intro.");
        }

        if (currentState == DialogueState.Complete && !hasGivenReward && isReadyToComplete)
        {
            RemoveRequiredItems();

            if (mapPieceReward && rewardSpawnPoint)
                Instantiate(mapPieceReward, rewardSpawnPoint.position, Quaternion.identity);

            if (portalObject)
                portalObject.SetActive(true);

            QuestManager.instance.CompleteQuest(questData); // ✅ Chỉ gọi sau hội thoại hoàn thành

            hasGivenReward = true;
        }





        currentLine = 0;
        isTyping = false;
        typingCoroutine = null;
        currentFullText = "";
        currentState = DialogueState.None;

        if (currentState == DialogueState.Complete && !QuestManager.instance.IsQuestCompleted(questData.questID))
        {
            QuestManager.instance.CompleteQuest(questData);  // ✅ Hoàn thành sau khi nói xong
        }
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
