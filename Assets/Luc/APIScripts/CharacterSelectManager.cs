using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CharacterSelectManager : MonoBehaviour
{
    public Transform characterListContent;         // Content của ScrollView
    public GameObject characterCardPrefab;         // Prefab cho card nhân vật
    public Button createNewCharacterButton;        // Nút tạo mới

    void Start()
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        if (playerId != -1)
            StartCoroutine(LoadCharacters(playerId));
        else
            SceneManager.LoadScene("LoginScene");

        createNewCharacterButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CreateCharacterScene");
        });
    }

    IEnumerator LoadCharacters(int playerId)
    {
        string url = $"http://localhost:5186/api/character/list/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("JSON danh sách nhân vật nhận về: " + request.downloadHandler.text);

            CharacterListWrapper wrapper = JsonUtility.FromJson<CharacterListWrapper>(request.downloadHandler.text);
            foreach (Transform child in characterListContent)
                Destroy(child.gameObject);

            if (wrapper.data != null && wrapper.data.Length > 0)
            {
                Debug.Log($"Có tổng cộng {wrapper.data.Length} nhân vật.");
                int count = 0;
                foreach (var character in wrapper.data)
                {
                    GameObject card = Instantiate(characterCardPrefab, characterListContent);
                    card.GetComponent<CharacterCardUI>().Setup(character, OnCharacterSelected);
                    Debug.Log($"Card {count}: ID={character.id}, Class={character.characterClass}");
                    count++;
                }
                Debug.Log($"Đã tạo {count} card nhân vật.");
            }
            else
            {
                SceneManager.LoadScene("CreateCharacterScene");
            }
        }
        else
        {
            Debug.LogError("Không tải được danh sách nhân vật: " + request.error);
        }
    }

    public void OnCharacterSelected(PlayerCharacter character)
    {
        var oldPlayer = FindFirstObjectByType<PlayerPositionManager>();
        if (oldPlayer != null)
        {
            Debug.Log("Xoá player cũ trước khi load nhân vật mới");
            Destroy(oldPlayer.gameObject);
        }

        PlayerPrefs.SetInt("CharacterId", character.id);
        PlayerPrefs.SetString("CharacterName", character.name);
        StartCoroutine(GoToGameScene(character.id));
    }

    IEnumerator GoToGameScene(int characterId)
    {
        yield return StartCoroutine(ProfileManager.LoadProfileStatic(characterId));

        if (ProfileManager.CurrentProfile != null)
        {
            if (GoldManager.Instance != null)
                GoldManager.Instance.RefreshGoldFromProfile();

            string[] skip = {
            "LoginScene","CharacterSelectScene","CreateCharacterScene",
            "ChangePasswordScene","RegisterScene","ResultScene"
        };

            bool IsPlayableScene(string s)
            {
                if (string.IsNullOrEmpty(s)) return false;
                foreach (var k in skip) if (s == k) return false;
                return true;
            }

            string targetScene = null;

            var cp = ProfileManager.CurrentProfile.currentCheckpoint;
            if (!string.IsNullOrEmpty(cp))
            {
                int idx = cp.IndexOf(':');
                if (idx > 0)
                {
                    string sceneFromCP = cp.Substring(0, idx);
                    if (IsPlayableScene(sceneFromCP))
                        targetScene = sceneFromCP;
                }
            }

            if (targetScene == null)
            {
                var last = ProfileManager.CurrentProfile.lastScene;
                if (IsPlayableScene(last)) targetScene = last;
            }

            if (string.IsNullOrEmpty(targetScene))
                targetScene = "Level 0";

            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("Không thể load profile cho nhân vật này!");
        }
    }

    [System.Serializable]
    public class CharacterListWrapper
    {
        public bool status;
        public PlayerCharacter[] data;
    }
}
