using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "BackGVideo1.mp4");

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        videoPlayer.url = "file:///" + videoPath;
#else
    videoPlayer.url = videoPath;
#endif

        if (System.IO.File.Exists(videoPath))
        {
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video file not found at: " + videoPath);
        }
    }

}
