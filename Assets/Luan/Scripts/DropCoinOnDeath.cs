using UnityEngine;

public class DropCoinOnDeath : MonoBehaviour
{
    public GameObject coinPrefab; // Prefab đã setup như bạn có
    public Vector3 spawnOffset = new Vector3(0, 0.1f, 0); // Cho coin hơi lơ lửng

    public void DropCoin()
    {
        Instantiate(coinPrefab, transform.position + spawnOffset, Quaternion.identity);
    }
}
