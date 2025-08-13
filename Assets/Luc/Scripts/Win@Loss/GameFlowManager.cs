using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameFlowManager
{
    private const string RESULT_SCENE = "ResultScene";

    // Đặt tên scene cutscene theo ý bạn (đúng với Build Settings)
    private const string CUTSCENE_WIN_MALE = "CutSceneEndMale";
    private const string CUTSCENE_WIN_FEMALE = "CutSceneEndFemale";

    private static readonly string[] LEVELS = { "Level 0", "Level 1", "Level 2", "Level 3" };

    // ====== Public APIs sẽ gọi  ======

    public static void Win()
    {
        SetCommonState("Win");                    
        string cutscene = GetWinCutsceneName();   
        SceneManager.LoadScene(cutscene);         
    }

    private static string GetWinCutsceneName()
    {
        string cls = ProfileManager.CurrentProfile != null ? ProfileManager.CurrentProfile.characterClass : null;
        if (string.IsNullOrEmpty(cls)) return CUTSCENE_WIN_MALE;

        cls = cls.ToLowerInvariant();
        // tuỳ dữ liệu của bạn: "Female"/"Nữ" → female
        if (cls.Contains("female") || cls.Contains("nu") || cls.Contains("nữ"))
            return CUTSCENE_WIN_FEMALE;

        return CUTSCENE_WIN_MALE;
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
