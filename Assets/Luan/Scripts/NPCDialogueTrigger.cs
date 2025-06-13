using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;

public class NPCDialogueTrigger : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public GameObject skipButton;
    public GameObject npcImage;
    public GameObject playerImage;

    [Tooltip("Key từ bảng NPCLines")]
    public string[] dialogueKeys;

    private int currentLine = 0;
    private bool isTalking = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentFullText;

    // Components
    private vThirdPersonCamera tpCamera;
    private vMeleeCombatInput combatInput;
    private vThirdPersonInput playerInput;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private bool originalUseRootMotion;

    void Awake()
    {
        tpCamera = FindObjectOfType<vThirdPersonCamera>();
        combatInput = FindObjectOfType<vMeleeCombatInput>();
        playerInput = FindObjectOfType<vThirdPersonInput>();
        playerAnimator = FindObjectOfType<Animator>();
        playerRigidbody = FindObjectOfType<Rigidbody>();

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
        currentLine = 0;
        isTalking = true;
        dialoguePanel.SetActive(true);
        npcImage.SetActive(true);
        playerImage.SetActive(false);
        ShowLine(currentLine);
        LockControls();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

        // Hiện ảnh theo nhân vật đang nói
        if (currentLine % 2 == 0)
        {
            npcImage.SetActive(true);
            playerImage.SetActive(false);
        }
        else
        {
            npcImage.SetActive(false);
            playerImage.SetActive(true);
        }

        var table = LocalizationSettings.StringDatabase;
        var localizedString = table.GetLocalizedStringAsync("NPCLines", key);
        yield return localizedString;

        currentFullText = localizedString.Result;
        dialogueText.text = "";

        for (int i = 0; i < currentFullText.Length; i++)
        {
            dialogueText.text += currentFullText[i];
            yield return new WaitForSeconds(0.02f); // tốc độ gõ chữ
        }

        isTyping = false;
        typingCoroutine = null;
    }

    public void SkipTyping()
    {
        if (!isTyping || string.IsNullOrEmpty(currentFullText)) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

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
    }

    void LockControls()
    {
        if (tpCamera) tpCamera.enabled = false;

        if (combatInput)
        {
            combatInput.lockInput = true;
            combatInput.cc.input = Vector2.zero;
            combatInput.cc._rigidbody.linearVelocity = Vector3.zero;
        }

        if (playerInput)
            playerInput.enabled = false;

        if (playerRigidbody)
            playerRigidbody.linearVelocity = Vector3.zero;

        if (playerAnimator)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetFloat("InputMagnitude", 0f);
            playerAnimator.SetFloat("InputHorizontal", 0f);
            playerAnimator.SetFloat("InputVertical", 0f);
            playerAnimator.Play("Idle", 0);
        }
    }

    void UnlockControls()
    {
        if (tpCamera) tpCamera.enabled = true;

        if (combatInput)
        {
            combatInput.lockInput = false;
            combatInput.cc.enabled = true;
        }

        if (playerInput)
            playerInput.enabled = true;

        if (playerAnimator)
            playerAnimator.applyRootMotion = originalUseRootMotion;
    }
}
