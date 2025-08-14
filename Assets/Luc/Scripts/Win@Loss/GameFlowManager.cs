using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameFlowManager
{
    private const string RESULT_SCENE = "ResultScene";
    private const string CUTSCENE_WIN_MALE = "CutSceneEndMale";
    private const string CUTSCENE_WIN_FEMALE = "CutSceneEndFemale";
    private static readonly string[] LEVELS = { "Level 0", "Level 1", "Level 2", "Level 3" };

    public static void Win()
    {
        SetCommonState("Win");
        string cutscene = GetWinCutsceneName();
        SceneTransition.Load(cutscene); // <— dùng transition
    }

    public static void Lose()
    {
        SetCommonState("Lose");
        SceneTransition.Load(RESULT_SCENE);
    }

    public static void RetryLastLevel()
    {
        string last = PlayerPrefs.GetString("LastLevel", LEVELS[0]);
        SceneTransition.Load(last);
    }

    public static void GoCharacterSelect()
    {
        SceneTransition.Load("CharacterSelectScene");
    }

    public static void NextFromLastLevelOrBack()
    {
        string last = PlayerPrefs.GetString("LastLevel", LEVELS[0]);
        string next = GetNextLevelName(last);
        if (!string.IsNullOrEmpty(next)) SceneTransition.Load(next);
        else GoCharacterSelect();
    }

    private static string GetWinCutsceneName()
    {
        string cls = ProfileManager.CurrentProfile != null ? ProfileManager.CurrentProfile.characterClass : null;
        if (string.IsNullOrEmpty(cls)) return CUTSCENE_WIN_MALE;
        cls = cls.ToLowerInvariant();
        if (cls.Contains("female") || cls.Contains("nu") || cls.Contains("nữ"))
            return CUTSCENE_WIN_FEMALE;
        return CUTSCENE_WIN_MALE;
    }

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
