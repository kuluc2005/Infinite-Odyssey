using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class ChangePasswordManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;
    public ErrorPanelManager errorPanelManager;
    public TMP_Text statusText;

    public void OnChangePasswordButtonClick()
    {
        string email = emailInput.text.Trim();
        string oldPass = oldPasswordInput.text.Trim();
        string newPass = newPasswordInput.text.Trim();

        // Kiểm tra lỗi người dùng nhập
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

        if (string.IsNullOrEmpty(oldPass))
        {
            errorPanelManager.ShowError("Mật khẩu cũ không được để trống", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(newPass))
        {
            errorPanelManager.ShowError("Mật khẩu mới không được để trống", Color.red);
            return;
        }

        if (newPass.Length < 6)
        {
            errorPanelManager.ShowError("Mật khẩu mới phải có ít nhất 6 ký tự", Color.red);
            return;
        }

        if (oldPass == newPass)
        {
            errorPanelManager.ShowError("Mật khẩu mới không được trùng mật khẩu cũ", Color.red);
            return;
        }

        StartCoroutine(ChangePasswordCoroutine());
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }

    IEnumerator ChangePasswordCoroutine()
    {
        string url = "http://localhost:5186/api/changepassword";

        ChangePasswordRequest data = new ChangePasswordRequest
        {
            Email = emailInput.text,
            OldPassword = oldPasswordInput.text,
            NewPassword = newPasswordInput.text
        };

        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Change password response: " + responseText);
            ChangePasswordResponse response = JsonUtility.FromJson<ChangePasswordResponse>(responseText);

            if (response != null && response.status)
            {
                errorPanelManager.ShowError("Đổi mật khẩu thành công!", Color.green);
                yield return new WaitForSeconds(1.5f);
                errorPanelManager.HideError();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                errorPanelManager.ShowError("" + (response?.message ?? "Thay đổi thất bại!"), Color.red);
                Debug.LogWarning("API báo lỗi đổi mật khẩu: " + response?.message);
            }
        }
        else
        {
            if (responseText.StartsWith("\"") && responseText.EndsWith("\""))
                responseText = responseText.Trim('"');

            if (string.IsNullOrEmpty(responseText))
                responseText = $"Lỗi máy chủ ({request.responseCode})";

            errorPanelManager.ShowError("" + responseText, Color.red);
            Debug.LogError("Lỗi kết nối API: " + responseText);
        }
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    [System.Serializable]
    public class ChangePasswordResponse
    {
        public bool status;
        public string message;
        public object data;
    }
}

[System.Serializable]
public class ChangePasswordRequest
{
    public string Email;
    public string OldPassword;
    public string NewPassword;
}
