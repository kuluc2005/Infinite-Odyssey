using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Invector.vItemManager;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName;
    public string nextLevelName;
    [Range(0f, 3f)] public float saveDelay = 0.6f;

    bool _processing;

    void OnTriggerEnter(Collider other)
    {
        if (_processing) return;
        if (!other.CompareTag("Player")) return;

        _processing = true;
        PlayerPrefs.SetString("NextLevel", nextLevelName);
        StartCoroutine(HandlePortal(other.gameObject));
    }

    IEnumerator HandlePortal(GameObject player)
    {
        yield return SaveAllAndFlush(player);
        SceneManager.LoadScene(cutsceneSceneName);
    }

    IEnumerator SaveAllAndFlush(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        var ppm = player.GetComponent<PlayerPositionManager>();
        var inv = player.GetComponent<InventorySyncManager>();

        if (inv != null) inv.SaveInventoryToServer();

        if (ppm != null)
        {
            int curGold = GoldManager.Instance ? GoldManager.Instance.CurrentGold : (ProfileManager.CurrentProfile != null ? ProfileManager.CurrentProfile.coins : 0);

            ppm.UpdateProfile(profile =>
            {
                if (stats != null)
                {
                    profile.exp = stats.currentExp;
                    profile.level = stats.level;
                    profile.maxHP = stats.maxHP;
                    profile.maxMP = stats.maxMP;
                    profile.HP = stats.currentHP;
                    profile.MP = stats.currentMP;
                }
                profile.coins = curGold;
            });
        }

        if (GoldManager.Instance != null)
            CoroutineRunner.Run(GoldManager.Instance.UpdateCoinsToAPI());

        yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, saveDelay));
    }
}
