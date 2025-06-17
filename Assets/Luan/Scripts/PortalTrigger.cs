using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    public string sceneToLoad = "Level2";
    public GameObject loadingScreen;

    private bool isLoading = false; // Ngăn gọi lại nhiều lần

    private void OnTriggerEnter(Collider other)
    {
        if (!isLoading && other.CompareTag("Player"))
        {
            isLoading = true;

            // Vô hiệu hóa collider để tránh bị trigger lại
            GetComponent<Collider>().enabled = false;

            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            StartCoroutine(LoadSceneAsync());
        }
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float fakeProgress = 0f;

        // Tăng từ 0% đến 90% đều đặn
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.25f; // chỉnh tốc độ tại đây
            LoadingUI.instance?.UpdateProgress(fakeProgress);
            yield return null;
        }

        // Đợi thật sự scene load đến 90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Tăng từ 90% → 100% nhẹ nhàng
        float finalProgress = 0.9f;
        while (finalProgress < 1f)
        {
            finalProgress += Time.deltaTime * 0.2f;
            LoadingUI.instance?.UpdateProgress(finalProgress);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // delay một chút trước khi load scene
        asyncLoad.allowSceneActivation = true;
    }

}
