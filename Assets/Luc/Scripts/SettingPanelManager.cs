using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SettingPanelManager : MonoBehaviour
{
    public GameObject panelSetting;

    // Cách phổ biến nhất (1 player, hoặc player local được tag "Player")
    private PlayerPositionManager GetCurrentPlayerPositionManager()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go ? go.GetComponent<PlayerPositionManager>() : null;
    }
    void Update()
    {
        // Nếu Panel đang ẩn, nhấn ESC sẽ mở Setting Panel
        if (!panelSetting.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
            OpenSetting();
        }
        // Nếu Panel đang mở, nhấn ESC sẽ tắt Setting Panel
        else if (panelSetting.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
            CloseSetting();
        }
    }


    public void OpenSetting()
    {
        panelSetting.SetActive(true);
        Time.timeScale = 0f;
        // ===== Hiện và unlock chuột =====
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSetting()
    {
        panelSetting.SetActive(false);
        Time.timeScale = 1f;
        // ===== Ẩn và lock chuột lại nếu cần =====
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnRegisterClicked()
    {
        Time.timeScale = 1f; // Resume nếu có pause
        SceneManager.LoadScene("RegisterScene");
    }

    public void OnLoginClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoginScene");
    }

    public void OnChangePasswordClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("ChangePasswordScene");
    }

    public void OnLogoutClicked()
    {
        StartCoroutine(SaveAndLogout());
    }

    public void OnExitGameClicked()
    {
        StartCoroutine(SaveAndExit());
    }

    private IEnumerator SaveAndLogout()
    {
        var ppm = GetCurrentPlayerPositionManager();
        if (ppm != null)
        {
            ppm.SavePlayerPosition();
            yield return new WaitForSeconds(1f);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoginScene");
    }

    private IEnumerator SaveAndExit()
    {
        var ppm = GetCurrentPlayerPositionManager();
        if (ppm != null)
        {
            ppm.SavePlayerPosition();
            yield return new WaitForSeconds(1f);
        }
        Application.Quit();
    }
    // ...
}
