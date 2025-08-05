using UnityEngine;
using Invector.vCharacterController.AI;

public class EnemyDeathHandler : MonoBehaviour
{
    private vSimpleMeleeAI_Controller ai;

    void Start()
    {
        ai = GetComponent<vSimpleMeleeAI_Controller>();
        if (ai != null)
        {
            ai.onDead.AddListener(OnDeadEvent);
        }
    }

    void OnDeadEvent(GameObject enemy)
    {
        Destroy(enemy, 1.5f); 
    }
}
