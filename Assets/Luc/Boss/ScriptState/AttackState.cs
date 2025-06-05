using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    Transform player;
    static int basicAttackCount = 0;
    float attackRange = 3.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        animator.transform.LookAt(player);
        float distance = Vector3.Distance(player.position, animator.transform.position);

        if (distance > attackRange)
        {
            animator.SetBool("isAttacking", false);
            return;
        }

        // neu danh du 3 lan thi dung skill
        if (EnemyAttackCounter.basicAttackCount >= 3)
        {
            animator.SetBool("isAttacking", false);
            animator.SetTrigger("Attack1");
            EnemyAttackCounter.basicAttackCount = 0;
        }
        else
        {
            animator.SetBool("isAttacking", true);
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset khi  thoat ra
        animator.SetBool("isAttacking", false);
        animator.ResetTrigger("Attack1");
        basicAttackCount++;
    }
}
