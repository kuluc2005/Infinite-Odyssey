using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SettingPanelManager : MonoBehaviour
{
    public GameObject panelSetting;

    // Cách phổ biến nhất (1 player, hoặc player local được tag "Player")
    private PlayerPositionManager GetCurrentPlayerPositionManager()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go ? go.GetComponent<PlayerPositionManager>() : null;
    }
    void Update()
    {
        // Nếu Panel đang ẩn, nhấn ESC sẽ mở Setting Panel
        if (!panelSetting.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
            OpenSetting();
        }
        // Nếu Panel đang mở, nhấn ESC sẽ tắt Setting Panel
        else if (panelSetting.activeSelf && Input.GetKeyDown(KeyCode.P))
        {
            CloseSetting();
        }
    }


    public void OpenSetting()
    {
        panelSetting.SetActive(true);
        Time.timeScale = 0f;
        // ===== Hiện và unlock chuột =====
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSetting()
    {
        panelSetting.SetActive(false);
        Time.timeScale = 1f;
        // ===== Ẩn và lock chuột lại nếu cần =====
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnRegisterClicked()
    {
        Time.timeScale = 1f; // Resume nếu có pause
        SceneManager.LoadScene("RegisterScene");
    }

    public void OnLoginClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoginScene");
    }

    public void OnChangePasswordClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("ChangePasswordScene");
    }

    public void OnLogoutClicked()
    {
        StartCoroutine(SaveAndLogout());
    }

    public void OnExitGameClicked()
    {
        StartCoroutine(SaveAndExit());
    }

    private IEnumerator SaveAndLogout()
    {
        // Lưu vị trí
        var ppm = GetCurrentPlayerPositionManager();
        if (ppm != null)
        {
            ppm.SavePlayerPosition();
            yield return new WaitForSeconds(0.5f);
        }

        // Lưu tất cả nhiệm vụ active, chờ từng quest gửi xong mới logout
        if (QuestManager.instance != null)
        {
            if (QuestManager.instance.activeQuests.Count == 0)
                goto NEXT_STEP;

            bool anyFail = false;

            foreach (var quest in QuestManager.instance.activeQuests.Values)
            {
                bool done = false;
                bool resultOk = false;

                // Log trạng thái trước khi gửi
                Debug.Log($"[Logout] Gửi SaveQuestToApi: {quest.questID} | currentAmount={quest.objectives[0].currentAmount}/{quest.objectives[0].requiredAmount}");

                QuestManager.instance.SaveQuestToApi(quest, "active", ok => { done = true; resultOk = ok; });
                while (!done) yield return null;

                if (resultOk)
                {
                    Debug.Log($"[Logout] Đã lưu quest {quest.questID} thành công!");
                }
                else
                {
                    Debug.LogWarning($"[Logout] Lưu quest {quest.questID} thất bại! Không logout để tránh mất nhiệm vụ.");
                    anyFail = true;
                }
            }

            if (anyFail)
            {
                // Có thể báo popup ở đây nếu bạn muốn
                Debug.LogError("[Logout] Có nhiệm vụ lưu thất bại. Hãy kiểm tra kết nối mạng hoặc thử lại!");
                yield break; // Không logout
            }
        }

    NEXT_STEP:
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoginScene");
    }



    private IEnumerator SaveAndExit()
    {
        var ppm = GetCurrentPlayerPositionManager();
        if (ppm != null)
        {
            ppm.SavePlayerPosition();
            yield return new WaitForSeconds(0.5f);
        }

        if (QuestManager.instance != null)
        {
            foreach (var quest in QuestManager.instance.activeQuests.Values)
            {
                QuestManager.instance.SaveQuestToApi(quest, "active");
                yield return new WaitForSeconds(0.2f);
            }
        }

        Application.Quit();
    }
    // ...
}