using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string sceneToLoad = "Level 1"; // Tên scene muốn chuyển tới

    private void OnTriggerEnter(Collider other)
    {
        // Nếu tag của player là "Player" thì chuyển scene
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
