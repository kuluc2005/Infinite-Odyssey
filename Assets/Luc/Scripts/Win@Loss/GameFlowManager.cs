using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameFlowManager
{
    private const string RESULT_SCENE = "ResultScene";

    // Danh sách level (đúng tên scene)
    private static readonly string[] LEVELS = { "Level 0", "Level 1", "Level 2", "Level 3" };

    // ====== Public APIs sẽ gọi  ======

    public static void Win()
    {
        SetCommonState("Win");
        SceneManager.LoadScene(RESULT_SCENE);
    }

    public static void Lose()
    {
        SetCommonState("Lose");
        SceneManager.LoadScene(RESULT_SCENE);
    }

    public static void RetryLastLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", LEVELS[0]);
        SceneManager.LoadScene(last);
    }

    public static void GoCharacterSelect()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public static void NextFromLastLevelOrBack()
    {
        string last = PlayerPrefs.GetString("LastLevel", LEVELS[0]);
        string next = GetNextLevelName(last);
        if (!string.IsNullOrEmpty(next)) SceneManager.LoadScene(next);
        else GoCharacterSelect(); 
    }

    // ====== Helpers ======

    private static void SetCommonState(string result)
    {
        string cur = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastResult", result);
        PlayerPrefs.SetString("LastLevel", cur);


        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static string GetNextLevelName(string current)
    {
        for (int i = 0; i < LEVELS.Length; i++)
            if (LEVELS[i] == current && i + 1 < LEVELS.Length)
                return LEVELS[i + 1];
        return null;
    }
}
