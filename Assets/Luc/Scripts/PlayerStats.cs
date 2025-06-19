using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : MonoBehaviour
{
    public int currentHP;
    public int maxHP;
    public int currentMP;
    public int maxMP;
    public int level;
    public int currentExp;
    public int expToLevelUp = 100;

    private IEnumerator Start()
    {
        // ❗️Đợi cho đến khi profile được gán
        while (ProfileManager.CurrentProfile == null)
        {
            Debug.LogWarning("⏳ Đang đợi ProfileManager.CurrentProfile...");
            yield return null;
        }

        var profile = ProfileManager.CurrentProfile;

        level = profile.level;
        maxHP = profile.maxHP;
        maxMP = profile.maxMP;
        currentHP = profile.hP;
        currentMP = profile.mP;

        Debug.Log("✅ PlayerStats khởi tạo xong từ Profile");
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
            Debug.Log("Profile cập nhật thành công!");
        }
        else
        {
            Debug.LogError("Lỗi cập nhật profile: " + request.error);
        }
    }
}
