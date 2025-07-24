using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;
    public ErrorPanelManager errorPanelManager;
    public TMP_Text statusText;

    public void OnRegisterButtonClick()
    {
        //Kiểm tra lỗi người dùng nhập trước khi gọi API
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        string username = usernameInput.text.Trim();

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

        if (password.Length < 6)
        {
            errorPanelManager.ShowError("Mật khẩu phải có ít nhất 6 ký tự", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(username))
        {
            errorPanelManager.ShowError("Tên người dùng không được để trống", Color.red);
            return;
        }

        if (username.Contains(" "))
        {
            errorPanelManager.ShowError("Tên người dùng không được chứa khoảng trắng", Color.red);
            return;
        }

        StartCoroutine(RegisterCoroutine());
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    public void GoToChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChangePasswordScene");
    }

    IEnumerator RegisterCoroutine()
    {
        string url = "http://172.16.80.23:5186/api/register";

        PlayerRegister registerData = new PlayerRegister
        {
            Email = emailInput.text,
            PasswordHash = passwordInput.text,
            UserName = usernameInput.text
        };

        string json = JsonUtility.ToJson(registerData);
        Debug.Log("JSON gửi đi từ Unity: " + json);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Phản hồi: " + responseText);
            RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(responseText);

            if (response != null && response.status)
            {
                errorPanelManager.ShowError("Đăng ký thành công!", Color.green);
                yield return new WaitForSeconds(1.5f);
                errorPanelManager.HideError();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                errorPanelManager.ShowError("" + (response?.message ?? "Đăng ký thất bại."), Color.red);
            }
        }
        else
        {
            if (responseText.StartsWith("\"") && responseText.EndsWith("\""))
                responseText = responseText.Trim('"');

            if (string.IsNullOrEmpty(responseText))
                responseText = $"Lỗi máy chủ ({request.responseCode})";

            errorPanelManager.ShowError("" + responseText, Color.red);
            Debug.LogError("Đăng ký lỗi: " + responseText);
        }
    }


    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    [System.Serializable]
    public class PlayerRegister
    {
        public string Email;
        public string PasswordHash;
        public string UserName;
    }

    [System.Serializable]
    public class RegisterResponse
    {
        public bool status;
        public string message;
    }
}
