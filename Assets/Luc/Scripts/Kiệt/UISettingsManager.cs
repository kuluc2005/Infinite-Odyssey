using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu
{
    public class UISettingsManager : MonoBehaviour
    {
        [Header("GAME SETTINGS")]
        public GameObject showhudtext;
        public GameObject tooltipstext;

        [Header("TOOLTIPS TARGET")]
        public GameObject controlHintsImage;

        [Header("HUD TARGET")]
        public GameObject hudRoot;

        [Header("RESOLUTION DROPDOWN")]
        public TMP_Dropdown resolutionDropdown;

        [Header("MUSIC SLIDER (BGM)")]
        public GameObject musicSlider;

        [Header("MUSIC SLIDER (Combat BGM)")]
        public GameObject combatMusicSlider;

        [Header("SFX SLIDER (NEW)")]
        public GameObject sfxSlider;  


        [Tooltip("Kéo các AudioSource SFX vào đây để chỉnh âm lượng")]
        public AudioSource[] sfxSources;

        // cache
        private Resolution[] availableRes;
        private int currentResIndex = 0;

        void Start()
        {
            // --- BGM ---
            if (musicSlider)
                musicSlider.GetComponent<Slider>().value =
                    PlayerPrefs.GetFloat("MusicVolume", 1f);

            if (combatMusicSlider)
                combatMusicSlider.GetComponent<Slider>().value =
                    PlayerPrefs.GetFloat("MusicCombat", 1f);

            // --- SFX ---
            if (sfxSlider)
            {
                float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.GetComponent<Slider>().value = savedSFX;
                ApplySFXVolume(savedSFX);
            }

            // resolution
            SetupResolutionsDropdown();

            // HUD
            bool hudOn = PlayerPrefs.GetInt("ShowHUD", 1) == 1;
            showhudtext.GetComponent<TMP_Text>().text = hudOn ? "on" : "off";
            if (hudRoot) hudRoot.SetActive(hudOn);

            // tooltips
            bool tooltipsOn = PlayerPrefs.GetInt("ToolTips", 1) == 1;
            tooltipstext.GetComponent<TMP_Text>().text = tooltipsOn ? "on" : "off";
            if (controlHintsImage) controlHintsImage.SetActive(tooltipsOn);
        }

        void SetupResolutionsDropdown()
        {
            if (!resolutionDropdown) return;

            availableRes = Screen.resolutions;
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

            PlayerPrefs.SetInt("ResolutionIndex", currentResIndex);
        }

        // Gọi từ TMP_Dropdown.OnValueChanged(int)
        public void ResolutionDropdownChanged(int index)
        {
            if (availableRes == null || availableRes.Length == 0) return;
            currentResIndex = Mathf.Clamp(index, 0, availableRes.Length - 1);
            PlayerPrefs.SetInt("ResolutionIndex", currentResIndex);

            ApplyResolution(currentResIndex);
        }

        void ApplyResolution(int index)
        {
            var r = availableRes[index];
#if UNITY_2022_2_OR_NEWER
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
#else
            Screen.SetResolution(r.width, r.height, Screen.fullScreen, r.refreshRate);
#endif
        }

        // Gọi từ Slider OnValueChanged(float)
        public void MusicSlider()
        {
            if (!musicSlider) return;
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
        }

        // Gọi từ Slider OnValueChanged(float) (slider combat)
        public void CombatMusicSlider()
        {
            if (!combatMusicSlider) return;
            PlayerPrefs.SetFloat("MusicCombat", combatMusicSlider.GetComponent<Slider>().value);
        }

        // SFX Slider
        public void SFXSlider()
        {
            if (!sfxSlider) return;

            float value = sfxSlider.GetComponent<Slider>().value;
            PlayerPrefs.SetFloat("SFXVolume", value);
            ApplySFXVolume(value);
        }

        private void ApplySFXVolume(float value)
        {
            if (sfxSources == null) return;
            foreach (var src in sfxSources)
            {
                if (src) src.volume = value;
            }
        }

        // HUD & Tooltips
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
    }
}
