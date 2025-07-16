using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "BackGVideo1.mp4"; 
    public RenderTexture sharedRenderTexture;

    public AudioSource bgMusicSource;
    public AudioClip bgMusicClip;

    void Start()
    {
        if (bgMusicSource && bgMusicClip)
        {
            bgMusicSource.clip = bgMusicClip;
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        videoPlayer.url = "file:///" + videoPath;
#else
        videoPlayer.url = videoPath;
#endif

        if (System.IO.File.Exists(videoPath))
        {
            videoPlayer.targetTexture = sharedRenderTexture; 
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video not found: " + videoPath);
        }
    }
}
