using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentHealth = 100;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player HP: " + currentHealth);
    }
}
