using UnityEngine;

public class MapPiecePickup : MonoBehaviour
{
    public string itemID = "13";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager.instance.UpdateQuestObjective(ObjectiveType.CollectItem, itemID, 1);
            Destroy(gameObject);
        }
    }
}
