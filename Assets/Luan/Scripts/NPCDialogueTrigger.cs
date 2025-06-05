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

    [Tooltip("Danh sách key từ bảng NPCLines")]
    public string[] dialogueKeys;

    private int currentLine = 0;
    private bool isTalking = false;

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
        if (isTalking && Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        currentLine = 0;
        isTalking = true;
        dialoguePanel.SetActive(true);
        ShowLine(currentLine);
        LockControls();
    }

    void ShowLine(int index)
    {
        if (index < dialogueKeys.Length)
        {
            StartCoroutine(LoadLocalizedLine(dialogueKeys[index]));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator LoadLocalizedLine(string key)
    {
        var table = LocalizationSettings.StringDatabase;
        var localizedString = table.GetLocalizedStringAsync("NPCLines", key);
        yield return localizedString;

        dialogueText.text = localizedString.Result;
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
    }

    void LockControls()
    {
        if (tpCamera) tpCamera.enabled = false;

        if (combatInput)
        {
            combatInput.lockInput = true;
            combatInput.cc._rigidbody.linearVelocity = Vector3.zero;
            combatInput.cc.input = Vector2.zero;
        }

        if (playerInput)
            playerInput.enabled = false;

        if (playerRigidbody)
            playerRigidbody.linearVelocity = Vector3.one;

        if (playerAnimator)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetFloat("InputMagnitude", 0f);
        }
        if (playerAnimator)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetFloat("InputMagnitude", 0f);
            playerAnimator.SetFloat("InputHorizontal", 0f);
            playerAnimator.SetFloat("InputVertical", 0f);
            playerAnimator.Play("Idle", 0); // đảm bảo animation không bị rơi vào Locomotion hay Walk
        }
    }

    //void UnlockControls()
    //{
    //    if (tpCamera) tpCamera.enabled = true;

    //    if (combatInput)
    //        combatInput.lockInput = false;

    //    if (playerInput)
    //        playerInput.enabled = true;

    //    if (playerAnimator)
    //        playerAnimator.applyRootMotion = originalUseRootMotion;
    //}
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
