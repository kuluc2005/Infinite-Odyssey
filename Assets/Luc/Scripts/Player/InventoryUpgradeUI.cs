using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using Invector.vCharacterController;
using Invector.vMelee;

[System.Serializable]
public class UpgradeStats  
{
    public int damageIncrease;
    public int staminaDecrease;
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

    [Header("Tỉ lệ nền (base)")]
    [Range(0f, 1f)] public float successRate = 0.3f; 
    [Tooltip("Nếu bật, thất bại vẫn trừ Gold. Nếu tắt, chỉ trừ khi thành công.")]
    public bool consumeOnFail = true;

    [Header("Tăng tỉ lệ theo vàng thêm (Extra Gold)")]
    public TMP_InputField goldExtraInput;    // Ô nhập vàng đầu tư thêm
    public TMP_Text rateDetailText;          // Tùy chọn: hiển thị base -> effective
    [Range(0f, 1f)] public float maxRate = 0.95f;     // trần tỉ lệ
    [Range(0.1f, 5f)] public float curveK = 1.2f;     // độ gắt của đường cong
    [Range(1f, 10f)] public float maxExtraMultiplier = 3f; // E tối đa = 3*C

    [Header("Thông báo nâng cấp (Toast)")]
    public TMP_Text resultText;
    public Color successColor = new Color(0.2f, 0.8f, 0.2f);
    public Color errorColor = new Color(0.9f, 0.2f, 0.2f);
    [Range(0.5f, 10f)] public float messageDuration = 3f;
    [Range(0f, 2f)] public float messageFadeOut = 0.35f;

    // ==== Nội bộ ====
    private vItem currentSelectedItem;
    private readonly Dictionary<int, int> baseDamageTable = new Dictionary<int, int>();
    private Coroutine messageCo;
    private Canvas _upgradeCanvasCmp;
    private Canvas _resultCanvas;
    private CanvasGroup _resultGroup;
    private bool _suppressClearMessage = false;

    private readonly Dictionary<Behaviour, bool> _prevEnabledMap = new Dictionary<Behaviour, bool>();

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

        if (goldExtraInput != null)
            goldExtraInput.onValueChanged.AddListener(_ => UpdateRatePreview());

        ClearMessage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (upgradeCanvas != null)
            {
                bool isOpen = upgradeCanvas.activeSelf;
                if (isOpen) CloseUpgrade(); else OpenUpgrade();
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
            if (!IsWeapon(item)) continue; 

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

            if (healthText != null) healthText.gameObject.SetActive(false);

            int calculatedLevel = CalculateLevel(item.id, damageVal);
            if (levelBadge != null)
                levelBadge.text = $"Lv.{calculatedLevel}";

            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnItemClicked(item));
        }
    }

    private bool IsWeapon(vItem item)
    {
        if (item == null) return false;

        var hasDamageAttr = item.GetItemAttribute(vItemAttributes.Damage) != null;
        if (hasDamageAttr) return true;

        bool canEquipInHand = false;
        if (playerItemManager && playerItemManager.inventory && playerItemManager.inventory.equipAreas != null)
        {
            foreach (var area in playerItemManager.inventory.equipAreas)
            {
                if (area == null || area.equipSlots == null) continue;
                if (area.equipPointName == "RightArm" || area.equipPointName == "LeftArm")
                {
                    if (area.equipSlots.Exists(slot => slot.isValid && slot.itemType.Contains(item.type)))
                    {
                        canEquipInHand = true;
                        break;
                    }
                }
            }
        }
        if (canEquipInHand) return true;

        if (item.originalObject && item.originalObject.GetComponent<Invector.vMelee.vMeleeWeapon>() != null)
            return true;

        return false;
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
            int have = GoldManager.Instance ? GoldManager.Instance.CurrentGold : 0;
            if (coinText) coinText.text = $"{have} / {nextStats.goldCost}";

            int previewDamage = damageVal + nextStats.damageIncrease;
            int previewStamina = Mathf.Max(0, staminaVal - nextStats.staminaDecrease);

            if (rightAttackPreview) rightAttackPreview.text = $"Damage: {previewDamage}";
            if (rightStaminaPreview) rightStaminaPreview.text = $"StaminaCost: {previewStamina}";
        }
        else
        {
            if (coinText) coinText.text = "MAX";
            if (rightAttackPreview) rightAttackPreview.text = "MAX";
            if (rightStaminaPreview) rightStaminaPreview.text = "MAX";
        }

        UpdateRatePreview();
        OpenUpgrade();
    }

    void OnUpgradeClicked()
    {
        if (currentSelectedItem == null)
        {
            ShowMessage("No item selected!", false);
            return;
        }

        int damageVal = GetAttributeValue(currentSelectedItem, vItemAttributes.Damage);
        int currentLevel = CalculateLevel(currentSelectedItem.id, damageVal);

        if (currentLevel >= maxLevel)
        {
            ShowMessage("Maximum level reached!", false);
            return;
        }

        if (upgradeTable == null || upgradeTable.Count < currentLevel)
        {
            ShowMessage("Upgrade table has insufficient data!", false);
            return;
        }

        UpgradeStats stats = upgradeTable[currentLevel - 1];

        int extraClamped;
        float effectiveRate = ComputeEffectiveRate(currentLevel, stats.goldCost, out extraClamped);

        if (GoldManager.Instance == null)
        {
            ShowMessage("GoldManager chưa sẵn sàng!", false);
            return;
        }

        int currentGold = GoldManager.Instance.CurrentGold;
        int totalCost = stats.goldCost + extraClamped;
        if (currentGold < totalCost)
        {
            ShowMessage("Not enough Gold for total cost!", false);
            return;
        }

        bool isSuccess = Random.value < effectiveRate;

        void SpendAllGold() => GoldManager.Instance.SpendCoins(totalCost);

        if (consumeOnFail)
        {
            SpendAllGold();
        }

        if (isSuccess)
        {
            if (!consumeOnFail)
                SpendAllGold();

            var dmgAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.Damage);
            var staminaAttr = currentSelectedItem.GetItemAttribute(vItemAttributes.StaminaCost);

            if (dmgAttr != null) dmgAttr.value += stats.damageIncrease;
            if (staminaAttr != null) staminaAttr.value = Mathf.Max(0, staminaAttr.value - stats.staminaDecrease);

            _suppressClearMessage = true;
            RefreshInventoryUI();
            OnItemClicked(currentSelectedItem);
            _suppressClearMessage = false;

            ShowMessage($"Upgrade successful → Lv.{currentLevel + 1}", true);

            var sync = FindFirstObjectByType<InventorySyncManager>();
            if (sync != null) sync.SaveInventoryToServer();
        }
        else
        {
            _suppressClearMessage = true;
            RefreshInventoryUI();
            OnItemClicked(currentSelectedItem);
            _suppressClearMessage = false;

            ShowMessage("Upgrade failed! Try again.", false);
        }
    }

    private float ComputeEffectiveRate(int level, int baseGoldCost, out int extraGoldClamped)
    {
        float baseRate = Mathf.Clamp01(successRate);
        float cap = Mathf.Clamp01(maxRate);

        int extra = 0;
        if (goldExtraInput && !string.IsNullOrEmpty(goldExtraInput.text))
            int.TryParse(goldExtraInput.text, out extra);

        int maxExtra = Mathf.RoundToInt(maxExtraMultiplier * Mathf.Max(1, baseGoldCost));
        extraGoldClamped = Mathf.Clamp(extra, 0, maxExtra);

        if (baseGoldCost <= 0) return baseRate;

        float t = 1f - Mathf.Exp(-curveK * (extraGoldClamped / (float)baseGoldCost));
        float p = baseRate + t * (cap - baseRate);
        return Mathf.Clamp01(p);
    }

    private void UpdateRatePreview()
    {
        if (currentSelectedItem == null)
        {
            if (successRateText) successRateText.text = "Success: --";
            if (rateDetailText) rateDetailText.text = ""; 
            return;
        }

        int damageVal = GetAttributeValue(currentSelectedItem, vItemAttributes.Damage);
        int level = CalculateLevel(currentSelectedItem.id, damageVal);

        if (upgradeTable == null || level > upgradeTable.Count || level <= 0)
        {
            if (successRateText) successRateText.text = "Success: --";
            if (rateDetailText) rateDetailText.text = "";
            return;
        }

        var stats = upgradeTable[level - 1];

        int extraClamped;
        float effective = ComputeEffectiveRate(level, stats.goldCost, out extraClamped);
        int basePct = Mathf.RoundToInt(Mathf.Clamp01(successRate) * 100f);
        int effPct = Mathf.RoundToInt(effective * 100f);
        int delta = Mathf.Max(0, effPct - basePct);

        if (successRateText)
        {
            if (delta > 0) successRateText.text = $"{effPct}% (+{delta}%)";
            else successRateText.text = $"{effPct}%";
        }

        if (coinText)
        {
            int have = GoldManager.Instance ? GoldManager.Instance.CurrentGold : 0;
            int total = stats.goldCost + extraClamped;
            coinText.text = $"Cost: {total}   Have: {have}";
        }

        if (rateDetailText)
        {
            rateDetailText.text = ""; 
        }
    }


    public void ForceRefresh() => RefreshInventoryUI();

    public void OpenUpgrade()
    {
        if (!upgradeCanvas) return;
        upgradeCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameplayInputGate.IsBlocked = true;

        ClearMessage();
    }

    public void CloseUpgrade()
    {
        if (!upgradeCanvas) return;
        upgradeCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameplayInputGate.IsBlocked = false;
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

    // ===== INPUT FREEZE =====
    private void ToggleBehaviour<T>(GameObject player, bool active) where T : Behaviour
    {
        var comp = player.GetComponentInChildren<T>(true);
        if (comp == null) return;

        if (!active)
        {
            if (!_prevEnabledMap.ContainsKey(comp))
                _prevEnabledMap[comp] = comp.enabled;
            comp.enabled = false;
        }
        else
        {
            if (_prevEnabledMap.TryGetValue(comp, out bool wasEnabled))
            {
                comp.enabled = wasEnabled;
                _prevEnabledMap.Remove(comp);
            }
        }
    }

    void OnDisable()
    {
        GameplayInputGate.IsBlocked = false;
    }
}
