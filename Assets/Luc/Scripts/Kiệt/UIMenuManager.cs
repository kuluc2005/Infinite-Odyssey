using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu
{
    public class UIMenuManager : MonoBehaviour
    {
        // Animator cũ dùng cho Position1/2 đã bỏ — giữ lại nếu bạn vẫn dùng ở nơi khác
        private Animator CameraObject;

        [Header("MENUS")]
        public GameObject mainMenu;
        public GameObject firstMenu;
        public GameObject exitMenu;

        [Header("PAUSE / SETTINGS")]
        public GameObject pauseCanvas;      // Trùng với PauseManager.pauseCanvas
        public GameObject settingsCanvas;   // Canvas settings

        [Header("Refs")]
        public PauseManager pauseManager;   // Kéo PauseManager vào đây trong Inspector

        public enum Theme { custom1, custom2, custom3 };
        [Header("THEME SETTINGS")]
        public Theme theme;
        private int themeIndex;
        public ThemedUIData themeController;

        [Header("PANELS")]
        public GameObject mainCanvas;
        public GameObject PanelControls;
        public GameObject PanelVideo;
        public GameObject PanelGame;
        public GameObject PanelKeyBindings;
        public GameObject PanelMovement;
        public GameObject PanelCombat;
        public GameObject PanelGeneral;

        [Header("SETTINGS SCREEN")]
        public GameObject lineGame;
        public GameObject lineVideo;
        public GameObject lineControls;
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

        // ====== NÚT TRONG PAUSE MENU ======

        // Continue: đóng pause & tiếp tục game
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

        // Settings: mở settings, vẫn giữ trạng thái pause
        public void OpenSettings()
        {
            if (pauseCanvas) pauseCanvas.SetActive(false);
            if (settingsCanvas) settingsCanvas.SetActive(true);
            // Giữ Time.timeScale = 0 khi đang pause (do PauseManager đã dừng)
        }

        // Return: quay lại pause menu (vẫn pause) — giống CloseSettings
        public void Return()
        {
            if (settingsCanvas) settingsCanvas.SetActive(false);
            if (pauseCanvas) pauseCanvas.SetActive(true);
        }

        // Close Settings: alias của Return cho tiện gắn nút
        public void CloseSettings() => Return();

        // Exit: mở panel xác nhận thoát, ẩn các panel khác (vẫn pause)
        public void ShowExitMenu()
        {
            if (pauseCanvas) pauseCanvas.SetActive(false);
            if (settingsCanvas) settingsCanvas.SetActive(false);
            if (exitMenu) exitMenu.SetActive(true);
        }

        // Back từ Exit về Pause (không resume)
        public void BackFromExitToPause()
        {
            if (exitMenu) exitMenu.SetActive(false);
            if (pauseCanvas) pauseCanvas.SetActive(true);
        }

        // ====== PHẦN CÒN LẠI (nếu bạn vẫn dùng main menu / panels) ======

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

        // ĐÃ GỠ Position1/Position2

        void DisablePanels()
        {
            if (PanelControls) PanelControls.SetActive(false);
            if (PanelVideo) PanelVideo.SetActive(false);
            if (PanelGame) PanelGame.SetActive(false);
            if (PanelKeyBindings) PanelKeyBindings.SetActive(false);

            if (lineGame) lineGame.SetActive(false);
            if (lineControls) lineControls.SetActive(false);
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
        public void ControlsPanel() { DisablePanels(); if (PanelControls) { PanelControls.SetActive(true); if (lineControls) lineControls.SetActive(true); } }
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
    }
}