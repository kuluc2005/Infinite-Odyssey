using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    Transform player;
    BossManager bossManager;
    float attackRange = 3.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        bossManager = animator.GetComponent<BossManager>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || bossManager == null) return;

        animator.transform.LookAt(player);
        float distance = Vector3.Distance(player.position, animator.transform.position);

        if (distance > attackRange)
        {
            animator.SetBool("isAttacking", false);
            return;
        }

        animator.SetBool("isAttacking", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (bossManager != null)
        {
            bossManager.CountBasicAttack();
        }

        animator.SetBool("isAttacking", false);
        animator.ResetTrigger("Attack1");
    }
}
