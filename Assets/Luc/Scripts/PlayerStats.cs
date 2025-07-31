using UnityEngine;
using System.Collections;
using Invector.vCharacterController;
using System.Reflection;

public class PlayerStats : MonoBehaviour
{
    public int currentHP;
    public int maxHP;
    public int currentMP;
    public int maxMP;
    public int level;
    public int currentExp;
    public int expToLevelUp = 100;

    private void Awake()
    {
        StartCoroutine(InitWithDelay());
    }

    private IEnumerator InitWithDelay()
    {
        yield return null; // Đợi Invector chạy Start xong
        yield return InitStatsAfterProfileLoaded();
    }

    private IEnumerator InitStatsAfterProfileLoaded()
    {
        while (ProfileManager.CurrentProfile == null)
        {
            Debug.LogWarning("Đang đợi ProfileManager.CurrentProfile...");
            yield return null;
        }

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
        if (controller != null)
        {
            ForceUpdateStatsToInvector(controller, maxHP, maxMP, currentHP, currentMP);
        }

        if (vHUDController.instance != null)
        {
            vHUDController.instance.Init(controller);
            vHUDController.instance.healthSlider.maxValue = maxHP;
            vHUDController.instance.healthSlider.value = currentHP;
            vHUDController.instance.staminaSlider.maxValue = maxMP;
            vHUDController.instance.staminaSlider.value = currentMP;
        }

        var expUIManager = GetComponentInChildren<ExpUIManager>();
        if (expUIManager != null)
        {
            expUIManager.UpdateUI();
        }

        Debug.Log($"HP/MP đã gán: {currentHP}/{maxHP} - {currentMP}/{maxMP}");
    }


    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToLevelUp)
        {
            LevelUp();
        }
        else
        {
            // Update UI
            var expUIManager = GetComponentInChildren<ExpUIManager>();
            if (expUIManager != null)
                expUIManager.UpdateUI();

            // Gửi exp mới nhất lên server
            SyncExp();
        }
    }
    private void SyncExp()
    {
        var ppm = GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            ppm.UpdateProfile(profile =>
            {
                profile.exp = currentExp;
            });
        }
    }


    private void LevelUp()
    {
        level++;
        currentExp = 0;
        expToLevelUp += 50;

        maxHP += 20;
        maxMP += 10;
        currentHP = maxHP;
        currentMP = maxMP;

        var controller = GetComponent<vThirdPersonController>();
        if (controller != null)
        {
            ForceUpdateStatsToInvector(controller, maxHP, maxMP, currentHP, currentMP, debugFields: true);
        }

        // --- CHỈ DÙNG PlayerPositionManager ĐỂ UPDATE PROFILE ---
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
                //profile.expToLevelUp = expToLevelUp;
                // KHÔNG động vào profile.currentCheckpoint!
            });
        }
        var expUIManager = GetComponentInChildren<ExpUIManager>();
        if (expUIManager != null)
            expUIManager.UpdateUI();

        else
        {
            Debug.LogError("PlayerPositionManager chưa được gắn vào player!");
        }
    }

    private void ForceUpdateStatsToInvector(vThirdPersonController controller, int newMaxHP, int newMaxMP, int newCurrentHP, int newCurrentMP, bool debugFields = false)
    {
        var type = controller.GetType();

        // Set _maxHealth
        var maxHealthField = type.GetField("_maxHealth", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (maxHealthField != null)
        {
            maxHealthField.SetValue(controller, newMaxHP);
            Debug.Log($"_maxHealth = {newMaxHP}");
        }
        else
        {
            Debug.LogError("Không tìm thấy _maxHealth trong controller");
        }

        // Set _maxStamina
        var maxStaminaField = type.GetField("_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (maxStaminaField != null)
        {
            maxStaminaField.SetValue(controller, newMaxMP);
            Debug.Log($"_maxStamina = {newMaxMP}");
        }
        else
        {
            Debug.LogError("Không tìm thấy _maxStamina trong controller");
        }

        // Set current values
        controller.ChangeHealth(newCurrentHP);
        controller.ChangeStamina(newCurrentMP);

        // Debug thêm nếu muốn
        if (debugFields)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in fields)
            {
                if (f.Name.ToLower().Contains("health"))
                    Debug.Log($"FIELD: {f.Name}, Value = {f.GetValue(controller)}");
            }
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
        if (currentHP <= 0)
        {
            currentHP = 0;
            //Die();
        }

        Debug.Log($"Người chơi bị trúng đòn! HP còn lại: {currentHP}");
    }

}
