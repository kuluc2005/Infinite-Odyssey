// LevelPortal.cs (instant load, non-blocking save)
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName;
    public string nextLevelName;
    public bool instant = true;   // true = vào cổng là chuyển scene ngay

    bool _processing;

    void OnTriggerEnter(Collider other)
    {
        if (_processing) return;
        if (!other.CompareTag("Player")) return;
        _processing = true;

        PlayerPrefs.SetString("NextLevel", nextLevelName);

        SaveAllNoBlock(other.gameObject);  

        if (instant)
        {
            SceneManager.LoadScene(cutsceneSceneName);  
        }
        else
        {
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }

    void SaveAllNoBlock(GameObject player)
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

                p.lastScene = nextLevelName;
            });
        }

        if (GoldManager.Instance != null)
            CoroutineRunner.Run(GoldManager.Instance.UpdateCoinsToAPI());
    }
}
