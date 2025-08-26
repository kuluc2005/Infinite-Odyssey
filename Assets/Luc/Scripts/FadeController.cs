using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("UI Reference")]
    public Image fadeImage;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f; 

    private void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();
    }

    void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f; 
            fadeImage.color = c;
            StartCoroutine(FadeOut());
        }
        else
        {
            Debug.LogWarning("[FadeController] Không tìm thấy FadeImage!");
        }
    }

    public IEnumerator FadeOut()
    {
        Color c = fadeImage.color;
        float startAlpha = c.a;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalized = t / fadeDuration;
            c.a = Mathf.Lerp(startAlpha, 0, normalized);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;

        gameObject.SetActive(false);
    }
}
