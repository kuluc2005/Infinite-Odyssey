using UnityEngine;
using Invector.vItemManager;
using System.Collections;

public class InventoryRestoreOnSceneLoad : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForPlayerAndLoad());
    }

    IEnumerator WaitForPlayerAndLoad()
    {
        GameObject player = null;
        float timer = 0f;

        // Đợi Player spawn
        while (player == null && timer < 3f)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            timer += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            var inv = player.GetComponent<vItemManager>();

            if (inv != null && InventorySaveManager.instance != null)
            {
                // ✅ Load lại item
                InventorySaveManager.instance.LoadInventory(inv);

                // ✅ Tìm UI và cập nhật lại
                var inventoryUI = FindObjectOfType<Invector.vItemManager.vInventory>();
                if (inventoryUI != null)
                {
                    // 👉 Gán itemManager bằng reflection (không đụng vào mã gốc)
                    var itemManagerField = typeof(Invector.vItemManager.vInventory)
                        .GetField("itemManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (itemManagerField != null)
                    {
                        itemManagerField.SetValue(inventoryUI, inv);
                        Debug.Log("✅ Gán itemManager qua Reflection thành công.");
                    }
                    inventoryUI.UpdateInventory();              // 🔁 Làm mới UI
                    inventoryUI.gameObject.SetActive(true);     // 👁️ Hiện giao diện
                }
                Debug.Log("[InventoryRestore] ✅ LoadInventory hoàn tất");
            }
        }
        else
        {
            Debug.LogWarning("[InventoryRestore] ❌ Không tìm thấy Player sau khi vào scene mới");
        }
    }
}
