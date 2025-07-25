using UnityEngine;

public class MouseUnlockOnSceneLoad : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f; 
    }
}
