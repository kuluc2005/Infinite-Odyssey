using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ResultSceneController : MonoBehaviour
{
    public GameObject winMaleUI;
    public GameObject winFemaleUI;
    public GameObject loseMaleUI;
    public GameObject loseFemaleUI;
    public void OnRetryLevel() => GameFlowManager.RetryLastLevel();

    void Awake()
    {
        if (winMaleUI) winMaleUI.SetActive(false);
        if (winFemaleUI) winFemaleUI.SetActive(false);
        if (loseMaleUI) loseMaleUI.SetActive(false);
        if (loseFemaleUI) loseFemaleUI.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator Start()
    {
        int guard = 0;
        while (ProfileManager.CurrentProfile == null && guard < 300) 
        {
            guard++;
            yield return null;
        }

        // Lấy class
        string cls = ProfileManager.CurrentProfile != null
            ? ProfileManager.CurrentProfile.characterClass
            : PlayerPrefs.GetString("SelectedClass", "Male"); // dự phòng

        string result = PlayerPrefs.GetString("LastResult", "Win");
        bool isFemale = (!string.IsNullOrEmpty(cls) && cls == "Female");

        // Bật đúng UI
        if (result == "Win")
        {
            if (isFemale) { if (winFemaleUI) winFemaleUI.SetActive(true); }
            else { if (winMaleUI) winMaleUI.SetActive(true); }
        }
        else
        {
            if (isFemale) { if (loseFemaleUI) loseFemaleUI.SetActive(true); }
            else { if (loseMaleUI) loseMaleUI.SetActive(true); }
        }
    }

    // ===== Buttons =====
    public void OnBackToSelect()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public static void RetryLastLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", "Level 0");
        UnityEngine.SceneManagement.SceneManager.LoadScene(last);
    }


    public void OnNextLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", "Level 0");
        string next = GetNextLevelName(last);
        if (!string.IsNullOrEmpty(next)) SceneManager.LoadScene(next);
        else SceneManager.LoadScene("CharacterSelectScene"); // hết level
    }

    public void OnQuitGame()
    {
        //  test trong Unity Editor thì dừng Play Mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // Khi build ra, thoát hẳn game
    Application.Quit();
#endif
    }


    // Map 4 level
    private string GetNextLevelName(string current)
    {
        switch (current)
        {
            case "Level 0": return "Level 1";
            case "Level 1": return "Level 2";
            case "Level 2": return "Level 3";
            default: return null;
        }
    }
}
