using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class PlayerPositionManager : MonoBehaviour
{
    [HideInInspector] public int characterId = -1;

    public void UpdateProfile(System.Action<PlayerSpawner.PlayerProfile> modifyProfile)
    {
        CoroutineRunner.Run(UpdateProfileCoroutine(modifyProfile));
    }

    public IEnumerator UpdateProfileAndWait(System.Action<PlayerSpawner.PlayerProfile> modifyProfile)
    {
        yield return UpdateProfileCoroutine(modifyProfile);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            SavePlayerPosition();
    }

    IEnumerator UpdateProfileCoroutine(System.Action<PlayerSpawner.PlayerProfile> modifyProfile)
    {
        int id = characterId;

        string urlGet = $"http://localhost:5186/api/character/profile/{id}";
        UnityWebRequest req = UnityWebRequest.Get(urlGet);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            yield break;

        var wrapper = JsonUtility.FromJson<PlayerSpawner.PlayerProfileWrapper>(req.downloadHandler.text);
        if (wrapper == null || wrapper.data == null)
            yield break;

        // KHÔNG tham chiếu transform ở đây nữa
        // Chỉ sửa profile theo hàm truyền vào
        modifyProfile(wrapper.data);

        string putUrl = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(wrapper.data);
        UnityWebRequest request = new UnityWebRequest(putUrl, "PUT");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
    }

    public void SavePlayerPosition()
    {
        Vector3 pos = transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string[] skipScenes = { "LoginScene", "CharacterSelectScene", "CreateCharacterScene", "ChangePasswordScene", "RegisterScene", "ResultScene" };
        bool isGameScene = !System.Linq.Enumerable.Contains(skipScenes, sceneName);

        int curGold = (GoldManager.Instance != null)
            ? GoldManager.Instance.CurrentGold
            : (ProfileManager.CurrentProfile != null ? ProfileManager.CurrentProfile.coins : 0);

        int exp = 0, level = 0, maxHP = 0, maxMP = 0, hp = 0, mp = 0;
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            exp = stats.currentExp;
            level = stats.level;
            maxHP = stats.maxHP;
            maxMP = stats.maxMP;
            hp = stats.currentHP;
            mp = stats.currentMP;
        }
        string checkpoint = $"{sceneName}:{pos.x},{pos.y},{pos.z}";

        if (ProfileManager.CurrentProfile != null)
        {
            ProfileManager.CurrentProfile.currentCheckpoint = checkpoint;
            if (isGameScene) ProfileManager.CurrentProfile.lastScene = sceneName;
            ProfileManager.CurrentProfile.coins = curGold;
            ProfileManager.CurrentProfile.exp = exp;
            ProfileManager.CurrentProfile.level = level;
            ProfileManager.CurrentProfile.maxHP = maxHP;
            ProfileManager.CurrentProfile.maxMP = maxMP;
            ProfileManager.CurrentProfile.HP = hp;
            ProfileManager.CurrentProfile.MP = mp;
        }

        UpdateProfile(p =>
        {
            p.currentCheckpoint = checkpoint;
            if (isGameScene) p.lastScene = sceneName;

            p.coins = curGold;
            p.exp = exp;
            p.level = level;
            p.maxHP = maxHP;
            p.maxMP = maxMP;
            p.HP = hp;
            p.MP = mp;
        });
    }

    public IEnumerator SavePlayerPositionRoutine()
    {
        Vector3 pos = transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string[] skipScenes = { "LoginScene", "CharacterSelectScene", "CreateCharacterScene", "ChangePasswordScene", "RegisterScene", "ResultScene" };
        bool isGameScene = !System.Linq.Enumerable.Contains(skipScenes, sceneName);

        int curGold = (GoldManager.Instance != null)
            ? GoldManager.Instance.CurrentGold
            : (ProfileManager.CurrentProfile != null ? ProfileManager.CurrentProfile.coins : 0);

        int exp = 0, level = 0, maxHP = 0, maxMP = 0, hp = 0, mp = 0;
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            exp = stats.currentExp;
            level = stats.level;
            maxHP = stats.maxHP;
            maxMP = stats.maxMP;
            hp = stats.currentHP;
            mp = stats.currentMP;
        }
        string checkpoint = $"{sceneName}:{pos.x},{pos.y},{pos.z}";

        if (ProfileManager.CurrentProfile != null)
        {
            ProfileManager.CurrentProfile.currentCheckpoint = checkpoint;
            if (isGameScene) ProfileManager.CurrentProfile.lastScene = sceneName;
            ProfileManager.CurrentProfile.coins = curGold;
            ProfileManager.CurrentProfile.exp = exp;
            ProfileManager.CurrentProfile.level = level;
            ProfileManager.CurrentProfile.maxHP = maxHP;
            ProfileManager.CurrentProfile.maxMP = maxMP;
            ProfileManager.CurrentProfile.HP = hp;
            ProfileManager.CurrentProfile.MP = mp;
        }

        yield return UpdateProfileAndWait(p =>
        {
            p.currentCheckpoint = checkpoint;
            if (isGameScene) p.lastScene = sceneName;

            p.coins = curGold;
            p.exp = exp;
            p.level = level;
            p.maxHP = maxHP;
            p.maxMP = maxMP;
            p.HP = hp;
            p.MP = mp;
        });
    }

    void OnApplicationQuit()
    {
        SavePlayerPosition();
    }
}
