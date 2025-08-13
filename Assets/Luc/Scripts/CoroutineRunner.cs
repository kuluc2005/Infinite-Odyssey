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
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<CoroutineRunner>();
        }
    }

    public static Coroutine Run(IEnumerator routine)
    {
        if (Instance == null) Bootstrap();
        return Instance.StartCoroutine(routine);
    }
}
