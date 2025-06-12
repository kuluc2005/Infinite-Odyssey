using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class ChangePasswordManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField newPasswordInput;
    public TMP_Text statusText;

    public void OnChangePasswordButtonClick()
    {
        StartCoroutine(ChangePasswordCoroutine());
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }


    IEnumerator ChangePasswordCoroutine()
    {
        string url = "http://localhost:5186/api/changepassword";

        PlayerChangePassword data = new PlayerChangePassword
        {
            Email = emailInput.text,
            PasswordHash = newPasswordInput.text
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
            if (request.downloadHandler.text.Contains("true"))
            {
                statusText.text = "✅ Đổi mật khẩu thành công!";
                yield return new WaitForSeconds(1.5f);
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                statusText.text = "❌ Thay đổi thất bại!";
            }
        }
        else
        {
            statusText.text = "⚠️ Lỗi kết nối: " + request.error;
        }
    }
}

[System.Serializable]
public class PlayerChangePassword
{
    public string Email;
    public string PasswordHash;
}
