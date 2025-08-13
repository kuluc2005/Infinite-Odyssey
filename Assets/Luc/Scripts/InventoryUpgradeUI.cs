// InventoryUpgradeUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;

[System.Serializable]
public class UpgradeStats
{
    public int damageIncrease;
    public int staminaDecrease;
    public int gemCost;
    public int goldCost;
}

public class InventoryUpgradeUI : MonoBehaviour
{
    [Header("Tham chiếu Inventory")]
    public vItemManager playerItemManager;

    [Header("ScrollView Bên Phải")]
    public Transform itemListParent;
    public GameObject itemButtonPrefab;

    [Header("Panel Bên Trái (Upgrade)")]
    public Image leftIconCurrent;
    public Image leftIconNext;

    public TMP_Text leftLevelText;
    public TMP_Text rightLevelText;

    public TMP_Text gemCountText;
    public TMP_Text coinText;
    public TMP_Text leftAttack;
    public TMP_Text rightAttackPreview;
    public TMP_Text leftStamina;
    public TMP_Text rightStaminaPreview;
    public TMP_Text successRateText;
    public TMP_Text levelDisplayText;
    public Button upgradeButton;

    [Header("UI")]
    public GameObject upgradeCanvas;

    [Header("Bảng tăng cấp")]
    public List<UpgradeStats> upgradeTable = new List<UpgradeStats>();
    public int maxLevel = 3;

    [Header("Thông báo nâng cấp (Toast)")]
    public TMP_Text resultText;
    public Color successColor = new Color(0.2f, 0.8f, 0.2f);
    public Color errorColor = new Color(0.9f, 0.2f, 0.2f);
    [Range(0.5f, 10f)] public float messageDuration = 3f;
    [Range(0f, 2f)] public float messageFadeOut = 0.35f;

    private vItem currentSelectedItem;
    private int playerGem = 100;

    private readonly Dictionary<int, int> baseDamageTable = new Dictionary<int, int>();

    private Coroutine messageCo;
    private Canvas _upgradeCanvasCmp;
    private Canvas _resultCanvas;
    private CanvasGroup _resultGroup;

    private bool _suppressClearMessage = false;

    void Awake()
    {
        EnsureUpgradeCanvasOverlay();
        EnsureResultOverlay();
    }

    void Start()
    {
        if (playerItemManager == null)
            playerItemManager = FindFirstObjectByType<vItemManager>();

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

                if (!isOpen)
                    ClearMessage();
            }
        }
    }

    public void RefreshInventoryUI()
    {
        if (itemListParent == null || itemButtonPrefab == null || playerItemManager == null)
            return;

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

        if (!_suppressClearMessage)
            ClearMessage();

        if (item == null) return;

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

        if (currentLevel < maxLevel && upgradeTable != null && upgradeTable.Count >= currentLevel)
        {
            UpgradeStats nextStats = upgradeTable[currentLevel - 1];
            if (gemCountText) gemCountText.text = $"{playerGem} / {nextStats.gemCost}";
            int have = GoldManager.Instance ? GoldManager.Instance.CurrentGold : 0;
            if (coinText) coinText.text = $"{have} / {nextStats.goldCost}";

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

        OpenUpgrade();
    }

    void OnUpgradeClicked()
    {
        if (currentSelectedItem == null)
        {
            ShowMessage("Chưa chọn vật phẩm!", false);
            return;
        }

        int damageVal = GetAttributeValue(currentSelectedItem, vItemAttributes.Damage);
        int currentLevel = CalculateLevel(currentSelectedItem.id, damageVal);

        if (currentLevel >= maxLevel)
        {
            ShowMessage("Đã đạt cấp tối đa!", false);
            return;
        }

        if (upgradeTable == null || upgradeTable.Count < currentLevel)
        {
            ShowMessage("Bảng nâng cấp chưa đủ dữ liệu!", false);
            return;
        }

        UpgradeStats stats = upgradeTable[currentLevel - 1];

        if (playerGem < stats.gemCost)
        {
            ShowMessage("Không đủ Gem!", false);
            return;
        }

        if (GoldManager.Instance == null)
        {
            ShowMessage("GoldManager chưa sẵn sàng!", false);
            return;
        }

        if (!GoldManager.Instance.SpendCoins(stats.goldCost))
        {
            ShowMessage("Không đủ Gold!", false);
            return;
        }

        playerGem -= stats.gemCost;

        var dmgAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.Damage);
        var staminaAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.StaminaCost);

        if (dmgAttr != null) dmgAttr.value += stats.damageIncrease;
        if (staminaAttr != null) staminaAttr.value = Mathf.Max(0, staminaAttr.value - stats.staminaDecrease);

        _suppressClearMessage = true;
        RefreshInventoryUI();
        OnItemClicked(currentSelectedItem);
        _suppressClearMessage = false;

        ShowMessage($"Nâng cấp thành công → Lv.{currentLevel + 1}", true);

        var sync = FindFirstObjectByType<InventorySyncManager>();
        if (sync != null) sync.SaveInventoryToServer();
    }

    public void ForceRefresh() => RefreshInventoryUI();

    public void OpenUpgrade()
    {
        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

    private void ShowMessage(string msg, bool isSuccess)
    {
        if (!resultText) return;

        if (messageCo != null) StopCoroutine(messageCo);

        EnsureResultOverlay();

        resultText.text = msg;
        resultText.color = isSuccess ? successColor : errorColor;

        if (upgradeCanvas) resultText.transform.SetParent(upgradeCanvas.transform, true);
        resultText.transform.SetAsLastSibling();

        if (_resultGroup) _resultGroup.alpha = 1f;

        resultText.raycastTarget = false;
        resultText.gameObject.SetActive(true);

        messageCo = StartCoroutine(MessageRoutine());
    }

    private IEnumerator MessageRoutine()
    {
        float timer = 0f;
        while (timer < messageDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_resultGroup && messageFadeOut > 0f)
        {
            float t = 0f;
            while (t < messageFadeOut)
            {
                t += Time.unscaledDeltaTime;
                _resultGroup.alpha = 1f - (t / messageFadeOut);
                yield return null;
            }
        }

        if (_resultGroup) _resultGroup.alpha = 0f;
        resultText.gameObject.SetActive(false);
        messageCo = null;
    }

    private void ClearMessage()
    {
        if (messageCo != null)
        {
            StopCoroutine(messageCo);
            messageCo = null;
        }
        if (_resultGroup) _resultGroup.alpha = 0f;
        if (resultText) resultText.gameObject.SetActive(false);
    }

    private void EnsureUpgradeCanvasOverlay()
    {
        if (!upgradeCanvas) return;

        _upgradeCanvasCmp = upgradeCanvas.GetComponent<Canvas>();
        if (_upgradeCanvasCmp == null) _upgradeCanvasCmp = upgradeCanvas.AddComponent<Canvas>();

        _upgradeCanvasCmp.renderMode = RenderMode.ScreenSpaceOverlay;
        _upgradeCanvasCmp.overrideSorting = true;
        if (_upgradeCanvasCmp.sortingOrder < 500) _upgradeCanvasCmp.sortingOrder = 500;
    }

    private void EnsureResultOverlay()
    {
        if (!resultText) return;

        _resultCanvas = resultText.GetComponent<Canvas>();
        if (_resultCanvas == null) _resultCanvas = resultText.gameObject.AddComponent<Canvas>();

        _resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _resultCanvas.overrideSorting = true;

        int baseOrder = _upgradeCanvasCmp ? _upgradeCanvasCmp.sortingOrder : 500;
        _resultCanvas.sortingOrder = baseOrder + 1;

        _resultGroup = resultText.GetComponent<CanvasGroup>();
        if (_resultGroup == null) _resultGroup = resultText.gameObject.AddComponent<CanvasGroup>();
        _resultGroup.ignoreParentGroups = true;
        _resultGroup.alpha = 0f;
        _resultGroup.interactable = false;
        _resultGroup.blocksRaycasts = false;
    }

    private int CalculateLevel(int itemId, int currentDamage)
    {
        if (!baseDamageTable.ContainsKey(itemId))
            return 1;

        int baseDamage = baseDamageTable[itemId];
        int extraDamage = currentDamage - baseDamage;

        if (extraDamage < 5) return 1;
        else if (extraDamage < 15) return 2;
        else return 3;
    }

    private int GetAttributeValue(vItem item, vItemAttributes attr)
    {
        var attribute = item.GetItemAttribute(attr);
        return attribute != null ? attribute.value : 0;
    }
}
