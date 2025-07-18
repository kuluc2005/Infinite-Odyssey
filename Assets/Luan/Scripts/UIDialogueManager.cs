using UnityEngine;
using TMPro;

public class UIDialogueManager : MonoBehaviour
{
    public static UIDialogueManager Instance { get; private set; }

    [Header("UI Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public GameObject skipButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
}