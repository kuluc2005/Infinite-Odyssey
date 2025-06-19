using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ProfileManager : MonoBehaviour
{
    public static PlayerProfile CurrentProfile;

    void Start()
    {
        int characterId = PlayerPrefs.GetInt("CharacterId", -1);
        Debug.Log("CharacterId từ PlayerPrefs: " + characterId);

        if (characterId != -1)
            StartCoroutine(LoadProfile(characterId));
    }

    IEnumerator LoadProfile(int characterId)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("JSON profile nhận được: " + json);

            PlayerProfileWrapper wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(json);
            CurrentProfile = wrapper.data;

            Debug.Log($"Profile loaded: Id = {CurrentProfile.id}, Class = {CurrentProfile.characterClass}, Level = {CurrentProfile.level}");
        }
        else
        {
            Debug.LogError("Không thể load profile: " + request.error);
        }
    }

    public static IEnumerator LoadProfileStatic(int characterId)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[Static] Profile JSON: " + json);

            PlayerProfileWrapper wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(json);
            CurrentProfile = wrapper.data;

            Debug.Log($"[Static] Loaded profile: {CurrentProfile.characterClass}, Lv: {CurrentProfile.level}");
        }
        else
        {
            Debug.LogError("[Static] Không thể load profile: " + request.error);
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
    public int maxHP;
    public int maxMP;
}
