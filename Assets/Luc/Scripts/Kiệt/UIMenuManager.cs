using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu
{
    public class UIMenuManager : MonoBehaviour
    {
        private Animator CameraObject;

        private PlayerPositionManager GetCurrentPlayerPositionManager()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            return go ? go.GetComponent<PlayerPositionManager>() : null;
        }

        [Header("MENUS")]
        public GameObject mainMenu;
        public GameObject firstMenu;
        public GameObject exitMenu;

        [Header("PAUSE / SETTINGS")]
        public GameObject pauseCanvas;
        public GameObject settingsCanvas;

        [Header("Refs")]
        public PauseManager pauseManager;

        public enum Theme { custom1, custom2, custom3 };
        [Header("THEME SETTINGS")]
        public Theme theme;
        private int themeIndex;
        public ThemedUIData themeController;

        [Header("PANELS")]
        public GameObject mainCanvas;
        public GameObject PanelFunction;   // Đổi từ PanelControls thành PanelFunction
        public GameObject PanelVideo;
        public GameObject PanelGame;
        public GameObject PanelKeyBindings;
        public GameObject PanelMovement;
        public GameObject PanelCombat;
        public GameObject PanelGeneral;

        [Header("SETTINGS SCREEN")]
        public GameObject lineGame;
        public GameObject lineVideo;
        public GameObject lineFunction;    // Đổi từ lineControls thành lineFunction
        public GameObject lineKeyBindings;
        public GameObject lineMovement;
        public GameObject lineCombat;
        public GameObject lineGeneral;

        [Header("LOADING SCREEN")]
        public bool waitForInput = true;
        public GameObject loadingMenu;
        public Slider loadingBar;
        public TMP_Text loadPromptText;
        public KeyCode userPromptKey;

        [Header("SFX")]
        public AudioSource hoverSound;
        public AudioSource sliderSound;
        public AudioSource swooshSound;

        void Start()
        {
            CameraObject = transform.GetComponent<Animator>();

            if (exitMenu) exitMenu.SetActive(false);
            if (firstMenu) firstMenu.SetActive(true);
            if (mainMenu) mainMenu.SetActive(true);

            if (pauseCanvas) pauseCanvas.SetActive(false);
            if (settingsCanvas) settingsCanvas.SetActive(false);

            SetThemeColors();
        }

        void SetThemeColors()
        {
            switch (theme)
            {
                case Theme.custom1:
                    themeController.currentColor = themeController.custom1.graphic1;
                    themeController.textColor = themeController.custom1.text1;
                    themeIndex = 0;
                    break;
                case Theme.custom2:
                    themeController.currentColor = themeController.custom2.graphic2;
                    themeController.textColor = themeController.custom2.text2;
                    themeIndex = 1;
                    break;
                case Theme.custom3:
                    themeController.currentColor = themeController.custom3.graphic3;
                    themeController.textColor = themeController.custom3.text3;
                    themeIndex = 2;
                    break;
            }
        }

        public void ContinueGame()
        {
            if (settingsCanvas) settingsCanvas.SetActive(false);
            if (exitMenu) exitMenu.SetActive(false);

            if (pauseManager != null)
            {
                pauseManager.ResumeGame();
            }
            else
            {
                if (pauseCanvas) pauseCanvas.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OpenSettings()
        {
            if (pauseCanvas) pauseCanvas.SetActive(false);
            if (settingsCanvas) settingsCanvas.SetActive(true);
        }

        public void Return()
        {
            if (settingsCanvas) settingsCanvas.SetActive(false);
            if (pauseCanvas) pauseCanvas.SetActive(true);
        }

        public void CloseSettings() => Return();

        public void ShowExitMenu()
        {
            if (pauseCanvas) pauseCanvas.SetActive(false);
            if (settingsCanvas) settingsCanvas.SetActive(false);
            if (exitMenu) exitMenu.SetActive(true);
        }

        public void BackFromExitToPause()
        {
            if (exitMenu) exitMenu.SetActive(false);
            if (pauseCanvas) pauseCanvas.SetActive(true);
        }

        public void ReturnMenu()
        {
            if (exitMenu) exitMenu.SetActive(false);
            if (mainMenu) mainMenu.SetActive(true);
        }

        public void LoadScene(string scene)
        {
            if (!string.IsNullOrEmpty(scene))
                StartCoroutine(LoadAsynchronously(scene));
        }

        void DisablePanels()
        {
            if (PanelFunction) PanelFunction.SetActive(false);
            if (PanelVideo) PanelVideo.SetActive(false);
            if (PanelGame) PanelGame.SetActive(false);
            if (PanelKeyBindings) PanelKeyBindings.SetActive(false);

            if (lineGame) lineGame.SetActive(false);
            if (lineFunction) lineFunction.SetActive(false);
            if (lineVideo) lineVideo.SetActive(false);
            if (lineKeyBindings) lineKeyBindings.SetActive(false);

            if (PanelMovement) PanelMovement.SetActive(false);
            if (lineMovement) lineMovement.SetActive(false);
            if (PanelCombat) PanelCombat.SetActive(false);
            if (lineCombat) lineCombat.SetActive(false);
            if (PanelGeneral) PanelGeneral.SetActive(false);
            if (lineGeneral) lineGeneral.SetActive(false);
        }

        public void GamePanel() { DisablePanels(); if (PanelGame) { PanelGame.SetActive(true); if (lineGame) lineGame.SetActive(true); } }
        public void VideoPanel() { DisablePanels(); if (PanelVideo) { PanelVideo.SetActive(true); if (lineVideo) lineVideo.SetActive(true); } }
        public void FunctionPanel() { DisablePanels(); if (PanelFunction) { PanelFunction.SetActive(true); if (lineFunction) lineFunction.SetActive(true); } }
        public void KeyBindingsPanel() { DisablePanels(); MovementPanel(); if (PanelKeyBindings) PanelKeyBindings.SetActive(true); if (lineKeyBindings) lineKeyBindings.SetActive(true); }
        public void MovementPanel() { DisablePanels(); if (PanelKeyBindings) PanelKeyBindings.SetActive(true); if (PanelMovement) PanelMovement.SetActive(true); if (lineMovement) lineMovement.SetActive(true); }
        public void CombatPanel() { DisablePanels(); if (PanelKeyBindings) PanelKeyBindings.SetActive(true); if (PanelCombat) PanelCombat.SetActive(true); if (lineCombat) lineCombat.SetActive(true); }
        public void GeneralPanel() { DisablePanels(); if (PanelKeyBindings) PanelKeyBindings.SetActive(true); if (PanelGeneral) PanelGeneral.SetActive(true); if (lineGeneral) lineGeneral.SetActive(true); }

        public void PlayHover() { if (hoverSound) hoverSound.Play(); }
        public void PlaySFXHover() { if (sliderSound) sliderSound.Play(); }
        public void PlaySwoosh() { if (swooshSound) swooshSound.Play(); }

        public void AreYouSure()
        {
            if (exitMenu) exitMenu.SetActive(true);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        IEnumerator LoadAsynchronously(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            if (mainCanvas) mainCanvas.SetActive(false);
            if (loadingMenu) loadingMenu.SetActive(true);

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.95f);
                if (loadingBar) loadingBar.value = progress;

                if (operation.progress >= 0.9f && waitForInput)
                {
                    if (loadPromptText) loadPromptText.text = "Press " + userPromptKey.ToString().ToUpper() + " to continue";
                    if (loadingBar) loadingBar.value = 1;

                    if (Input.GetKeyDown(userPromptKey))
                        operation.allowSceneActivation = true;
                }
                else if (operation.progress >= 0.9f && !waitForInput)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        public void OnRegisterClicked()
        {
            Time.timeScale = 1f;
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

        public void OnSelected()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("CharacterSelectScene");
        }

        public void OnLogoutClicked()
        {
            StartCoroutine(SaveAndLogout());
        }

        public void OnExitGameClicked()
        {
            StartCoroutine(SaveAndQuit());
        }

        private IEnumerator SaveAndLogout()
        {
            if (QuestManager.instance != null) QuestManager.instance.isLoggingOut = true;

            var ppm = GetCurrentPlayerPositionManager();
            if (ppm != null)
            {
                ppm.SavePlayerPosition();
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (QuestManager.instance != null)
            {
                if (QuestManager.instance.activeQuests.Count > 0)
                {
                    bool anyFail = false;

                    foreach (var quest in QuestManager.instance.activeQuests.Values)
                    {
                        bool done = false;
                        bool resultOk = false;

                        Debug.Log($"[Logout] Gửi SaveQuestToApi: {quest.questID} | currentAmount={quest.objectives[0].currentAmount}/{quest.objectives[0].requiredAmount}");

                        QuestManager.instance.SaveQuestToApi(quest, "active", ok => { done = true; resultOk = ok; });
                        while (!done) yield return null;

                        if (!resultOk)
                        {
                            Debug.LogWarning($"[Logout] Lưu quest {quest.questID} thất bại!");
                            anyFail = true;
                        }
                    }

                    if (anyFail)
                    {
                        Debug.LogError("[Logout] Có quest lưu thất bại. Hủy logout để tránh mất tiến độ.");
                        yield break;
                    }
                }
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene("LoginScene");
        }

        private IEnumerator SaveAndQuit()
        {
            var ppm = GetCurrentPlayerPositionManager();
            var playerGo = ppm ? ppm.gameObject : null;
            var stats = playerGo ? playerGo.GetComponent<PlayerStats>() : null;
            var inv = playerGo ? playerGo.GetComponent<InventorySyncManager>() : null;

            if (QuestManager.instance != null) QuestManager.instance.isLoggingOut = true;

            // 1) Lưu VỊ TRÍ trước (đợi hoàn tất)
            if (ppm != null)
                yield return ppm.SavePlayerPositionRoutine();

            // 2) PUT Gold (không block app, nhưng nên gọi)
            if (GoldManager.Instance != null)
                CoroutineRunner.Run(GoldManager.Instance.UpdateCoinsToAPI());

            // 3) Lưu stat (đợi hoàn tất) & GÁN LẠI checkpoint (phòng race)
            if (ppm != null && stats != null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                Vector3 pos = playerGo.transform.position;

                yield return ppm.UpdateProfileAndWait(p =>
                {
                    p.exp = stats.currentExp;
                    p.level = stats.level;
                    p.maxHP = stats.maxHP;
                    p.maxMP = stats.maxMP;
                    p.HP = stats.currentHP;
                    p.MP = stats.currentMP;

                    p.currentCheckpoint = $"{sceneName}:{pos.x},{pos.y},{pos.z}";
                });
            }

            if (inv != null) inv.SaveInventoryToServer();
            yield return new WaitForSecondsRealtime(0.5f);


            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }
}
