using UnityEngine;

public class EnemyDeathTrigger : MonoBehaviour
{
    public Animator animator;

    public void TriggerDeath()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
    }
}
