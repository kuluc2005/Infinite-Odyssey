using UnityEngine;

public enum ItemType { Melee, Consumable }

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int price;
    public ItemType type;
    public GameObject prefab;

    [Header("Inventory Settings")]
    public int itemID;     // ID trong ItemListData của Invector
    public int quantity = 1; // số lượng mua vào kho
}
