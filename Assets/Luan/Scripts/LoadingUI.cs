using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Slider loadingBar;
    public static LoadingUI instance;
    public TMP_Text levelLabel; // <-- Dùng TextMeshPro nên phải là TMP_Text
    public void SetProgress(float value)
    {
        if (loadingBar != null)
        {
            loadingBar.value = value;
        }
    }

    public void UpdateProgress(float progress)
{
    if (loadingBar != null)
        loadingBar.value = progress;

    if (levelLabel != null)
        levelLabel.text = $"Loading... {(progress * 100f):0}%";
}

}
