using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
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
            Debug.LogWarning("⏳ Đang đợi ProfileManager.CurrentProfile...");
            yield return null;
        }

        var profile = ProfileManager.CurrentProfile;

        level = profile.level;
        maxHP = profile.maxHP;
        maxMP = profile.maxMP;

        currentHP = profile.hP <= 0 ? maxHP : profile.hP;
        currentMP = profile.mP <= 0 ? maxMP : profile.mP;

        profile.hP = currentHP;
        profile.mP = currentMP;

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

        Debug.Log($"✅ HP/MP đã gán: {currentHP}/{maxHP} - {currentMP}/{maxMP}");
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToLevelUp)
        {
            LevelUp();
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

        var profile = ProfileManager.CurrentProfile;
        profile.level = level;
        profile.maxHP = maxHP;
        profile.maxMP = maxMP;
        profile.hP = currentHP;
        profile.mP = currentMP;

        StartCoroutine(UpdateProfile(profile));

        var controller = GetComponent<vThirdPersonController>();
        if (controller != null)
        {
            ForceUpdateStatsToInvector(controller, maxHP, maxMP, currentHP, currentMP, debugFields: true);
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
            Debug.Log($"✅ [Invector] _maxHealth = {newMaxHP}");
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy _maxHealth trong controller");
        }

        // Set _maxStamina
        var maxStaminaField = type.GetField("_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (maxStaminaField != null)
        {
            maxStaminaField.SetValue(controller, newMaxMP);
            Debug.Log($"✅ [Invector] _maxStamina = {newMaxMP}");
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy _maxStamina trong controller");
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
                    Debug.Log($"👉 FIELD: {f.Name}, Value = {f.GetValue(controller)}");
            }
        }
    }

    IEnumerator UpdateProfile(PlayerProfile profile)
    {
        string url = "http://localhost:5186/api/update-profile";
        string json = JsonUtility.ToJson(profile);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Profile cập nhật thành công!");
        }
        else
        {
            Debug.LogError("❌ Lỗi cập nhật profile: " + request.error);
        }
    }
}
