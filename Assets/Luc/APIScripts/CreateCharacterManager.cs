using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateCharacterManager : MonoBehaviour
{
    public Button maleButton;
    public Button femaleButton;
    public TMP_InputField nameInput;
    public GameObject nameInputPanel;         // Panel chứa input & nút, ẩn đi lúc đầu
    public Button createCharacterButton;      // Nút xác nhận tạo nhân vật
    public ErrorPanelManager errorPanelManager;
    public Button closeNameInputButton;  //Nút tắt panel đặt tên

    public Image avatarImage;                   // UI ảnh đại diện nhân vật
    public Sprite avatarMaleSprite;             // Ảnh nam
    public Sprite avatarFemaleSprite;           // Ảnh nữ



    private string selectedClass = null;

    void Start()
    {
        nameInputPanel.SetActive(false);

        maleButton.onClick.AddListener(() => ShowNameInput("Male"));
        femaleButton.onClick.AddListener(() => ShowNameInput("Female"));
        createCharacterButton.onClick.AddListener(OnCreateCharacter);

        closeNameInputButton.onClick.AddListener(() => nameInputPanel.SetActive(false));
    }


    void ShowNameInput(string characterClass)
    {
        selectedClass = characterClass;
        nameInput.text = "";
        nameInputPanel.SetActive(true);

        // Gán avatar tương ứng từ Resources nếu có
        if (avatarImage != null)
        {
            // Ưu tiên load từ Resources/ImageL/[characterClass]
            Sprite loadedAvatar = Resources.Load<Sprite>("ImageL/" + characterClass);

            if (loadedAvatar != null)
            {
                avatarImage.sprite = loadedAvatar;
            }
            else
            {
                avatarImage.sprite = (characterClass == "Male") ? avatarMaleSprite : avatarFemaleSprite;
            }
        }
    }



    void OnCreateCharacter()
    {
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        string name = nameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            errorPanelManager.ShowError("Hãy nhập tên nhân vật!", Color.red);
            return;
        }
        if (playerId != -1 && !string.IsNullOrEmpty(selectedClass))
            StartCoroutine(CreateCharacter(playerId, selectedClass, name));
    }

    IEnumerator CreateCharacter(int playerId, string characterClass, string name)
    {
        string url = "http://localhost:5186/api/character/create";
        CreateCharacterRequest req = new CreateCharacterRequest
        {
            playerId = playerId,
            characterClass = characterClass,
            name = name
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
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelectScene");
        }
        else
        {
            errorPanelManager.ShowError("Lỗi tạo nhân vật: " + request.error, Color.red);
        }
    }

    [System.Serializable]
    public class CreateCharacterRequest
    {
        public int playerId;
        public string characterClass;
        public string name;
    }
}
