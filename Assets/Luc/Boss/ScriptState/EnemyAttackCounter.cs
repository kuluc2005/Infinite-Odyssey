using UnityEngine;

public class EnemyAttackCounter : MonoBehaviour
{
    public static int basicAttackCount = 0;

    // goi tu Animation Event
    public void CountBasicAttack()
    {
        basicAttackCount++;
        Debug.Log("da danh thuong: " + basicAttackCount);
    }
}
