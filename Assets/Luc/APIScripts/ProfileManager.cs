using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ProfileManager : MonoBehaviour
{
    public static PlayerProfile CurrentProfile;

    void Start()
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        Debug.Log("📦 PlayerId từ PlayerPrefs: " + playerId);

        if (playerId != -1)
            StartCoroutine(LoadProfile(playerId));
    }

    IEnumerator LoadProfile(int playerId)
    {
        string url = $"http://localhost:5186/api/playerprofile/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("📥 JSON profile nhận được: " + json);

            PlayerProfileWrapper wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(json);
            CurrentProfile = wrapper.data;

            Debug.Log($"✅ Profile loaded: Id = {CurrentProfile.id}, PlayerId = {CurrentProfile.playerId}, Class = {CurrentProfile.characterClass}");
        }
        else
        {
            Debug.LogError("❌ Không thể load profile: " + request.error);
        }
    }
}

[System.Serializable]
public class PlayerProfileWrapper
{
    public bool status;
    public string message;
    public PlayerProfile data;
}


[System.Serializable]
public class PlayerProfile
{
    public int id;
    public int playerId;
    public string characterClass;
    public int level;
    public int exp;
    public int coins;
    public int hP;
    public int mP;
    public string currentCheckpoint;
    public string inventoryJSON;
    public string skillTreeJSON;
    public string lastScene;
}
