using UnityEngine;

public class Idle : StateMachineBehaviour
{
    float timer;
    Transform player;

    [Header("Config")]
    public float idleToRoamAfter = 1.5f;
    public float chaseRange = 16f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        var go = GameObject.FindGameObjectWithTag("Player");
        player = go ? go.transform : null;

        animator.SetBool("isPatrolling", false);
        animator.SetBool("isAttacking", false);
        // giữ nguyên isChasing
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        if (timer >= idleToRoamAfter)
            animator.SetBool("isPatrolling", true); // → walk_forward

        if (player)
        {
            float d = Vector3.Distance(player.position, animator.transform.position);
            if (d < chaseRange) animator.SetBool("isChasing", true);
        }
    }
}
