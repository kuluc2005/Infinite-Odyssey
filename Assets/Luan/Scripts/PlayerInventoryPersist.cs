using UnityEngine;

public class PlayerInventoryPersist : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
