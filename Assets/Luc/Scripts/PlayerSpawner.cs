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
        int characterId = PlayerPrefs.GetInt("CharacterId", -1);
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

            GameObject player = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Spawned character: " + characterClass);
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
        public int hp;
        public int mp;
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
