using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    public void OnLoginButtonClick()
    {
        StartCoroutine(LoginCoroutine());
    }

    IEnumerator LoginCoroutine()
    {
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
            statusText.text = "Đăng nhập thành công!";
            Debug.Log(request.downloadHandler.text);
            // Chuyển scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("SceneMain");
        }
        else
        {
            statusText.text = "Lỗi: " + request.error;
        }
    }
}

[System.Serializable]
public class PlayerLogin
{
    public string Email;
    public string PasswordHash;
}
