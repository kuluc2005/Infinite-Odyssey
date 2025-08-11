using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;

[System.Serializable]
public class UpgradeStats
{
    public int damageIncrease;    // ✅ Damage tăng mỗi cấp
    public int staminaDecrease;   // ✅ StaminaCost giảm mỗi cấp
    public int gemCost;           // ✅ Gem cần để nâng cấp
    public int goldCost;          // ✅ Vàng cần để nâng cấp
}

public class InventoryUpgradeUI : MonoBehaviour
{
    [Header("📜 Tham chiếu Inventory")]
    public vItemManager playerItemManager;

    [Header("📜 ScrollView Bên Phải")]
    public Transform itemListParent;
    public GameObject itemButtonPrefab;

    [Header("⬅️ Panel Bên Trái (Upgrade)")]
    public Image leftIconCurrent;
    public Image leftIconNext;

    public TMP_Text leftLevelText;
    public TMP_Text rightLevelText;

    public TMP_Text gemCountText;
    public TMP_Text coinText;   // 👉 Text con trong GameObject Coin (chỉ hiển thị số vàng phải trả)
    public TMP_Text leftAttack;
    public TMP_Text rightAttackPreview;
    public TMP_Text leftStamina;
    public TMP_Text rightStaminaPreview;
    public TMP_Text successRateText;
    public TMP_Text levelDisplayText;
    public Button upgradeButton;

    [Header("⚙️ UI")]
    public GameObject upgradeCanvas;

    [Header("📈 Bảng tăng cấp (có thể chỉnh trong Inspector)")]
    public List<UpgradeStats> upgradeTable = new List<UpgradeStats>();
    public int maxLevel = 3;   // chỉ có 3 level

    [Header("🟢/🔴 Thông báo nâng cấp")]
    public TMP_Text resultText;
    public Color successColor = new Color(0.2f, 0.8f, 0.2f);
    public Color errorColor = new Color(0.9f, 0.2f, 0.2f);
    public float messageDuration = 2f;

    private vItem currentSelectedItem;
    private int playerGem = 100;
    private int playerGold = 1000;

    // Lưu damage gốc cho từng vũ khí
    private Dictionary<int, int> baseDamageTable = new Dictionary<int, int>();

    // --- message helpers ---
    private Coroutine messageCo;
    private void ShowMessage(string msg, bool isSuccess)
    {
        if (resultText == null) return;

        if (messageCo != null) StopCoroutine(messageCo);
        resultText.text = msg;
        resultText.color = isSuccess ? successColor : errorColor;
        resultText.gameObject.SetActive(true);
        messageCo = StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }

    private void ClearMessage()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
    // -----------------------

    void Start()
    {
        if (playerItemManager == null)
            playerItemManager = FindObjectOfType<vItemManager>();

        RefreshInventoryUI();
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        ClearMessage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (upgradeCanvas != null)
            {
                bool isOpen = upgradeCanvas.activeSelf;
                upgradeCanvas.SetActive(!isOpen);

                Cursor.lockState = isOpen ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isOpen;

                if (!isOpen) ClearMessage(); // mở panel thì xóa thông báo cũ
            }
        }
    }

    public void RefreshInventoryUI()
    {
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        List<vItem> items = playerItemManager.items;

        foreach (vItem item in items)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemListParent);

            foreach (Transform child in buttonObj.GetComponentsInChildren<Transform>(true))
                child.gameObject.SetActive(true);

            Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            TMP_Text nameText = buttonObj.transform.Find("Name")?.GetComponent<TMP_Text>();
            TMP_Text attackText = buttonObj.transform.Find("Attack")?.GetComponent<TMP_Text>();
            TMP_Text healthText = buttonObj.transform.Find("Health")?.GetComponent<TMP_Text>();
            TMP_Text levelBadge = buttonObj.transform.Find("LevelText")?.GetComponent<TMP_Text>();

            if (icon != null) icon.sprite = item.icon;
            if (nameText != null) nameText.text = item.name;

            int damageVal = GetAttributeValue(item, vItemAttributes.Damage);
            int staminaVal = GetAttributeValue(item, vItemAttributes.StaminaCost);

            if (!baseDamageTable.ContainsKey(item.id))
                baseDamageTable[item.id] = damageVal;

            if (attackText != null) attackText.text = "Damage: " + damageVal;
            if (healthText != null) healthText.text = "StaminaCost: " + staminaVal;

            int calculatedLevel = CalculateLevel(item.id, damageVal);

            if (levelBadge != null)
                levelBadge.text = $"Lv.{calculatedLevel}";

            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnItemClicked(item));
        }
    }

    void OnItemClicked(vItem item)
    {
        currentSelectedItem = item;
        ClearMessage(); // xóa thông báo khi đổi item

        if (leftIconCurrent) leftIconCurrent.sprite = item.icon;
        if (leftIconNext) leftIconNext.sprite = item.icon;

        int damageVal = GetAttributeValue(item, vItemAttributes.Damage);
        int staminaVal = GetAttributeValue(item, vItemAttributes.StaminaCost);

        int currentLevel = CalculateLevel(item.id, damageVal);
        int nextLevel = Mathf.Clamp(currentLevel + 1, 1, maxLevel);
        if (leftLevelText) leftLevelText.text = $"Lv.{currentLevel}";
        if (rightLevelText) rightLevelText.text = currentLevel < maxLevel ? $"Lv.{nextLevel}" : "MAX";

        if (leftAttack) leftAttack.text = $"Damage: {damageVal}";
        if (leftStamina) leftStamina.text = $"StaminaCost: {staminaVal}";

        if (levelDisplayText) levelDisplayText.text = $"Lv. {currentLevel}";

        // HIỂN THỊ GEM & GOLD COST
        if (currentLevel < maxLevel)
        {
            UpgradeStats nextStats = upgradeTable[currentLevel - 1];
            if (gemCountText) gemCountText.text = $"{playerGem} / {nextStats.gemCost}";
            if (coinText) coinText.text = $"{nextStats.goldCost}";

            int previewDamage = damageVal + nextStats.damageIncrease;
            int previewStamina = Mathf.Max(0, staminaVal - nextStats.staminaDecrease);

            if (rightAttackPreview) rightAttackPreview.text = $"Damage: {previewDamage}";
            if (rightStaminaPreview) rightStaminaPreview.text = $"StaminaCost: {previewStamina}";
        }
        else
        {
            if (gemCountText) gemCountText.text = "MAX LEVEL";
            if (coinText) coinText.text = "MAX";
            if (rightAttackPreview) rightAttackPreview.text = "MAX";
            if (rightStaminaPreview) rightStaminaPreview.text = "MAX";
        }

        if (successRateText) successRateText.text = "Tỉ lệ thành công: 100%";

        // đảm bảo lần click đầu mở UI
        OpenUpgrade();
    }

    void OnUpgradeClicked()
    {
        if (currentSelectedItem == null)
        {
            Debug.LogWarning("❌ Chưa chọn item để nâng cấp!");
            ShowMessage("Chưa chọn vật phẩm!", false);
            return;
        }

        int damageVal = GetAttributeValue(currentSelectedItem, vItemAttributes.Damage);
        int currentLevel = CalculateLevel(currentSelectedItem.id, damageVal);

        if (currentLevel >= maxLevel)
        {
            Debug.Log("⚠ Đã đạt cấp tối đa!");
            ShowMessage("Đã đạt cấp tối đa!", false);
            return;
        }

        UpgradeStats stats = upgradeTable[currentLevel - 1];

        if (playerGem < stats.gemCost)
        {
            Debug.Log("❌ Không đủ Gem để nâng cấp!");
            ShowMessage("Không đủ Gem!", false);
            return;
        }

        if (playerGold < stats.goldCost)
        {
            Debug.Log("❌ Không đủ Gold để nâng cấp!");
            ShowMessage("Không đủ Gold!", false);
            return;
        }

        // Trừ Gem & Gold
        playerGem -= stats.gemCost;
        playerGold -= stats.goldCost;

        var dmgAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.Damage);
        var staminaAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.StaminaCost);

        if (dmgAttr != null) dmgAttr.value += stats.damageIncrease;
        if (staminaAttr != null) staminaAttr.value = Mathf.Max(0, staminaAttr.value - stats.staminaDecrease);

        Debug.Log($"✅ {currentSelectedItem.name} → Lv.{currentLevel + 1}: +{stats.damageIncrease} DMG, -{stats.staminaDecrease} STC (Gem {playerGem}, Gold {playerGold})");

        // Thông báo thành công
        ShowMessage($"Nâng cấp thành công → Lv.{currentLevel + 1}", true);

        RefreshInventoryUI();
        OnItemClicked(currentSelectedItem); // cập nhật preview/cost mới
    }

    public void ForceRefresh()
    {
        RefreshInventoryUI();
    }

    public void OpenUpgrade()
    {
        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ClearMessage();
        }
    }

    public void CloseUpgrade()
    {
        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private int CalculateLevel(int itemId, int currentDamage)
    {
        if (!baseDamageTable.ContainsKey(itemId))
            return 1;

        int baseDamage = baseDamageTable[itemId];
        int extraDamage = currentDamage - baseDamage;

        if (extraDamage < 5)
            return 1;
        else if (extraDamage < 15)
            return 2;
        else
            return 3;
    }

    private int GetAttributeValue(vItem item, vItemAttributes attr)
    {
        var attribute = item.GetItemAttribute(attr);
        return attribute != null ? attribute.value : 0;
    }
}