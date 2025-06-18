using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{
    public void SelectMale()
    {
        if (ProfileManager.CurrentProfile == null)
        {
            Debug.LogError("⚠ Profile chưa load xong!");
            return;
        }

        PlayerPrefs.SetString("SelectedCharacter", "Male");
        StartCoroutine(UpdateProfileAndLoadScene("Male", 120, 80));
    }


    public void SelectFemale()
    {
        if (ProfileManager.CurrentProfile == null)
        {
            Debug.LogError("⚠ Profile chưa load xong!");
            return;
        }

        PlayerPrefs.SetString("SelectedCharacter", "Female");
        StartCoroutine(UpdateProfileAndLoadScene("Female", 100, 100));
    }

    IEnumerator UpdateProfileAndLoadScene(string characterClass, int hp, int mp)
    {
        // Lấy PlayerProfile đang dùng
        ProfileManager.CurrentProfile.characterClass = characterClass;
        ProfileManager.CurrentProfile.hP = hp;
        ProfileManager.CurrentProfile.mP = mp;

        string json = JsonUtility.ToJson(ProfileManager.CurrentProfile);
        Debug.Log("📤 JSON gửi lên API: " + json);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:5186/api/update-profile", "PUT");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Đã cập nhật profile thành công");
            SceneManager.LoadScene("SceneMain"); // Vào game
        }
        else
        {
            Debug.LogError("❌ Không thể cập nhật profile: " + request.error);
        }
    }

}
