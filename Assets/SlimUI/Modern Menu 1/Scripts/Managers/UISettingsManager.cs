using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu
{
    public class UISettingsManager : MonoBehaviour
    {
        [Header("VIDEO SETTINGS")]
        public GameObject fullscreentext;
        public GameObject ambientocclusiontext;
        public GameObject shadowofftextLINE;
        public GameObject shadowlowtextLINE;
        public GameObject shadowhightextLINE;
        public GameObject aaofftextLINE;
        public GameObject aa2xtextLINE;
        public GameObject aa4xtextLINE;
        public GameObject aa8xtextLINE;
        public GameObject vsynctext;
        public GameObject motionblurtext;
        public GameObject texturelowtextLINE;
        public GameObject texturemedtextLINE;
        public GameObject texturehightextLINE;
        public GameObject cameraeffectstext;

        [Header("GAME SETTINGS")]
        public GameObject showhudtext;
        public GameObject tooltipstext;
        public GameObject difficultynormaltextLINE;
        public GameObject difficultyhardcoretextLINE;

        [Header("CONTROLS SETTINGS")]
        public GameObject invertmousetext;

        [Header("TOOLTIPS TARGET")]
        public GameObject controlHintsImage; // Image hoặc root hiển thị tooltip

        [Header("HUD TARGET")]
        public GameObject hudRoot; // Root HUD

        [Header("RESOLUTION DROPDOWN")]
        public TMP_Dropdown resolutionDropdown; // Kéo TMP_Dropdown vào đây

        // sliders
        public GameObject musicSlider;
        public GameObject sensitivityXSlider;
        public GameObject sensitivityYSlider;
        public GameObject mouseSmoothSlider;

        // cache
        private Resolution[] availableRes;
        private int currentResIndex = 0;

        private float sliderValueXSensitivity = 0.0f;
        private float sliderValueYSensitivity = 0.0f;
        private float sliderValueSmoothing = 0.0f;

        void Start()
        {
            // difficulty
            if (PlayerPrefs.GetInt("NormalDifficulty") == 1)
            {
                difficultynormaltextLINE.SetActive(true);
                difficultyhardcoretextLINE.SetActive(false);
            }
            else
            {
                difficultyhardcoretextLINE.SetActive(true);
                difficultynormaltextLINE.SetActive(false);
            }

            // sliders
            musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
            sensitivityXSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("XSensitivity");
            sensitivityYSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("YSensitivity");
            mouseSmoothSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseSmoothing");

            // resolutions → đổ dropdown
            SetupResolutionsDropdown();

            // fullscreen text
            if (fullscreentext) fullscreentext.GetComponent<TMP_Text>().text = Screen.fullScreen ? "on" : "off";

            // HUD
            bool hudOn = PlayerPrefs.GetInt("ShowHUD", 1) == 1;
            showhudtext.GetComponent<TMP_Text>().text = hudOn ? "on" : "off";
            if (hudRoot) hudRoot.SetActive(hudOn);

            // tooltips
            bool tooltipsOn = PlayerPrefs.GetInt("ToolTips", 1) == 1;
            tooltipstext.GetComponent<TMP_Text>().text = tooltipsOn ? "on" : "off";
            if (controlHintsImage) controlHintsImage.SetActive(tooltipsOn);

            // shadows
            int sh = PlayerPrefs.GetInt("Shadows");
            if (sh == 0)
            {
                QualitySettings.shadowCascades = 0;
                QualitySettings.shadowDistance = 0;
                shadowofftextLINE.SetActive(true);
                shadowlowtextLINE.SetActive(false);
                shadowhightextLINE.SetActive(false);
            }
            else if (sh == 1)
            {
                QualitySettings.shadowCascades = 2;
                QualitySettings.shadowDistance = 75;
                shadowofftextLINE.SetActive(false);
                shadowlowtextLINE.SetActive(true);
                shadowhightextLINE.SetActive(false);
            }
            else
            {
                QualitySettings.shadowCascades = 4;
                QualitySettings.shadowDistance = 500;
                shadowofftextLINE.SetActive(false);
                shadowlowtextLINE.SetActive(false);
                shadowhightextLINE.SetActive(true);
            }

            // vsync
            vsynctext.GetComponent<TMP_Text>().text = QualitySettings.vSyncCount == 0 ? "off" : "on";

            // invert mouse
            invertmousetext.GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("Inverted") == 0 ? "off" : "on";

            // motion blur
            motionblurtext.GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("MotionBlur") == 0 ? "off" : "on";

            // ambient occlusion
            ambientocclusiontext.GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("AmbientOcclusion") == 0 ? "off" : "on";

            // texture quality
            int tx = PlayerPrefs.GetInt("Textures");
            if (tx == 0)
            {
                QualitySettings.globalTextureMipmapLimit = 2;
                texturelowtextLINE.SetActive(true);
                texturemedtextLINE.SetActive(false);
                texturehightextLINE.SetActive(false);
            }
            else if (tx == 1)
            {
                QualitySettings.globalTextureMipmapLimit = 1;
                texturelowtextLINE.SetActive(false);
                texturemedtextLINE.SetActive(true);
                texturehightextLINE.SetActive(false);
            }
            else
            {
                QualitySettings.globalTextureMipmapLimit = 0;
                texturelowtextLINE.SetActive(false);
                texturemedtextLINE.SetActive(false);
                texturehightextLINE.SetActive(true);
            }
        }

        void SetupResolutionsDropdown()
        {
            if (!resolutionDropdown) return;

            availableRes = Screen.resolutions; // tự lấy tất cả res máy hỗ trợ
            resolutionDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<string>();
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
            int currentMatch = 0;

            for (int i = 0; i < availableRes.Length; i++)
            {
                var r = availableRes[i];
#if UNITY_2022_2_OR_NEWER
                string label = $"{r.width} x {r.height} @ {r.refreshRateRatio.value:F0}Hz";
#else
                string label = $"{r.width} x {r.height} @ {r.refreshRate}Hz";
#endif
                options.Add(label);

                // khớp với currentResolution để chọn sẵn
                if (r.width == Screen.currentResolution.width &&
                    r.height == Screen.currentResolution.height)
                {
                    currentMatch = i;
                }
            }

            resolutionDropdown.AddOptions(options);

            currentResIndex = (savedIndex >= 0 && savedIndex < availableRes.Length) ? savedIndex : currentMatch;
            resolutionDropdown.SetValueWithoutNotify(currentResIndex);
            resolutionDropdown.RefreshShownValue();

            // lưu trạng thái hiện tại
            PlayerPrefs.SetInt("ResolutionIndex", currentResIndex);
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        }

        void Update()
        {
            sliderValueXSensitivity = sensitivityXSlider.GetComponent<Slider>().value;
            sliderValueYSensitivity = sensitivityYSlider.GetComponent<Slider>().value;
            sliderValueSmoothing = mouseSmoothSlider.GetComponent<Slider>().value;
        }

        // =============== RESOLUTION HANDLERS ===============

        // Gọi từ TMP_Dropdown.OnValueChanged(int)
        public void ResolutionDropdownChanged(int index)
        {
            if (availableRes == null || availableRes.Length == 0) return;
            currentResIndex = Mathf.Clamp(index, 0, availableRes.Length - 1);
            PlayerPrefs.SetInt("ResolutionIndex", currentResIndex);

            bool fs = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            ApplyResolution(currentResIndex, fs);
        }

        void ApplyResolution(int index, bool fullscreen)
        {
            var r = availableRes[index];

#if UNITY_2022_2_OR_NEWER
            // Unity mới: tham số 3 là FullScreenMode + refreshRateRatio
            var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(r.width, r.height, mode, r.refreshRateRatio);
#else
            // Unity cũ: có overload dùng bool + refreshRate (int)
            Screen.SetResolution(r.width, r.height, fullscreen, r.refreshRate);
#endif

            if (fullscreentext) fullscreentext.GetComponent<TMP_Text>().text = fullscreen ? "on" : "off";
        }

        public void FullScreen()
        {
            // Toggle giữ nguyên size theo dropdown
            bool newFS = !Screen.fullScreen;
            PlayerPrefs.SetInt("Fullscreen", newFS ? 1 : 0);

            if (availableRes == null || availableRes.Length == 0)
            {
#if UNITY_2022_2_OR_NEWER
                var modeOnly = newFS ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                // Không đổi size/refresh, chỉ đổi mode:
                Screen.fullScreenMode = modeOnly;
#else
                Screen.fullScreen = newFS; // fallback
#endif
            }
            else
            {
                int idx = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
                idx = Mathf.Clamp(idx, 0, availableRes.Length - 1);
                ApplyResolution(idx, newFS);
            }

            if (fullscreentext) fullscreentext.GetComponent<TMP_Text>().text = newFS ? "on" : "off";
        }

        // =============== OTHER SETTINGS ===============

        public void MusicSlider()
        {
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
        }

        public void SensitivityXSlider()
        {
            PlayerPrefs.SetFloat("XSensitivity", sliderValueXSensitivity);
        }

        public void SensitivityYSlider()
        {
            PlayerPrefs.SetFloat("YSensitivity", sliderValueYSensitivity);
        }

        public void SensitivitySmoothing()
        {
            PlayerPrefs.SetFloat("MouseSmoothing", sliderValueSmoothing);
        }

        public void ShowHUD()
        {
            bool on;
            if (PlayerPrefs.GetInt("ShowHUD", 1) == 0)
            {
                PlayerPrefs.SetInt("ShowHUD", 1);
                showhudtext.GetComponent<TMP_Text>().text = "on";
                on = true;
            }
            else
            {
                PlayerPrefs.SetInt("ShowHUD", 0);
                showhudtext.GetComponent<TMP_Text>().text = "off";
                on = false;
            }
            if (hudRoot) hudRoot.SetActive(on);
        }

        public void ToolTips()
        {
            bool on;
            if (PlayerPrefs.GetInt("ToolTips", 1) == 0)
            {
                PlayerPrefs.SetInt("ToolTips", 1);
                tooltipstext.GetComponent<TMP_Text>().text = "on";
                on = true;
            }
            else
            {
                PlayerPrefs.SetInt("ToolTips", 0);
                tooltipstext.GetComponent<TMP_Text>().text = "off";
                on = false;
            }
            if (controlHintsImage) controlHintsImage.SetActive(on);
        }

        public void NormalDifficulty()
        {
            difficultyhardcoretextLINE.SetActive(false);
            difficultynormaltextLINE.SetActive(true);
            PlayerPrefs.SetInt("NormalDifficulty", 1);
            PlayerPrefs.SetInt("HardCoreDifficulty", 0);
        }

        public void HardcoreDifficulty()
        {
            difficultyhardcoretextLINE.SetActive(true);
            difficultynormaltextLINE.SetActive(false);
            PlayerPrefs.SetInt("NormalDifficulty", 0);
            PlayerPrefs.SetInt("HardCoreDifficulty", 1);
        }

        public void ShadowsOff()
        {
            PlayerPrefs.SetInt("Shadows", 0);
            QualitySettings.shadowCascades = 0;
            QualitySettings.shadowDistance = 0;
            shadowofftextLINE.SetActive(true);
            shadowlowtextLINE.SetActive(false);
            shadowhightextLINE.SetActive(false);
        }

        public void ShadowsLow()
        {
            PlayerPrefs.SetInt("Shadows", 1);
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowDistance = 75;
            shadowofftextLINE.SetActive(false);
            shadowlowtextLINE.SetActive(true);
            shadowhightextLINE.SetActive(false);
        }

        public void ShadowsHigh()
        {
            PlayerPrefs.SetInt("Shadows", 2);
            QualitySettings.shadowCascades = 4;
            QualitySettings.shadowDistance = 500;
            shadowofftextLINE.SetActive(false);
            shadowlowtextLINE.SetActive(false);
            shadowhightextLINE.SetActive(true);
        }

        public void vsync()
        {
            if (QualitySettings.vSyncCount == 0)
            {
                QualitySettings.vSyncCount = 1;
                vsynctext.GetComponent<TMP_Text>().text = "on";
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                vsynctext.GetComponent<TMP_Text>().text = "off";
            }
        }

        public void InvertMouse()
        {
            if (PlayerPrefs.GetInt("Inverted") == 0)
            {
                PlayerPrefs.SetInt("Inverted", 1);
                invertmousetext.GetComponent<TMP_Text>().text = "on";
            }
            else
            {
                PlayerPrefs.SetInt("Inverted", 0);
                invertmousetext.GetComponent<TMP_Text>().text = "off";
            }
        }

        public void MotionBlur()
        {
            if (PlayerPrefs.GetInt("MotionBlur") == 0)
            {
                PlayerPrefs.SetInt("MotionBlur", 1);
                motionblurtext.GetComponent<TMP_Text>().text = "on";
            }
            else
            {
                PlayerPrefs.SetInt("MotionBlur", 0);
                motionblurtext.GetComponent<TMP_Text>().text = "off";
            }
        }

        public void AmbientOcclusion()
        {
            if (PlayerPrefs.GetInt("AmbientOcclusion") == 0)
            {
                PlayerPrefs.SetInt("AmbientOcclusion", 1);
                ambientocclusiontext.GetComponent<TMP_Text>().text = "on";
            }
            else
            {
                PlayerPrefs.SetInt("AmbientOcclusion", 0);
                ambientocclusiontext.GetComponent<TMP_Text>().text = "off";
            }
        }

        public void CameraEffects()
        {
            if (PlayerPrefs.GetInt("CameraEffects") == 0)
            {
                PlayerPrefs.SetInt("CameraEffects", 1);
                cameraeffectstext.GetComponent<TMP_Text>().text = "on";
            }
            else
            {
                PlayerPrefs.SetInt("CameraEffects", 0);
                cameraeffectstext.GetComponent<TMP_Text>().text = "off";
            }
        }

        public void TexturesLow()
        {
            PlayerPrefs.SetInt("Textures", 0);
            QualitySettings.globalTextureMipmapLimit = 2;
            texturelowtextLINE.SetActive(true);
            texturemedtextLINE.SetActive(false);
            texturehightextLINE.SetActive(false);
        }

        public void TexturesMed()
        {
            PlayerPrefs.SetInt("Textures", 1);
            QualitySettings.globalTextureMipmapLimit = 1;
            texturelowtextLINE.SetActive(false);
            texturemedtextLINE.SetActive(true);
            texturehightextLINE.SetActive(false);
        }

        public void TexturesHigh()
        {
            PlayerPrefs.SetInt("Textures", 2);
            QualitySettings.globalTextureMipmapLimit = 0;
            texturelowtextLINE.SetActive(false);
            texturemedtextLINE.SetActive(false);
            texturehightextLINE.SetActive(true);
        }
    }
}
