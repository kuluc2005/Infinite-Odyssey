using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public Transform spawnPoint;

    void Start()
    {
        Debug.Log("PlayerSpawner Start chạy!");

        int characterId = PlayerPrefs.GetInt("CharacterId", -1);
        Debug.Log("characterId lấy từ PlayerPrefs: " + characterId);

        if (characterId <= 0)
        {
            Debug.LogError("Không có CharacterId được lưu!");
            return;
        }

        StartCoroutine(LoadCharacterAndSpawn(characterId));
    }

    IEnumerator LoadCharacterAndSpawn(int characterId)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Lỗi khi gọi API: " + request.error);
            yield break;
        }

        string response = request.downloadHandler.text;
        Debug.Log("Profile response: " + response);

        PlayerProfileWrapper wrapper = JsonUtility.FromJson<PlayerProfileWrapper>(response);
        if (wrapper != null && wrapper.data != null)
        {
            string characterClass = wrapper.data.characterClass;
            GameObject prefabToSpawn = characterClass == "Female" ? femalePrefab : malePrefab;

            // ==== Xác định vị trí spawn ====
            Vector3 spawnPos = spawnPoint.position;
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log("Scene hiện tại: " + currentScene);
            Debug.Log("Scene lưu trong lastScene: " + wrapper.data.lastScene);

            if (!string.IsNullOrEmpty(wrapper.data.lastScene) && wrapper.data.lastScene == currentScene)
            {
                if (!string.IsNullOrEmpty(wrapper.data.currentCheckpoint) && wrapper.data.currentCheckpoint.Contains(","))
                {
                    string[] values = wrapper.data.currentCheckpoint.Split(',');
                    if (values.Length == 3)
                    {
                        float x = float.Parse(values[0]);
                        float y = float.Parse(values[1]);
                        float z = float.Parse(values[2]);
                        spawnPos = new Vector3(x, y, z);
                        Debug.Log($"✅ Đã lấy lại vị trí lưu cho scene hiện tại: {spawnPos}");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ currentCheckpoint sai định dạng: " + wrapper.data.currentCheckpoint);
                    }
                }
            }
            else
            {
                Debug.Log("⛔ lastScene không trùng scene hiện tại. Sử dụng spawnPoint mặc định.");
            }

            // ==== Spawn nhân vật ====
            GameObject player = Instantiate(prefabToSpawn, spawnPos, spawnPoint.rotation);
            PlayerPositionManager ppm = player.GetComponent<PlayerPositionManager>();
            if (ppm != null)
            {
                ppm.characterId = wrapper.data.characterId;
            }
        }
        else
        {
            Debug.LogError("❌ Không thể đọc dữ liệu nhân vật từ phản hồi.");
        }
    }

    [System.Serializable]
    public class PlayerProfileWrapper
    {
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
        public int HP;
        public int MP;
        public string currentCheckpoint;
        public string inventoryJSON;
        public string skillTreeJSON;
        public string lastScene;
        public string lastLogin;
        public int maxHP;
        public int maxMP;
        public int characterId;
    }
}
