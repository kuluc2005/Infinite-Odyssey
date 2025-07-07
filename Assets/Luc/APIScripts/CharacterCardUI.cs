using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterCardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public Image avatarImage;
    public Button selectButton;

    private PlayerCharacter character;
    private System.Action<PlayerCharacter> onSelect;

    public void Setup(PlayerCharacter character, System.Action<PlayerCharacter> onSelect)
    {
        this.character = character;
        this.onSelect = onSelect;
        Debug.Log("Name nhân vật từ API: " + character.name);

        nameText.text = character.name ?? (character.characterClass + " #" + character.id);
        levelText.text = "Level: " + (character.level > 0 ? character.level.ToString() : "1");

        // Cách gán avatar theo class
        avatarImage.sprite = (character.characterClass == "Male")
            ? Resources.Load<Sprite>("AvatarMale")
            : Resources.Load<Sprite>("AvatarFemale");

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke(character));
    }
}
