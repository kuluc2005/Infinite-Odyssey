using UnityEngine;
using TMPro;

public class QuestPanelToggle : MonoBehaviour
{
    public GameObject questPanel;
    public GameObject toggleButton;
    public GameObject badge;
    public TextMeshProUGUI badgeText;

    private bool isPanelOpen = false;

    void Start()
    {
        questPanel.SetActive(false);
        toggleButton.SetActive(true);
        UpdateBadge();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleQuestPanel(); // 👉 bấm N thì cũng như click vào icon
        }
    }

    public void ToggleQuestPanel()
    {
        isPanelOpen = !isPanelOpen;
        questPanel.SetActive(isPanelOpen);
        toggleButton.SetActive(!isPanelOpen);

        if (!isPanelOpen)
        {
            UpdateBadge();
        }
    }

    public void OpenQuestPanel()
    {
        isPanelOpen = true;
        questPanel.SetActive(true);
        toggleButton.SetActive(false);
    }

    public void CloseQuestPanel()
    {
        isPanelOpen = false;
        questPanel.SetActive(false);
        toggleButton.SetActive(true);
        UpdateBadge();
    }

    public void UpdateBadge()
    {
        int questCount = QuestManager.instance != null ? QuestManager.instance.activeQuests.Count : 0;
        Debug.Log($"[QuestPanelToggle] Cập nhật badge, questCount: {questCount}");
        if (questCount > 0)
        {
            badge.SetActive(true);
            badgeText.text = questCount.ToString();
        }
        else
        {
            badge.SetActive(false);
        }
    }
}