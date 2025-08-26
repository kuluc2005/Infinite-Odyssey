// WinCutsceneController.cs — GẮN VÀO SCENE Cutscene_Win_Male/Female
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinCutsceneController : MonoBehaviour
{
    [Header("Nếu dùng Timeline, kéo PlayableDirector vào đây (optional)")]
    public PlayableDirector director;

    [Header("Nếu không dùng Timeline, fallback thời lượng (giây)")]
    [Range(0f, 60f)] public float fallbackDuration = 8f;

    [Header("Cho phép bấm phím bất kỳ để bỏ qua")]
    public bool allowSkipAnyKey = true;

    bool _done; 

    IEnumerator Start()
    {
        PlayerPrefs.SetString("LastResult", "Win");

        if (director != null)
        {
            director.stopped += OnDirectorStopped;
            if (director.state != PlayState.Playing) director.Play();
            yield break;
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, fallbackDuration));
        GoToResult();
    }

    void Update()
    {
        if (!_done && allowSkipAnyKey && Input.anyKeyDown)
            GoToResult();
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        GoToResult();
    }

    void GoToResult()
    {
        if (_done) return;
        _done = true;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneTransition.Load("ResultScene"); 
    }
}
