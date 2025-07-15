using UnityEngine;
using UnityEngine.Networking;
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

            // ==== Lấy vị trí lưu ====
            Vector3 spawnPos = spawnPoint.position;

            if (!string.IsNullOrEmpty(wrapper.data.currentCheckpoint) && wrapper.data.currentCheckpoint.Contains(":"))
            {
                string[] parts = wrapper.data.currentCheckpoint.Split(':');
                if (parts.Length == 2)
                {
                    string savedScene = parts[0];
                    string coords = parts[1];

                    // Kiểm tra scene
                    string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    if (savedScene == currentScene && coords.Contains(","))
                    {
                        string[] values = coords.Split(',');
                        if (values.Length == 3)
                        {
                            float x = float.Parse(values[0]);
                            float y = float.Parse(values[1]);
                            float z = float.Parse(values[2]);
                            spawnPos = new Vector3(x, y, z);
                            Debug.Log($"Đã lấy lại vị trí lưu: {spawnPos}");
                        }
                    }
                    else
                    {
                        Debug.Log("Vị trí lưu thuộc scene khác, sẽ spawn tại spawnPoint!");
                    }
                }
            }
            else
            {
                Debug.Log("Không có vị trí lưu hợp lệ, sẽ spawn tại spawnPoint");
            }

            // ==== Spawn tại vị trí đã lưu (hoặc spawnPoint nếu chưa có dữ liệu) ====
            GameObject player = Instantiate(prefabToSpawn, spawnPos, spawnPoint.rotation);
            PlayerPositionManager ppm = player.GetComponent<PlayerPositionManager>();
            if (ppm != null)
            {
                ppm.characterId = wrapper.data.characterId; 
            }

            InventorySyncManager ism = player.GetComponent<InventorySyncManager>();
            if (ism != null)
            {
                ism.characterId = wrapper.data.characterId;
            }
        }
        else
        {
            Debug.LogError("Không thể đọc dữ liệu nhân vật từ phản hồi.");
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
