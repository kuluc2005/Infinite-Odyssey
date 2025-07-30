using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;
    private bool hasInitialized = false;

    void Update()
    {
        if (GoldManager.Instance != null)
        {
            if (!hasInitialized && ProfileManager.CurrentProfile != null)
            {
                hasInitialized = true;
            }

            if (hasInitialized)
            {
                goldText.text = "Gold: " + GoldManager.Instance.CurrentGold.ToString();
            }
            else
            {
                goldText.text = "Gold: ..."; 
            }
        }
    }
}
