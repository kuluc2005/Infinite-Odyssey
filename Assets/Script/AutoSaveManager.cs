using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveManager : MonoBehaviour
{
    void OnApplicationQuit()
    {
        SaveCurrentScene();
    }

    void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SaveScene", currentScene);
        PlayerPrefs.Save();
        Debug.Log("Đã tự động lưu scene: " + currentScene);
    }
}
