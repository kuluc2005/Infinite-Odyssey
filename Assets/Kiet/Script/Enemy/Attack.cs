using UnityEngine;
using UnityEngine.AI;

public class Attack : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;
    public float disengageRange = 3.8f; // ra xa thì thoát attack

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        player = go ? go.transform : null;
        agent = animator.GetComponentInParent<NavMeshAgent>();

        if (agent && agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }
        animator.SetBool("isAttacking", true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player) return;

        var spawner = animator.GetComponentInParent<EnemyZoneRegister>()?.spawner;
        if (spawner != null && !spawner.IsPointInsideBounds(player.position))
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isChasing", false);
            return;
        }

        // Xoay mặt về phía player
        Vector3 dir = player.position - animator.transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            var rot = Quaternion.LookRotation(dir);
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, rot, 12f * Time.deltaTime);
        }

        if (Vector3.Distance(player.position, animator.transform.position) > disengageRange)
            animator.SetBool("isAttacking", false);
    }

}
