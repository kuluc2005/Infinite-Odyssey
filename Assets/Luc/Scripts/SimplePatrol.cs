using UnityEngine;
using UnityEngine.AI;

public class SimplePatrol : MonoBehaviour
{
    public Transform[] patrolPoints; // gan waypoints
    private int currentPointIndex = 0;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;
        if (!agent.isOnNavMesh) return; 

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            // chuyen snang waypoint ke tiep
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }
}
