using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("Assign the same Pause Canvas used by UIMenuManager")]
    public GameObject pauseCanvas;   // Main Pause Menu
    public GameObject optionsCanvas; // Option Menu

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Nếu đang ở Option → không làm gì cả
            if (optionsCanvas != null && optionsCanvas.activeSelf)
                return;

            // Nếu đang pause → Resume
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsPaused => isPaused;
}
