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
    public ErrorPanelManager errorPanelManager; // Kéo vào Inspector
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
        Debug.Log("JSON gửi đi từ Unity: " + json);  // Thêm dòng này
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Phản hồi: " + request.downloadHandler.text);
            RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

            if (response != null && response.status)
            {
                // Thành công thì vẫn có thể dùng statusText hoặc popup riêng
                errorPanelManager.ShowError("Đăng ký thành công!",Color.green);
                yield return new WaitForSeconds(1.5f);
                errorPanelManager.HideError();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
            else
            {
                errorPanelManager.ShowError("❌" + (response?.message ?? "Đăng ký thất bại."), Color.red);
            }
        }
        else
        {
            errorPanelManager.ShowError("!!!!!Lỗi: " + request.error);
            Debug.LogError("Đăng ký lỗi: " + request.error);
        }
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
        //public object data; // Không dùng cũng được
    }
}
