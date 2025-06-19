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

    void Start()
    {
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
            Debug.Log("🎮 Nhân vật được tạo, ID = " + wrapper.data.id);

            PlayerPrefs.SetInt("CharacterId", wrapper.data.id);
            PlayerPrefs.Save();

            yield return StartCoroutine(ProfileManager.LoadProfileStatic(wrapper.data.id));

            if (ProfileManager.CurrentProfile != null)
            {
                Debug.Log("✅ Profile đã load xong, chuyển scene.");
                SceneManager.LoadScene("SceneMain");
            }
            else
            {
                Debug.LogError("❌ Không thể chuyển scene vì Profile chưa load xong.");
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
