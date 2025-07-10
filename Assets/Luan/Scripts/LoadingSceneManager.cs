using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public string sceneToLoad = "Level2";
    private LoadingUI loadingUI;

    private void Start()
    {
        loadingUI = FindObjectOfType<LoadingUI>();
        StartCoroutine(LoadAsyncScene());
    }

    System.Collections.IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingUI != null)
                loadingUI.SetProgress(progress);

            // Khi tiến trình đạt 90% => load xong
            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f); // hiệu ứng delay 1s nếu muốn
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
