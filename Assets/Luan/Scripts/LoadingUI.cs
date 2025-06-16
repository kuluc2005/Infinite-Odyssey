using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Slider loadingBar;

    public void SetProgress(float value)
    {
        if (loadingBar != null)
        {
            loadingBar.value = value;
        }
    }
}
