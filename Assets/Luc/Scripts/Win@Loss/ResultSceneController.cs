using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ResultSceneController : MonoBehaviour
{
    [Header("Khai báo UI theo giới tính & kết quả")]
    public GameObject winMaleUI;
    public GameObject winFemaleUI;
    public GameObject loseMaleUI;
    public GameObject loseFemaleUI;

    public void OnRetryLevel() => GameFlowManager.RetryLastLevel();

    void Awake()
    {
        // Tắt hết UI trước, chờ xác định kết quả + giới tính
        if (winMaleUI) winMaleUI.SetActive(false);
        if (winFemaleUI) winFemaleUI.SetActive(false);
        if (loseMaleUI) loseMaleUI.SetActive(false);
        if (loseFemaleUI) loseFemaleUI.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator Start()
    {
        int guard = 0;
        while (ProfileManager.CurrentProfile == null && guard < 300)
        {
            guard++;
            yield return null;
        }

        string cls = ProfileManager.CurrentProfile != null
            ? ProfileManager.CurrentProfile.characterClass
            : PlayerPrefs.GetString("SelectedClass", "Male");

        string result = PlayerPrefs.GetString("LastResult", "Win");
        bool isFemale = (!string.IsNullOrEmpty(cls) && cls == "Female");

        // Bật đúng UI theo kết quả & giới tính
        if (result == "Win")
        {
            if (isFemale) { if (winFemaleUI) winFemaleUI.SetActive(true); }
            else { if (winMaleUI) winMaleUI.SetActive(true); }
        }
        else
        {
            if (isFemale) { if (loseFemaleUI) loseFemaleUI.SetActive(true); }
            else { if (loseMaleUI) loseMaleUI.SetActive(true); }
        }
    }

    // ======== Buttons / Actions ========

    public void OnBackToSelect()
    {
        SmoothLoad("CharacterSelectScene");
    }

    public void OnNextLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", "Level 0");
        string next = GetNextLevelName(last);
        if (!string.IsNullOrEmpty(next)) SmoothLoad(next);
        else SmoothLoad("CharacterSelectScene"); // Hết level thì quay về chọn nhân vật
    }

    public void OnResetToLevel0()
    {
        StartCoroutine(ResetToNewGameRoutine());
    }

    public void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ======== Logic Reset Về Trạng Thái Ban Đầu ========

    private IEnumerator ResetToNewGameRoutine()
    {
        int characterId = PlayerPrefs.GetInt("CharacterId", -1);

        if (characterId <= 0)
        {
            SmoothLoad("Level 0");
            yield break;
        }

        string urlGet = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest get = UnityWebRequest.Get(urlGet);
        yield return get.SendWebRequest();

        if (get.result != UnityWebRequest.Result.Success)
        {
            SmoothLoad("Level 0");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<PlayerSpawner.PlayerProfileWrapper>(get.downloadHandler.text);
        var profile = wrapper.data;

        profile.level = 1;
        profile.exp = 0;
        profile.maxHP = 100;
        profile.maxMP = 200;
        profile.HP = profile.maxHP;
        profile.MP = profile.maxMP;
        profile.coins = 100;

        profile.lastScene = "Level 0";
        profile.currentCheckpoint = ""; 

        // PUT cập nhật profile
        string putUrl = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(profile);
        UnityWebRequest put = new UnityWebRequest(putUrl, "PUT");
        put.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        put.downloadHandler = new DownloadHandlerBuffer();
        put.SetRequestHeader("Content-Type", "application/json");
        yield return put.SendWebRequest();

        if (ProfileManager.CurrentProfile != null)
        {
            var local = ProfileManager.CurrentProfile;
            local.level = profile.level;
            local.exp = profile.exp;
            local.maxHP = profile.maxHP;
            local.maxMP = profile.maxMP;
            local.HP = profile.HP;
            local.MP = profile.MP;
            local.coins = profile.coins;
            local.lastScene = profile.lastScene;
            local.currentCheckpoint = profile.currentCheckpoint;
        }

        GoldManager.Instance?.RefreshGoldFromProfile();

        SmoothLoad("Level 0");
    }

    // ======== Helpers ========

    private void SmoothLoad(string sceneName)
    {
        SceneTransition.Load(sceneName);
    }

    private string GetNextLevelName(string current)
    {
        switch (current)
        {
            case "Level 0": return "Level 1";
            case "Level 1": return "Level 2";
            case "Level 2": return "Level 3";
            default: return null;
        }
    }

    public static void RetryLastLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", "Level 0");
        SceneTransition.Load(last);
    }
}
