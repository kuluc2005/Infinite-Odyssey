// PlayerStats.cs
using UnityEngine;
using System.Collections;
using Invector.vCharacterController;
using System.Reflection;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public int currentHP;
    public int maxHP;
    public int currentMP;
    public int maxMP;
    public int level;
    public int currentExp;
    public int expToLevelUp = 100;
    private bool _isDeadHandled = false;

    void Awake()
    {
        StartCoroutine(InitWithDelay());
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    IEnumerator InitWithDelay()
    {
        yield return null;
        yield return InitStatsAfterProfileLoaded();
    }

    IEnumerator InitStatsAfterProfileLoaded()
    {
        while (ProfileManager.CurrentProfile == null) yield return null;
        var profile = ProfileManager.CurrentProfile;
        level = profile.level;
        maxHP = profile.maxHP;
        maxMP = profile.maxMP;
        currentExp = profile.exp;
        expToLevelUp = 100 + (level - 1) * 50;
        currentHP = profile.HP <= 0 ? maxHP : profile.HP;
        currentMP = profile.MP <= 0 ? maxMP : profile.MP;
        profile.HP = currentHP;
        profile.MP = currentMP;

        var controller = GetComponent<vThirdPersonController>();
        if (controller != null) ForceUpdateStatsToInvector(controller, maxHP, maxMP, currentHP, currentMP);

        var expUIManager = GetComponentInChildren<ExpUIManager>();
        if (expUIManager != null) expUIManager.UpdateUI();
    }

    void OnDisable()
    {
        SaveExpImmediate();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnApplicationQuit()
    {
        SaveExpImmediate();
    }

    void OnActiveSceneChanged(Scene oldS, Scene newS)
    {
        SaveExpImmediate();
    }

    void SaveExpImmediate()
    {
        var ppm = GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            ppm.UpdateProfile(p =>
            {
                p.exp = currentExp;
                p.maxHP = maxHP;
                p.maxMP = maxMP;
                p.HP = currentHP;
                p.MP = currentMP;
            });
        }
        if (ProfileManager.CurrentProfile != null)
        {
            ProfileManager.CurrentProfile.exp = currentExp;
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToLevelUp)
            LevelUp();
        else
            SyncExp();
        var expUIManager = GetComponentInChildren<ExpUIManager>();
        if (expUIManager != null) expUIManager.UpdateUI();
    }

    void SyncExp()
    {
        var ppm = GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            ppm.UpdateProfile(profile => { profile.exp = currentExp; });
        }
        if (ProfileManager.CurrentProfile != null) ProfileManager.CurrentProfile.exp = currentExp;
    }

    void LevelUp()
    {
        level++;
        currentExp = 0;
        expToLevelUp += 50;
        maxHP += 20;
        maxMP += 10;
        currentHP = maxHP;
        currentMP = maxMP;

        var controller = GetComponent<vThirdPersonController>();
        if (controller != null) ForceUpdateStatsToInvector(controller, maxHP, maxMP, currentHP, currentMP, true);

        var ppm = GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            ppm.UpdateProfile(profile =>
            {
                profile.level = level;
                profile.maxHP = maxHP;
                profile.maxMP = maxMP;
                profile.HP = currentHP;
                profile.MP = currentMP;
                profile.exp = currentExp;
            });
        }

        var expUIManager = GetComponentInChildren<ExpUIManager>();
        if (expUIManager != null) expUIManager.UpdateUI();
    }

    void ForceUpdateStatsToInvector(vThirdPersonController controller, int newMaxHP, int newMaxMP, int newCurrentHP, int newCurrentMP, bool debugFields = false)
    {
        var type = controller.GetType();
        var maxHealthField = type.GetField("_maxHealth", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (maxHealthField != null) maxHealthField.SetValue(controller, newMaxHP);
        var maxStaminaField = type.GetField("_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (maxStaminaField != null) maxStaminaField.SetValue(controller, newMaxMP);
        controller.ChangeHealth(newCurrentHP);
        controller.ChangeStamina(newCurrentMP);
        if (debugFields)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in fields) { var n = f.Name.ToLower(); if (n.Contains("health")) _ = f.GetValue(controller); }
        }
    }

    public void AddCoins(int amount)
    {
        if (ProfileManager.CurrentProfile == null) return;
        ProfileManager.CurrentProfile.coins += amount;
        GoldManager.Instance.AddCoins(amount);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0 && !_isDeadHandled)
        {
            _isDeadHandled = true;
            currentHP = 0;
            string curLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("LastResult", "Lose");
            PlayerPrefs.SetString("LastLevel", curLevel);
            UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
        }
    }

    public IEnumerator SaveExpBeforeSceneChange(System.Action onDone)
    {
        var ppm = GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            bool finished = false;
            ppm.UpdateProfile(profile => { profile.exp = currentExp; });
            StartCoroutine(WaitForProfileUpdate(() => { finished = true; }));
            yield return new WaitUntil(() => finished);
        }
        onDone?.Invoke();
    }

    IEnumerator WaitForProfileUpdate(System.Action onComplete)
    {
        yield return null;
        yield return new WaitForSeconds(1f);
        onComplete?.Invoke();
    }
}
