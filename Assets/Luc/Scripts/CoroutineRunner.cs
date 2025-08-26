// CoroutineRunner.cs
using UnityEngine;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("~CoroutineRunner");
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<CoroutineRunner>();
        }
    }

    public static Coroutine Run(IEnumerator routine)
    {
        if (Instance == null) Bootstrap();
        return Instance.StartCoroutine(routine);
    }

    void OnApplicationQuit()
    {
        if (Instance == this)
        {
            Destroy(gameObject);
            Instance = null;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorCleanupHook()
    {
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode && Instance != null)
            {
                Object.DestroyImmediate(Instance.gameObject);
                Instance = null;
            }
        };
    }
#endif
}
