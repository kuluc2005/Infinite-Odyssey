// GoldManager.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;
    public int CurrentGold { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("GoldManager");
            go.AddComponent<GoldManager>();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(WaitForProfileAndLoadGold());
    }

    IEnumerator WaitForProfileAndLoadGold()
    {
        while (ProfileManager.CurrentProfile == null) yield return null;
        CurrentGold = ProfileManager.CurrentProfile.coins;
    }

    public void AddCoins(int amount)
    {
        CurrentGold += amount;
        if (ProfileManager.CurrentProfile != null) ProfileManager.CurrentProfile.coins = CurrentGold;
        StartCoroutine(UpdateCoinsToAPI());
    }

    public bool SpendCoins(int amount)
    {
        if (CurrentGold < amount) return false;
        CurrentGold -= amount;
        if (ProfileManager.CurrentProfile != null) ProfileManager.CurrentProfile.coins = CurrentGold;
        StartCoroutine(UpdateCoinsToAPI());
        return true;
    }

    public IEnumerator UpdateCoinsToAPI()
    {
        if (ProfileManager.CurrentProfile == null) yield break;
        string url = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(ProfileManager.CurrentProfile);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        var req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

    public void RefreshGoldFromProfile()
    {
        if (ProfileManager.CurrentProfile != null)
            CurrentGold = ProfileManager.CurrentProfile.coins;
    }
}
