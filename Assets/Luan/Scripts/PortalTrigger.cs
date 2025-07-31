//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;
//using Invector.vItemManager;

//public class PortalTrigger : MonoBehaviour
//{
//    public string sceneToLoad = "Level2";
//    public GameObject loadingScreen;

//    private bool isLoading = false;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!isLoading && other.CompareTag("Player"))
//        {
//            isLoading = true;

//            // ✅ Lưu Inventory
//            var player = other.gameObject;
//            var inv = player.GetComponent<vItemManager>();
//            if (inv != null)
//            {
//                InventorySaveManager.instance.SaveInventory(inv);
//            }

//            // ✅ Tắt trigger
//            GetComponent<Collider>().enabled = false;

//            // ✅ Bật màn hình loading nếu có
//            if (loadingScreen != null)
//                loadingScreen.SetActive(true);

//            // ✅ Bắt đầu load scene
//            StartCoroutine(LoadSceneAsync());
//        }
//    }

//    IEnumerator LoadSceneAsync()
//    {
//        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
//        asyncLoad.allowSceneActivation = false;

//        float fakeProgress = 0f;

//        while (fakeProgress < 0.9f)
//        {
//            fakeProgress += Time.deltaTime * 0.25f;
//            LoadingUI.instance?.UpdateProgress(fakeProgress);
//            yield return null;
//        }

//        while (asyncLoad.progress < 0.9f)
//        {
//            yield return null;
//        }

//        float finalProgress = 0.9f;
//        while (finalProgress < 1f)
//        {
//            finalProgress += Time.deltaTime * 0.2f;
//            LoadingUI.instance?.UpdateProgress(finalProgress);
//            yield return null;
//        }

//        yield return new WaitForSeconds(0.5f);
//        asyncLoad.allowSceneActivation = true;
//    }
//}
