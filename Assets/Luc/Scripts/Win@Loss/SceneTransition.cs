using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneTransition
{
    public static float DefaultFadeDuration = 0.4f;

    private static float _incomingFadeDuration = 0.4f;
    private static bool _pendingFadeIn = false;

    public static void Load(string sceneName, float? fadeDuration = null)
    {
        float dur = fadeDuration ?? DefaultFadeDuration;

        var runnerGO = new GameObject("[TransitionRunner]");
        var runner = runnerGO.AddComponent<TransitionRunner>();
        runner.StartCoroutine(DoTransition(runner, sceneName, dur));
    }

    private static IEnumerator DoTransition(TransitionRunner runner, string sceneName, float fadeDuration)
    {
        var cg = CreateOverlayCanvas(runner.gameObject);
        yield return runner.StartCoroutine(Fade(cg, 0f, 1f, fadeDuration));

        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
            yield return null;

        _incomingFadeDuration = fadeDuration;
        _pendingFadeIn = true;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        op.allowSceneActivation = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (!_pendingFadeIn) return;
        _pendingFadeIn = false;

        var runnerGO = new GameObject("[TransitionRunner_NewScene]");
        var runner = runnerGO.AddComponent<TransitionRunner>();

        var cg = CreateOverlayCanvas(runnerGO);
        cg.alpha = 1f;
        runner.StartCoroutine(FadeAndCleanup(runner, cg, 1f, 0f, _incomingFadeDuration));
    }

    private static CanvasGroup CreateOverlayCanvas(GameObject owner)
    {
        var canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(owner.transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // luôn trên cùng

        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        var img = imageGO.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var cg = canvasGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = true;    
        cg.blocksRaycasts = true;

        return cg;
    }

    private static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null || duration <= 0f) { if (cg) cg.alpha = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private static IEnumerator FadeAndCleanup(TransitionRunner runner, CanvasGroup cg, float from, float to, float duration)
    {
        yield return Fade(cg, from, to, duration);
        if (runner) Object.Destroy(runner.gameObject); 
    }

    private class TransitionRunner : MonoBehaviour { }
}
