using UnityEngine;

public class ResultOnDead : MonoBehaviour
{
    private bool fired;

    public void OnDead()
    {
        if (fired) return;
        fired = true;

        var cur = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastResult", "Lose");
        PlayerPrefs.SetString("LastLevel", cur);

        GameFlowManager.Lose();
    }
}
