using UnityEngine;

public class PlayerStatss: MonoBehaviour
{
    public int currentHealth = 100;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player HP: " + currentHealth);
    }
}
