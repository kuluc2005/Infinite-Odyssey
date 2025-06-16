using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public int totalEnemies = 3;
    private int enemiesKilled = 0;
    public bool questCompleted = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void EnemyKilled()
    {
        enemiesKilled++;

        // Cập nhật hiển thị
        FindObjectOfType<QuestUI>()?.UpdateQuestText();

        if (enemiesKilled >= totalEnemies)
        {
            questCompleted = true;
            Debug.Log("Nhiệm vụ hoàn thành!");
            FindObjectOfType<QuestUI>()?.UpdateQuestText(); // Cập nhật lại để hiện "Thành công"
        }
    }



    public int GetEnemiesKilled() => enemiesKilled;
}
