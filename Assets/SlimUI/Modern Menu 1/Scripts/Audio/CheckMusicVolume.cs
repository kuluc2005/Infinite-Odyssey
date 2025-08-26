using UnityEngine;

namespace SlimUI.ModernMenu
{
    [RequireComponent(typeof(AudioSource))]
    public class CheckMusicVolume : MonoBehaviour
    {
        AudioSource audioSrc;

        void Awake()
        {
            audioSrc = GetComponent<AudioSource>();
            if (!PlayerPrefs.HasKey("MusicVolume"))
                PlayerPrefs.SetFloat("MusicVolume", 1f);

            audioSrc.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }

        void Start()
        {
            if (!audioSrc.isPlaying) audioSrc.Play();
        }

        public void UpdateVolume()
        {
            audioSrc.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
    }
}
