using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class ChangePasswordManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;
    public ErrorPanelManager errorPanelManager; // Kéo vào Inspector
    public TMP_Text statusText;

    public void OnChangePasswordButtonClick()
    {
        StartCoroutine(ChangePasswordCoroutine());
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    } public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }


    IEnumerator ChangePasswordCoroutine()
    {
        string url = "http://localhost:5186/api/changepassword";

        ChangePasswordRequest data = new ChangePasswordRequest
        {
            Email = emailInput.text,
            OldPassword = oldPasswordInput.text,     // <-- Lấy từ input mật khẩu cũ
            NewPassword = newPasswordInput.text      // <-- Lấy từ input mật khẩu mới
        };

        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Change password response: " + request.downloadHandler.text);
            ChangePasswordResponse response = JsonUtility.FromJson<ChangePasswordResponse>(request.downloadHandler.text);

            if (response != null && response.status)
            {
                errorPanelManager.ShowError("Đổi mật khẩu thành công!",Color.green);
                yield return new WaitForSeconds(1.5f);
                errorPanelManager.HideError();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                errorPanelManager.ShowError("❌" + (response?.message ?? "Thay đổi thất bại!"), Color.red);
                Debug.LogWarning("API báo lỗi đổi mật khẩu: " + response?.message);
            }
        }
        else
        {
            errorPanelManager.ShowError("Lỗi kết nối:" + request.error);
            Debug.LogError("Lỗi kết nối API: " + request.error);
        }
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

