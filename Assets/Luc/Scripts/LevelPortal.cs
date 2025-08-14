using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    [Header("Scene Names")]
    public string cutsceneSceneName;  
    public string nextLevelName;    

    [Header("Options")]
    public bool instant = true;      

    private bool _processing;

    private void OnTriggerEnter(Collider other)
    {
        if (_processing) return;
        if (!other.CompareTag("Player")) return;
        _processing = true;

        // Lưu để ResultScene/Next dùng
        if (!string.IsNullOrEmpty(nextLevelName))
            PlayerPrefs.SetString("NextLevel", nextLevelName);

        SaveAllNoBlock(other.gameObject);


        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (string.IsNullOrEmpty(cutsceneSceneName))
        {
            Debug.LogWarning("[LevelPortal] cutsceneSceneName trống, bỏ qua chuyển cảnh.");
            return;
        }

        if (instant)
        {
            SceneTransition.Load(cutsceneSceneName);
        }
        else
        {
            SceneTransition.Load(cutsceneSceneName);
        }
    }

    /// <summary>
    /// Lưu các chỉ số/Inventory/Profile mà KHÔNG chặn luồng chuyển scene.
    /// </summary>
    private void SaveAllNoBlock(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        var ppm = player.GetComponent<PlayerPositionManager>();
        var inv = player.GetComponent<InventorySyncManager>();

        if (inv != null) inv.SaveInventoryToServer();

        var prof = ProfileManager.CurrentProfile;
        if (prof != null)
        {
            if (stats != null)
            {
                prof.exp = stats.currentExp;
                prof.level = stats.level;
                prof.maxHP = stats.maxHP;
                prof.maxMP = stats.maxMP;
                prof.HP = stats.currentHP;
                prof.MP = stats.currentMP;
            }
            if (GoldManager.Instance != null)
                prof.coins = GoldManager.Instance.CurrentGold;

            if (!string.IsNullOrEmpty(nextLevelName))
                prof.lastScene = nextLevelName;
        }

        if (ppm != null)
        {
            ppm.UpdateProfile(p =>
            {
                if (stats != null)
                {
                    p.exp = stats.currentExp;
                    p.level = stats.level;
                    p.maxHP = stats.maxHP;
                    p.maxMP = stats.maxMP;
                    p.HP = stats.currentHP;
                    p.MP = stats.currentMP;
                }
                if (GoldManager.Instance != null)
                    p.coins = GoldManager.Instance.CurrentGold;

                if (!string.IsNullOrEmpty(nextLevelName))
                    p.lastScene = nextLevelName;
            });
        }

        if (GoldManager.Instance != null)
            CoroutineRunner.Run(GoldManager.Instance.UpdateCoinsToAPI());
    }
}
