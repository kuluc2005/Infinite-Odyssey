using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    public TMP_Text nameText;  
    public TMP_Text goldText;   

    void Start()
    {
        if (PlayerPrefs.HasKey("CharacterName"))
        {
            nameText.text = PlayerPrefs.GetString("CharacterName");
        }
        else
        {
            nameText.text = "No Name";
        }
    }

    void Update()
    {
        if (GoldManager.Instance != null)
        {
            goldText.text = "Gold: " + GoldManager.Instance.CurrentGold;
        }
    }
}
