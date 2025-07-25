using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneIntroController : MonoBehaviour
{
    [Header("API & Spawn Settings")]
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public Transform spawnPoint;

    [Header("Timeline Settings")]
    public PlayableDirector introTimeline;
    public Button skipButton;

    [Header("Cameras")]
    public GameObject timelineCameras;
    public GameObject playerCamera;

    [Header("Objects to hide after intro")]
    public GameObject[] objectsToHide;

    private GameObject spawnedPlayer;

    void Start()
    {
        // ✅ Unlock chuột khi Intro chạy để có thể click Skip
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ✅ Bật camera timeline, tắt camera player
        if (timelineCameras != null) timelineCameras.SetActive(true);
        if (playerCamera != null) playerCamera.SetActive(false);

        // ✅ Play Timeline
        if (introTimeline != null) introTimeline.Play();

        // ✅ Setup Skip button
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTimeline);
        }

        // ✅ Khi Timeline kết thúc, gọi EndIntro
        introTimeline.stopped += OnTimelineFinished;

        // ✅ Lấy CharacterId từ PlayerPrefs
        int characterId = PlayerPrefs.GetInt("CharacterId", -1);
        Debug.Log($"🎯 [SceneIntroController] CharacterId hiện tại: {characterId}");

        if (characterId > 0)
        {
            StartCoroutine(LoadCharacterAndSpawn(characterId));
        }
        else
        {
            Debug.LogError("❌ Không có CharacterId! Quay lại màn hình chọn nhân vật...");
            SceneManager.LoadScene("CharacterSelectScene"); // fallback
        }
    }

    IEnumerator LoadCharacterAndSpawn(int characterId)
    {
        string url = $"http://localhost:5186/api/character/profile/{characterId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ API lỗi: " + request.error);
            yield break;
        }

        var wrapper = JsonUtility.FromJson<PlayerSpawner.PlayerProfileWrapper>(request.downloadHandler.text);

        if (wrapper != null && wrapper.data != null)
        {
            string characterClass = wrapper.data.characterClass;
            GameObject prefabToSpawn = characterClass == "Female" ? femalePrefab : malePrefab;

            // ✅ Spawn position mặc định
            Vector3 spawnPos = spawnPoint.position;
            Quaternion spawnRot = spawnPoint.rotation;

            // ✅ Nếu có checkpoint hợp lệ thì spawn theo checkpoint
            if (!string.IsNullOrEmpty(wrapper.data.currentCheckpoint) && wrapper.data.currentCheckpoint.Contains(","))
            {
                string[] split = wrapper.data.currentCheckpoint.Split(':');
                string posString = split.Length > 1 ? split[1] : split[0];

                string[] coords = posString.Split(',');
                if (coords.Length == 3)
                {
                    float x, y, z;
                    if (float.TryParse(coords[0], out x) &&
                        float.TryParse(coords[1], out y) &&
                        float.TryParse(coords[2], out z))
                    {
                        spawnPos = new Vector3(x, y, z);
                        Debug.Log($"✅ Spawn theo checkpoint: {spawnPos}");
                    }
                }
            }

            // ✅ Spawn Player
            spawnedPlayer = Instantiate(prefabToSpawn, spawnPos, spawnRot);

            // Disable control khi intro đang chạy
            var controller = spawnedPlayer.GetComponent<Invector.vCharacterController.vThirdPersonController>();
            if (controller != null) controller.enabled = false;

            var input = spawnedPlayer.GetComponent<Invector.vCharacterController.vThirdPersonInput>();
            if (input != null) input.enabled = false;

            Canvas[] uiElements = spawnedPlayer.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas ui in uiElements)
            {
                ui.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"❌ Không tìm thấy profile cho nhân vật ID {characterId}. Quay lại chọn nhân vật...");
            SceneManager.LoadScene("CharacterSelectScene");
        }
    }

    public void SkipTimeline()
    {
        introTimeline.Stop();
        EndIntro();
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        EndIntro();
    }

    void EndIntro()
    {
        if (timelineCameras != null) timelineCameras.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        if (spawnedPlayer != null)
        {
            var controller = spawnedPlayer.GetComponent<Invector.vCharacterController.vThirdPersonController>();
            if (controller != null) controller.enabled = true;

            var input = spawnedPlayer.GetComponent<Invector.vCharacterController.vThirdPersonInput>();
            if (input != null) input.enabled = true;

            Canvas[] uiElements = spawnedPlayer.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas ui in uiElements)
            {
                ui.gameObject.SetActive(true);
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (skipButton != null) skipButton.gameObject.SetActive(false);

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}
