using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpUIManager : MonoBehaviour
{
    private bool isUIVisible = true;
    public GameObject expUIRoot;

    [Header("UI References")]
    public Image expFillImage;             // Kéo "Filler" vào đây (Image Type = Filled, Fill Method = Horizontal)
    public TextMeshProUGUI expText;        // Kéo Text hiện số XP vào đây
    public TextMeshProUGUI levelText;      // Kéo Text hiện Level vào đây

    [Header("Data Reference")]
    public PlayerStats playerStats;        // Auto gán nếu để trống

    void Awake()
    {
        // Tự động tìm PlayerStats nếu chưa kéo reference
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    void Start()
    {
        UpdateUI();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        if (expUIRoot != null)
            expUIRoot.SetActive(isUIVisible);
    }


    public void UpdateUI()
    {
        if (playerStats == null)
        {
            Debug.LogWarning($"{name}: Chưa gán playerStats cho ExpUIManager!");
            return;
        }

        // Cập nhật thanh XP
        if (expFillImage != null)
            expFillImage.fillAmount = (float)playerStats.currentExp / Mathf.Max(1, playerStats.expToLevelUp);

        // Cập nhật số XP
        if (expText != null)
            expText.text = $"{playerStats.currentExp} / {playerStats.expToLevelUp} XP";

        // Cập nhật Level
        if (levelText != null)
            levelText.text = $"Lv. {playerStats.level}";
    }
}
