using UnityEngine;

public class QuestResetTrigger : MonoBehaviour
{
    void Start()
    {
        QuestManager.instance?.ResetAllQuests();
    }
}
