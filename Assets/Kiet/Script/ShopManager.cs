using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Invector.vItemManager;

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
    public int playerGold = 1000;

    private ShopItem currentSelectedItem;

    public GameObject shopCanvas;

    private vItemManager playerItemManager;


    public bool IsShopOpen => shopCanvas != null && shopCanvas.activeSelf;
    void Start()
    {
        playerItemManager = FindObjectOfType<vItemManager>();

        // Hiện trỏ chuột để tương tác
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
                        Debug.Log("✅ Đã click nút: " + item.itemName);
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
        if (IsShopOpen && Input.GetKeyDown(KeyCode.Escape))
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

        // 👉 Tìm vItem gốc từ ItemListData trong vItemManager
        if (playerItemManager != null)
        {
            vItem vitem = playerItemManager.itemListData.items.Find(i => i.id == item.itemID);
            if (vitem != null)
            {
                var dmgAttr = vitem.GetItemAttribute(vItemAttributes.Damage);

                // 👉 Lấy StaminaCost trước, nếu không có thì lấy Stamina
                var staminaAttr = vitem.GetItemAttribute(vItemAttributes.StaminaCost);
                if (staminaAttr == null)
                    staminaAttr = vitem.GetItemAttribute(vItemAttributes.MaxStamina);  

                var healthAttr = vitem.GetItemAttribute(vItemAttributes.Health);


                // 🔥 🆕 PHÂN BIỆT THEO ITEM TYPE + TÊN ITEM
                if (item.type == ItemType.Melee)
                {
                    // 👉 Vũ khí chỉ hiển thị Damage + StaminaCost
                    if (dmgAttr != null)
                        displayText += $"<color=#FFD700>Damage: </color> {dmgAttr.value}\n";
                    if (staminaAttr != null)
                        displayText += $"<color=#00FF00>Stamina Cost: </color> {staminaAttr.value}\n";
                }
                else if (item.type == ItemType.Consumable)
                {
                    // 👉 Nếu là Health Potion
                    if (item.itemName.ToLower().Contains("health"))
                    {
                        if (healthAttr != null)
                            displayText += $"<color=#FF4C4C>Health: </color> {healthAttr.value}\n";
                    }
                    // 👉 Nếu là Stamina Potion
                    else if (item.itemName.ToLower().Contains("stamina"))
                    {
                        if (staminaAttr != null)
                            displayText += $"<color=#00FF00>Stamina: </color> {staminaAttr.value}\n";
                    }
                }
                else
                {
                    // 👉 Trường hợp còn lại hiển thị tất cả
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
                Debug.LogWarning($"⚠ Không tìm thấy vItem ID {item.itemID} trong ItemManager!");
            }
        }

        // 👉 Gán text hiển thị lên UI
        if (descriptionText != null)
            descriptionText.text = displayText;

        if (priceText != null) priceText.text = item.price.ToString();
    }



    void BuyItem()
    {
        if (currentSelectedItem == null) return;

        if (playerGold >= currentSelectedItem.price)
        {
            playerGold -= currentSelectedItem.price;
            UpdateCoinUI();
            Debug.Log("✅ Bought: " + currentSelectedItem.itemName);

            if (playerItemManager != null)
            {
                // ✅ Tạo ItemReference từ itemID và số lượng
                var itemRef = new ItemReference(currentSelectedItem.itemID);
                itemRef.amount = 1; // Mua 1 item
                itemRef.addToEquipArea = false; // Không auto trang bị

                // ✅ Thêm item vào kho
                playerItemManager.AddItem(itemRef, true);
                Debug.Log($"👜 Đã thêm item ID {currentSelectedItem.itemID} vào kho.");

                // ✅ 🔥 Refresh UI Upgrade sau khi mua
                InventoryUpgradeUI upgradeUI = FindObjectOfType<InventoryUpgradeUI>();
                if (upgradeUI != null)
                {
                    upgradeUI.ForceRefresh();
                }
            }
        }
        else
        {
            Debug.Log("❌ Not enough gold!");
        }
    }



    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Gold: " + playerGold.ToString();
    }

    public void OpenShop()
    {
        if (shopCanvas != null)
            shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopCanvas != null)
            shopCanvas.SetActive(false);
    }
}
