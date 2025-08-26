using UnityEngine;
using UnityEngine.AI;

public class Roam : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;
    EnemyRoamBounds bounds;

    public float waitAtPoint = 0.6f;
    public float speed = 3.2f;
    public float chaseRange = 12f;
    float waitTimer = 0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponentInParent<NavMeshAgent>();
        var go = GameObject.FindGameObjectWithTag("Player");
        player = go ? go.transform : null;
        bounds = animator.GetComponentInParent<EnemyRoamBounds>();

        if (agent == null || !agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.speed = speed;
        agent.stoppingDistance = 0f;

        if (bounds) agent.SetDestination(bounds.GetRandomPointOnNav());
        waitTimer = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPoint)
            {
                if (bounds) agent.SetDestination(bounds.GetRandomPointOnNav());
                waitTimer = 0f;
            }
        }

        if (player)
        {
            float d = Vector3.Distance(player.position, animator.transform.position);
            if (d < chaseRange) animator.SetBool("isChasing", true);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent && agent.isOnNavMesh) agent.ResetPath();
        animator.SetBool("isPatrolling", false);
    }
}
