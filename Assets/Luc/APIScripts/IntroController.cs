using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;     
using UnityEngine.Video;         

public class IntroController : MonoBehaviour
{
    [Header("Optional refs")]
    public PlayableDirector timeline;  
    public VideoPlayer video;          

    private bool fired;

    void Start()
    {
        // Auto-find nếu quên kéo
        if (!timeline) timeline = GetComponent<PlayableDirector>();
        if (!video) video = GetComponent<VideoPlayer>();

        // Đăng ký sự kiện kết thúc Timeline
        if (timeline)
        {
            timeline.stopped += OnTimelineStopped;
        }

        if (video)
        {
            video.loopPointReached += OnVideoFinished;
        }
    }

    void OnDestroy()
    {
        if (timeline) timeline.stopped -= OnTimelineStopped;
        if (video) video.loopPointReached -= OnVideoFinished;
    }

    void Update()
    {
        // Nhấn S để bỏ qua
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnSkipIntro();
        }
    }

    private void OnTimelineStopped(PlayableDirector d)
    {
        OnIntroFinished();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        OnIntroFinished();
    }

    public void OnIntroFinished()
    {
        if (fired) return;
        fired = true;

        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        if (playerId != -1)
        {
            PlayerPrefs.SetInt($"IntroSeen_{playerId}", 1);
            PlayerPrefs.Save();
        }

        // Chuyển qua tạo nhân vật
        SceneManager.LoadScene("CreateCharacterScene");
    }

    public void OnSkipIntro()
    {
        if (timeline && timeline.state == PlayState.Playing) timeline.time = timeline.duration;
        if (video && video.isPlaying) video.Stop();

        OnIntroFinished();
    }
}
