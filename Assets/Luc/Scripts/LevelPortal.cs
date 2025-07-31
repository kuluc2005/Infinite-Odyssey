using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName;
    public string nextLevelName;
    public Transform targetSpawnPoint; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetString("NextLevel", nextLevelName);

            if (targetSpawnPoint != null)
            {
                PlayerPrefs.SetFloat("SpawnX", targetSpawnPoint.position.x);
                PlayerPrefs.SetFloat("SpawnY", targetSpawnPoint.position.y);
                PlayerPrefs.SetFloat("SpawnZ", targetSpawnPoint.position.z);
            }

            SceneManager.LoadScene(cutsceneSceneName);
        }
    }
}
