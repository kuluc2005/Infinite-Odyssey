using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public ErrorPanelManager errorPanelManager; // Kéo vào Inspector
    public TMP_Text statusText;

    public void OnLoginButtonClick()
    {
        StartCoroutine(LoginCoroutine());
    }

    public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }

    IEnumerator LoginCoroutine()
    {
        PlayerPrefs.DeleteAll();

        string url = "http://localhost:5186/api/login";

        string json = JsonUtility.ToJson(new PlayerLogin
        {
            Email = emailInput.text,
            PasswordHash = passwordInput.text
        });

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login response: " + request.downloadHandler.text);
            LoginResponse loginResult = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (loginResult != null && loginResult.status && loginResult.data != null)
            {
                PlayerPrefs.SetInt("PlayerId", loginResult.data.id);
                PlayerPrefs.SetInt("CharacterId", -1);
                PlayerPrefs.Save();

                errorPanelManager.ShowError("Đăng nhập thành công!", Color.green);
                yield return new WaitForSeconds(1.2f);
                errorPanelManager.HideError();

                int playerId = loginResult.data.id;
                string charUrl = $"http://localhost:5186/api/character/list/{playerId}";
                UnityWebRequest charListRequest = UnityWebRequest.Get(charUrl);
                yield return charListRequest.SendWebRequest();

                if (charListRequest.result == UnityWebRequest.Result.Success)
                {
                    CharacterListWrapper wrapper = JsonUtility.FromJson<CharacterListWrapper>(charListRequest.downloadHandler.text);
                    if (wrapper.data != null && wrapper.data.Length > 0)
                    {
                        // Có nhân vật rồi, chuyển vào scene chọn lại nhân vật cũ
                        UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelectScene");
                    }
                    else
                    {
                        // Chưa có nhân vật nào, chuyển vào scene tạo nhân vật mới
                        UnityEngine.SceneManagement.SceneManager.LoadScene("CreateCharacterScene");
                    }
                }
                else
                {
                    errorPanelManager.ShowError("Không thể lấy danh sách nhân vật!\n" + charListRequest.error, Color.red);
                }
                // ----- KẾT THÚC ĐOẠN BỔ SUNG -----
            }
            else
            {
                errorPanelManager.ShowError("❌ " + (loginResult?.message ?? "Đăng nhập thất bại."), Color.red);
                Debug.LogError("Đăng nhập API trả về lỗi: " + loginResult?.message);
            }
        }
        else
        {
            errorPanelManager.ShowError("Lỗi: " + request.error);
            Debug.LogError("Lỗi kết nối: " + request.error);
        }
    }

    // ------- class cho response lấy danh sách nhân vật -------
    [System.Serializable]
    public class CharacterListWrapper
    {
        public bool status;
        public PlayerCharacter[] data;
    }
    // ---------------------------------------------------------------

    [System.Serializable]
    public class PlayerLogin
    {
        public string Email;
        public string PasswordHash;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public bool status;
        public string message;
        public PlayerData data;
    }

    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string Email;
        public string userName;
        public string CreatedAt;
    }
}
