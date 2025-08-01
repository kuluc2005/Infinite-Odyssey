using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName;
    public string nextLevelName;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetString("NextLevel", nextLevelName);
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }
}
