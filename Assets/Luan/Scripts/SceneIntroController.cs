using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneIntroController : MonoBehaviour
{
    public PlayableDirector introTimeline;
    public Button skipButton;
    public GameObject timelineCameras;
    public GameObject[] objectsToHide;

    private bool hasEnded = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timelineCameras != null)
            timelineCameras.SetActive(true);

        if (introTimeline != null)
        {
            introTimeline.time = 0;                  
            introTimeline.Evaluate();                
            introTimeline.stopped -= OnTimelineFinished;
            introTimeline.stopped += OnTimelineFinished;
            introTimeline.Play();                     
        }


        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTimeline);
        }
    }

    //void Update()
    //{
    //    if (introTimeline != null && introTimeline.time >= introTimeline.duration && !hasEnded)
    //    {
    //        EndIntro();
    //    }
    //}

    public void SkipTimeline()
    {
        if (introTimeline != null) introTimeline.Stop();
        EndIntro();
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        EndIntro();
    }

    void EndIntro()
    {
        if (hasEnded) return;
        hasEnded = true;

        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Lấy Level đích từ PlayerPrefs
        string nextLevel = PlayerPrefs.GetString("NextLevel", "Level 0");
        Debug.Log($"Cutscene kết thúc → load {nextLevel}");
        SceneManager.LoadScene(nextLevel);
    }
}
