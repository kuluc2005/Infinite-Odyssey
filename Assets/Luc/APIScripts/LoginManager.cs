using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public ErrorPanelManager errorPanelManager; // Kéo vào Inspector
    public TMP_Text statusText;

    public void OnLoginButtonClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            errorPanelManager.ShowError("Email không được để trống", Color.red);
            return;
        }

        if (!IsValidEmail(email))
        {
            errorPanelManager.ShowError("Email không hợp lệ", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            errorPanelManager.ShowError("Mật khẩu không được để trống", Color.red);
            return;
        }

        StartCoroutine(LoginCoroutine());
    }

    public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }

    public void GoToChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChangePasswordScene");
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

        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login response: " + responseText);
            LoginResponse loginResult = JsonUtility.FromJson<LoginResponse>(responseText);

            if (loginResult != null && loginResult.status && loginResult.data != null)
            {
                int loggedInPlayerId = loginResult.data.id; // đổi tên để không trùng
                PlayerPrefs.SetInt("PlayerId", loggedInPlayerId);
                PlayerPrefs.SetInt("CharacterId", -1);
                PlayerPrefs.Save();

                errorPanelManager.ShowError("Đăng nhập thành công!", Color.green);
                yield return new WaitForSeconds(1.2f);
                errorPanelManager.HideError();

                string charUrl = $"http://localhost:5186/api/character/list/{loggedInPlayerId}";
                UnityWebRequest charListRequest = UnityWebRequest.Get(charUrl);
                yield return charListRequest.SendWebRequest();

                if (charListRequest.result == UnityWebRequest.Result.Success)
                {
                    CharacterListWrapper wrapper = JsonUtility.FromJson<CharacterListWrapper>(charListRequest.downloadHandler.text);

                    bool hasCharacters = wrapper.data != null && wrapper.data.Length > 0;
                    string introKey = $"IntroSeen_{loggedInPlayerId}";
                    bool seenIntro = PlayerPrefs.GetInt(introKey, 0) == 1;

                    if (!seenIntro && !hasCharacters)
                    {
                        PlayerPrefs.SetInt("PlayerId", loggedInPlayerId);
                        PlayerPrefs.Save();
                        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
                    }
                    else
                    {
                        if (hasCharacters)
                            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelectScene");
                        else
                            UnityEngine.SceneManagement.SceneManager.LoadScene("CreateCharacterScene");
                    }
                }
                else
                {
                    errorPanelManager.ShowError("Không thể lấy danh sách nhân vật!\n" + charListRequest.error, Color.red);
                }
            }
            else
            {
                errorPanelManager.ShowError("" + (loginResult?.message ?? "Đăng nhập thất bại."), Color.red);
                Debug.LogError("Đăng nhập API trả về lỗi: " + loginResult?.message);
            }
        }
        else
        {
            if (responseText.StartsWith("\"") && responseText.EndsWith("\""))
                responseText = responseText.Trim('"');

            if (string.IsNullOrEmpty(responseText))
                responseText = $"Lỗi máy chủ ({request.responseCode})";

            errorPanelManager.ShowError("" + responseText, Color.red);
            Debug.LogError("Lỗi kết nối: " + responseText);
        }
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    [System.Serializable]
    public class CharacterListWrapper
    {
        public bool status;
        public PlayerCharacter[] data;
    }

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
