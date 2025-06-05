using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    public void SelectMale()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Male");
        SceneManager.LoadScene("SceneMain");
    }

    public void SelectFemale()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Female");
        SceneManager.LoadScene("SceneMain");
    }
}
