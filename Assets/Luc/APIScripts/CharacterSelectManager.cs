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
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");

        createNewCharacterButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CreateCharacterScene");
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
                UnityEngine.SceneManagement.SceneManager.LoadScene("CreateCharacterScene");
            }
        }
        else
        {
            Debug.LogError("Không tải được danh sách nhân vật: " + request.error);
        }
}

public void OnCharacterSelected(PlayerCharacter character)
    {
        PlayerPrefs.SetInt("CharacterId", character.id);
        StartCoroutine(GoToGameScene(character.id));
    }

    IEnumerator GoToGameScene(int characterId)
    {
        // Đợi load profile xong
        yield return StartCoroutine(ProfileManager.LoadProfileStatic(characterId));
        if (ProfileManager.CurrentProfile != null)
        {
            // Lấy scene cuối cùng của nhân vật này
            string lastScene = ProfileManager.CurrentProfile.lastScene;

            if (!string.IsNullOrEmpty(lastScene)
                && lastScene != "LoginScene"
                && lastScene != "CharacterSelectScene"
                && lastScene != "CreateCharacterScene"
                && lastScene != "ChangePasswordScene"
                && lastScene != "RegisterScene")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(lastScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SceneMain"); 
            }
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
