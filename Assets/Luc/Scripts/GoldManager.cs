using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int CurrentGold { get; private set; }

    void Awake()
    {
        //Đảm bảo chỉ có 1 GoldManager tồn tại
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi đổi scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (ProfileManager.CurrentProfile != null)
        {
            CurrentGold = ProfileManager.CurrentProfile.coins;
            Debug.Log($"GoldManager Loaded Gold: {CurrentGold}");
        }
        else
        {
            Debug.LogWarning("ProfileManager chưa có profile khi load GoldManager!");
        }
    }

    public void AddCoins(int amount)
    {
        CurrentGold += amount;
        if (ProfileManager.CurrentProfile != null)
        {
            ProfileManager.CurrentProfile.coins = CurrentGold;
        }
        StartCoroutine(UpdateCoinsToAPI());
    }

    //Trừ vàng (ví dụ mua đồ)
    public bool SpendCoins(int amount)
    {
        if (CurrentGold < amount)
        {
            Debug.Log("❌ Không đủ vàng!");
            return false;
        }

        CurrentGold -= amount;
        if (ProfileManager.CurrentProfile != null)
        {
            ProfileManager.CurrentProfile.coins = CurrentGold;
        }
        StartCoroutine(UpdateCoinsToAPI());
        return true;
    }

    //Hàm gọi API PUT để cập nhật vàng
    public IEnumerator UpdateCoinsToAPI()
    {
        if (ProfileManager.CurrentProfile == null)
        {
            Debug.LogError("Không có profile để update vàng!");
            yield break;
        }

        string url = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(ProfileManager.CurrentProfile);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"API: Gold updated to {CurrentGold}");
        }
        else
        {
            Debug.LogError("API lỗi khi update vàng: " + request.error);
        }
    }
}
