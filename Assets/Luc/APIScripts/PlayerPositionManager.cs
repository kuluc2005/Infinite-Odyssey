using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class PlayerPositionManager : MonoBehaviour
{
    [HideInInspector] public int characterId = -1;

    // --- Hàm đa năng: Cập nhật bất kỳ trường nào ---
    public void UpdateProfile(System.Action<PlayerSpawner.PlayerProfile> modifyProfile)
    {
        StartCoroutine(UpdateProfileCoroutine(modifyProfile));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            SavePlayerPosition();
    }


    IEnumerator UpdateProfileCoroutine(System.Action<PlayerSpawner.PlayerProfile> modifyProfile)
    {
        int characterId = this.characterId;
        string urlGet = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest req = UnityWebRequest.Get(urlGet);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Không lấy được profile mới nhất: " + req.error);
            yield break;
        }

        PlayerSpawner.PlayerProfileWrapper wrapper = JsonUtility.FromJson<PlayerSpawner.PlayerProfileWrapper>(req.downloadHandler.text);
        if (wrapper == null || wrapper.data == null)
        {
            Debug.LogError("Không parse được profile");
            yield break;
        }

        // ================================
        // GIỮ LẠI GIÁ TRỊ currentCheckpoint ĐÃ TỪNG LƯU
        // ================================
        if (string.IsNullOrEmpty(wrapper.data.currentCheckpoint) || wrapper.data.currentCheckpoint == "Start")
        {
            // Nếu chưa từng lưu vị trí, lấy vị trí hiện tại của player
            Vector3 pos = transform.position;
            wrapper.data.currentCheckpoint = $"{pos.x},{pos.y},{pos.z}";
        }
        // Nếu đã từng lưu vị trí rồi, GIỮ NGUYÊN, KHÔNG ghi đè = "Start"

        // Sửa profile bằng hàm truyền vào
        modifyProfile(wrapper.data);

        // PUT lại profile đã chỉnh sửa
        string url = "http://localhost:5186/api/character/profile/update";
        string json = JsonUtility.ToJson(wrapper.data);
        Debug.Log("PUT JSON gửi lên API: " + json);
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        Debug.Log(request.result == UnityWebRequest.Result.Success ? "Update profile thành công!" : "Update profile thất bại: " + request.error);
    }


    // --- Gọi hàm này khi cần lưu vị trí ---
    public void SavePlayerPosition()
    {
        Vector3 pos = transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string[] skipScenes = { "LoginScene", "CharacterSelectScene", "CreateCharacterScene", "ChangePasswordScene", "RegisterScene", "ResultScene" };
        bool isGameScene = !skipScenes.Contains(sceneName);


        UpdateProfile(profile =>
        {
            profile.currentCheckpoint = $"{sceneName}:{pos.x},{pos.y},{pos.z}";
            if (isGameScene)
                profile.lastScene = sceneName;

            if (GoldManager.Instance != null)
            {
                profile.coins = GoldManager.Instance.CurrentGold;
                Debug.Log($"[SavePlayerPosition] Lưu {profile.coins}");
            }
            var stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                profile.exp = stats.currentExp;
                Debug.Log($"[SavePlayerPosition] Lưu EXP: {stats.currentExp}");
            }
        });
    }




    // --- Có thể gọi hàm này khi tăng level ---
    public void LevelUp()
    {
        UpdateProfile(profile =>
        {
            profile.level += 1;
            // Có thể chỉnh exp/coin ở đây nếu muốn
            // KHÔNG động vào profile.currentCheckpoint!
        });
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit được gọi!");
        SavePlayerPosition();
    }
}
