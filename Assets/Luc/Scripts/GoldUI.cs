using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;

    void OnEnable()
    {
        TryRefresh();
    }

    void Update()
    {
        TryRefresh();
    }

    void TryRefresh()
    {
        if (goldText == null) return;
        if (GoldManager.Instance == null) return;
        goldText.text = "Gold: " + GoldManager.Instance.CurrentGold.ToString();
    }
}
