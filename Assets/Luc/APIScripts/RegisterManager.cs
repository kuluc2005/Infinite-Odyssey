using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;
    public TMP_Text statusText;

    public void OnRegisterButtonClick()
    {
        StartCoroutine(RegisterCoroutine());
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }


    IEnumerator RegisterCoroutine()
    {
        string url = "http://localhost:5186/api/register";

        PlayerRegister registerData = new PlayerRegister
        {
            Email = emailInput.text,
            PasswordHash = passwordInput.text,
            UserName = usernameInput.text
        };

        string json = JsonUtility.ToJson(registerData);
        Debug.Log("📤 JSON gửi đi từ Unity: " + json);  // Thêm dòng này
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Phản hồi: " + request.downloadHandler.text);

            if (request.downloadHandler.text.Contains("true"))
            {
                statusText.text = "Đăng ký thành công!";
                yield return new WaitForSeconds(1.5f);
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                statusText.text = "Đăng ký thất bại.";
            }
        }
        else
        {
            statusText.text = "!!!!!Lỗi: " + request.error;
        }
    }
}

[System.Serializable]
public class PlayerRegister
{
    public string Email;
    public string PasswordHash;
    public string UserName;
}
