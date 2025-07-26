using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName; // Ví dụ: Cutscene_Level1
    public string nextLevelName;     // Ví dụ: Level1

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ✅ Lưu Level đích vào PlayerPrefs
            PlayerPrefs.SetString("NextLevel", nextLevelName);

            // ✅ Load Cutscene trước
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }
}
