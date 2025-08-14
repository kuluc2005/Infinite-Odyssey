using UnityEngine;
using UnityEngine.AI;

public class Run : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float speed = 5.8f;
    public float attackRange = 3.2f;
    public float chaseStopRange = 28f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponentInParent<NavMeshAgent>();
        var go = GameObject.FindGameObjectWithTag("Player");
        player = go ? go.transform : null;

        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.speed = speed;
        agent.stoppingDistance = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player || agent == null || !agent.isOnNavMesh) return;

        // Kiểm tra player còn trong vùng spawn/roam không
        var spawner = animator.GetComponentInParent<EnemyZoneRegister>()?.spawner;
        if (spawner != null && !spawner.IsPointInsideBounds(player.position))
        {
            animator.SetBool("isChasing", false);
            return; // dừng chase
        }

        agent.SetDestination(player.position);

        float distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance < attackRange) animator.SetBool("isAttacking", true);
        if (distance > chaseStopRange) animator.SetBool("isChasing", false);
    }

}
