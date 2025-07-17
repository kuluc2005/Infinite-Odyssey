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

        nameText.text = character.name ?? (character.characterClass + " #" + character.id);
        levelText.text = "Level: " + (character.level > 0 ? character.level.ToString() : "1");

        string spriteName = "Avatar" + character.characterClass;
        Sprite avatarSprite = Resources.Load<Sprite>("ImageL/" + spriteName);

        if (avatarSprite != null)
            avatarImage.sprite = avatarSprite;
        else
            Debug.LogWarning("Không tìm thấy avatar: Resources/ImageL/" + spriteName);

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke(character));
    }

}
