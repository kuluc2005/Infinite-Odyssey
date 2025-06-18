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

    public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
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
            string response = request.downloadHandler.text;
            Debug.Log("📦 JSON raw từ API: " + response);
            Debug.Log("Login response: " + response);

            LoginResponse loginResult = JsonUtility.FromJson<LoginResponse>(response);

            if (loginResult != null && loginResult.data != null)
            {
                Debug.Log($"🔥 Đã lưu PlayerId: {loginResult.data.id}");
                PlayerPrefs.SetInt("PlayerId", loginResult.data.id);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("❌ Không giải mã được loginResult hoặc loginResult.data null");
            }

            statusText.text = "Đăng nhập thành công!";
            yield return new WaitForSeconds(1f);
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelectScene");
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

