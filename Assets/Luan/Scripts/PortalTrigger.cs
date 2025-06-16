using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public string sceneToLoad = "LoadScene"; // CHUYỂN QUA scene Loading trước
    public GameObject loadingScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            loadingScreen.SetActive(true);
            SceneManager.LoadScene(sceneToLoad); // Không cần coroutine
        }
    }
}
