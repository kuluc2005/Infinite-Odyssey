using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelPortal : MonoBehaviour
{
    public string cutsceneSceneName;
    public string nextLevelName;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetString("NextLevel", nextLevelName);

            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                StartCoroutine(SaveExpAndLoad(stats));
            }
            else
            {
                Debug.LogWarning("Không tìm thấy PlayerStats trên Player, load scene ngay.");
                SceneManager.LoadScene(cutsceneSceneName);
            }
        }
    }

    IEnumerator SaveExpAndLoad(PlayerStats stats)
    {
        PlayerPositionManager ppm = stats.GetComponent<PlayerPositionManager>();
        if (ppm != null)
        {
            ppm.UpdateProfile(profile =>
            {
                profile.exp = stats.currentExp;
            });

            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene(cutsceneSceneName);
    }

}
