using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Invector.vItemManager;
using Invector.vCharacterController;
using Invector.vMelee;

public class ShopManager : MonoBehaviour
{
    [Header("Item List")]
    public List<ShopItem> shopItems;

    [Header("Slot Holders")]
    public List<GameObject> meleeSlots;
    public List<GameObject> consumableSlots;

    [Header("Detail Panel (Right Side)")]
    public Image iconDisplay;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public Button buyButton;

    [Header("Player Info")]
    public TMP_Text coinText;

    [Header("Chặn input khi mở UI")]
    [Tooltip("Nếu bật, đóng băng thời gian (Time.timeScale=0) khi mở Shop.")]
    public bool pauseGameTime = false;
    [Tooltip("Nếu bật, tắt input/movement/attack của Player khi Shop mở.")]
    public bool blockPlayerInput = true;

    private float _prevTimeScale = 1f;

    private readonly Dictionary<Behaviour, bool> _prevEnabledMap = new Dictionary<Behaviour, bool>();

    private vItemManager playerItemManager;

    private ShopItem currentSelectedItem;
    public GameObject shopCanvas;
    public static bool IsAnyShopOpen = false;

    public bool IsShopOpen => shopCanvas != null && shopCanvas.activeSelf;

    void Start()
    {
        playerItemManager = FindFirstObjectByType<vItemManager>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateCoinUI();

        int meleeIndex = 0;
        int consumableIndex = 0;

        foreach (var item in shopItems)
        {
            GameObject slot = null;

            if (item.type == ItemType.Melee && meleeIndex < meleeSlots.Count)
                slot = meleeSlots[meleeIndex++];
            else if (item.type == ItemType.Consumable && consumableIndex < consumableSlots.Count)
                slot = consumableSlots[consumableIndex++];

            if (slot != null)
            {
                var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                var name = slot.transform.Find("Name")?.GetComponent<TMP_Text>();

                if (icon != null) icon.sprite = item.icon;
                if (name != null) name.text = item.itemName;

                Button btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log("Đã click nút: " + item.itemName);
                        ShowItemDetails(item);
                    });
                }
            }
        }

        if (buyButton != null)
            buyButton.onClick.AddListener(BuyItem);
    }

    void Update()
    {
        if (IsShopOpen && Input.GetKeyDown(KeyCode.B))
        {
            CloseShop();
        }
    }

    void ShowItemDetails(ShopItem item)
    {
        currentSelectedItem = item;

        if (iconDisplay != null) iconDisplay.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;

        string displayText = $"{item.description}\n";

        if (playerItemManager != null)
        {
            vItem vitem = playerItemManager.itemListData.items.Find(i => i.id == item.itemID);
            if (vitem != null)
            {
                var dmgAttr = vitem.GetItemAttribute(vItemAttributes.Damage);

                var staminaAttr = vitem.GetItemAttribute(vItemAttributes.StaminaCost);
                if (staminaAttr == null)
                    staminaAttr = vitem.GetItemAttribute(vItemAttributes.MaxStamina);

                var healthAttr = vitem.GetItemAttribute(vItemAttributes.Health);

                if (item.type == ItemType.Melee)
                {
                    if (dmgAttr != null)
                        displayText += $"<color=#FFD700>Damage: </color> {dmgAttr.value}\n";
                    if (staminaAttr != null)
                        displayText += $"<color=#00FF00>Stamina Cost: </color> {staminaAttr.value}\n";
                }
                else if (item.type == ItemType.Consumable)
                {
                    if (item.itemName.ToLower().Contains("health"))
                    {
                        if (healthAttr != null)
                            displayText += $"<color=#FF4C4C>Health: </color> {healthAttr.value}\n";
                    }
                    else if (item.itemName.ToLower().Contains("stamina"))
                    {
                        if (staminaAttr != null)
                            displayText += $"<color=#00FF00>Stamina: </color> {staminaAttr.value}\n";
                    }
                }
                else
                {
                    if (dmgAttr != null)
                        displayText += $"<color=#FFD700>Damage: </color> {dmgAttr.value}\n";
                    if (staminaAttr != null)
                        displayText += $"<color=#00FF00>Stamina Cost: </color> {staminaAttr.value}\n";
                    if (healthAttr != null)
                        displayText += $"<color=#FF4C4C>Health: </color> {healthAttr.value}\n";
                }
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy vItem ID {item.itemID} trong ItemManager!");
            }
        }

        if (descriptionText != null)
            descriptionText.text = displayText;

        if (priceText != null) priceText.text = item.price.ToString();
    }

    void BuyItem()
    {
        if (currentSelectedItem == null) return;

        if (GoldManager.Instance != null && GoldManager.Instance.SpendCoins(currentSelectedItem.price))
        {
            UpdateCoinUI();
            Debug.Log("Bought: " + currentSelectedItem.itemName);

            if (playerItemManager != null)
            {
                var itemRef = new ItemReference(currentSelectedItem.itemID);
                itemRef.amount = 1;
                itemRef.addToEquipArea = false;
                playerItemManager.AddItem(itemRef, true);

                FindFirstObjectByType<InventorySyncManager>()?.SaveInventoryToServer();
            }
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    void UpdateCoinUI()
    {
        if (coinText != null && GoldManager.Instance != null)
            coinText.text = "Gold: " + GoldManager.Instance.CurrentGold.ToString();
    }

    public void OpenShop()
    {
        if (shopCanvas != null)
            shopCanvas.SetActive(true);

        IsAnyShopOpen = true;

        if (pauseGameTime)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        if (blockPlayerInput)
        {
            SetGameplayInputActive(false);
        }
        GameplayInputGate.IsBlocked = true;   

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateCoinUI();
    }

    public void CloseShop()
    {
        if (shopCanvas != null)
            shopCanvas.SetActive(false);

        IsAnyShopOpen = false;

        // === MỞ KHÓA LẠI ===
        if (pauseGameTime)
            Time.timeScale = _prevTimeScale;
        if (blockPlayerInput)
            SetGameplayInputActive(true);
        GameplayInputGate.IsBlocked = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ===== INPUT FREEZE =====
    private void SetGameplayInputActive(bool active)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        ToggleBehaviour<vThirdPersonController>(player, active);
        ToggleBehaviour<vMeleeManager>(player, active);
        ToggleBehaviour<PlayerStaffSkillManager>(player, active);

    }

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
        if (pauseGameTime) Time.timeScale = _prevTimeScale;
        if (blockPlayerInput) SetGameplayInputActive(true);
        GameplayInputGate.IsBlocked = false;
    }
}
