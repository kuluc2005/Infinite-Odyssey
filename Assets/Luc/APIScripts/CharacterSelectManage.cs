using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CharacterSelectManager : MonoBehaviour
{
    public Button maleButton;
    public Button femaleButton;

    public GameObject existingCharactersPanel;
    public GameObject createNewCharacterPanel;
    public Transform characterListContainer;
    public GameObject characterButtonPrefab;
    public Button createNewCharacterButton;



    void Start()
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        if (playerId != -1)
            StartCoroutine(LoadCharacters(playerId));

        createNewCharacterButton.onClick.AddListener(() =>
        {
            existingCharactersPanel.SetActive(false);
            createNewCharacterPanel.SetActive(true);
        });

        maleButton.onClick.AddListener(() => SelectCharacter("Male"));
        femaleButton.onClick.AddListener(() => SelectCharacter("Female"));
    }


    void SelectCharacter(string characterClass)
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        Debug.Log("PlayerId đang sử dụng: " + playerId);
        if (playerId != -1)
            StartCoroutine(CreateCharacter(playerId, characterClass));
    }

    IEnumerator LoadCharacters(int playerId)
    {
        string url = $"http://localhost:5186/api/character/list/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            PlayerCharacterListWrapper wrapper = JsonUtility.FromJson<PlayerCharacterListWrapper>(json);
            Debug.Log("📥 JSON từ API: " + json);
            Debug.Log("📦 Số lượng nhân vật: " + wrapper.data?.Length);

            foreach (var character in wrapper.data)
            {
                Debug.Log("➡️ Nhân vật: " + character.characterClass + " - ID: " + character.id);
            }


            if (wrapper.data != null && wrapper.data.Length > 0)
            {
                existingCharactersPanel.SetActive(true);
                createNewCharacterPanel.SetActive(false);

                foreach (Transform child in characterListContainer)
                    Destroy(child.gameObject);

                foreach (var character in wrapper.data)
                {
                    GameObject btn = Instantiate(characterButtonPrefab, characterListContainer);
                    btn.GetComponentInChildren<TMP_Text>().text = character.characterClass + " #" + character.id;
                    var textComp = btn.GetComponentInChildren<TMP_Text>();
                    if (textComp != null)
                    {
                        textComp.text = character.characterClass + " #" + character.id;
                        Debug.Log("✅ Gán tên cho nhân vật: " + textComp.text);
                    }
                    else
                    {
                        Debug.LogError("❌ Không tìm thấy TMP_Text trong prefab!");
                    }

                    btn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        PlayerPrefs.SetInt("CharacterId", character.id);
                        StartCoroutine(ProfileManager.LoadProfileStatic(character.id));
                        StartCoroutine(WaitAndLoadScene());
                    });
                }
            }
            else
            {
                existingCharactersPanel.SetActive(false);
                createNewCharacterPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Lỗi khi load danh sách nhân vật: " + request.error);
        }
    }

    IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(1f);
        if (ProfileManager.CurrentProfile != null)
            SceneManager.LoadScene("SceneMain");
    }


    IEnumerator CreateCharacter(int playerId, string characterClass)
    {
        string url = "http://localhost:5186/api/character/create";

        CreateCharacterRequest req = new CreateCharacterRequest
        {
            playerId = playerId,
            characterClass = characterClass
        };

        string json = JsonUtility.ToJson(req);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            PlayerCharacterWrapper wrapper = JsonUtility.FromJson<PlayerCharacterWrapper>(response);

            PlayerPrefs.SetInt("CharacterId", wrapper.data.id);
            PlayerPrefs.Save();

            yield return StartCoroutine(ProfileManager.LoadProfileStatic(wrapper.data.id));

            if (ProfileManager.CurrentProfile != null)
            {
                Debug.Log("✅ Tạo nhân vật mới xong. Reload danh sách.");
                int pid = PlayerPrefs.GetInt("PlayerId");
                StartCoroutine(LoadCharacters(pid)); // ← reload lại danh sách
            }
        }
        else
        {
            Debug.LogError("Lỗi tạo nhân vật: " + request.error);
        }
    }
}

    [System.Serializable]
public class CreateCharacterRequest
{
    public int playerId;
    public string characterClass;
}

[System.Serializable]
public class PlayerCharacterWrapper
{
    public bool status;
    public string message;
    public PlayerCharacter data;
}

[System.Serializable]
public class PlayerCharacterListWrapper
{
    public bool status;
    public PlayerCharacter[] data;
}

