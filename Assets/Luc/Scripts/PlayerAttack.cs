using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator; 
    public string attackTrigger = "Attack"; // trigger trong animator cua player
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        // goi animation danh thuong 
        if (animator != null)
        {
            animator.SetTrigger(attackTrigger);
        }
    }
}
