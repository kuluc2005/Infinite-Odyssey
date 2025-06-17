using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public Transform spawnPoint;

    void Start()
    {
        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter");
        GameObject prefabToSpawn = null;

        if (selectedCharacter == "Male")
        {
            prefabToSpawn = malePrefab;
        }
        else if (selectedCharacter == "Female")
        {
            prefabToSpawn = femalePrefab;
        }
        else
        {
            Debug.LogError("No character selected! Spawning male by default.");
            prefabToSpawn = malePrefab;
        }

        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
    }
}
