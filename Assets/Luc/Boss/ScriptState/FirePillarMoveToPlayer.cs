using UnityEngine;

public class FirePillarMoveToPlayer : MonoBehaviour
{
    public float moveSpeed = 3f;         // Tốc độ bay về phía player
    public float stopDistance = 0.5f;    // Khoảng cách gần đến player sẽ dừng lại

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player); // nếu muốn hiệu ứng hướng về player
        }
        else
        {
            // Đã đến gần player, có thể gây damage hoặc tự hủy nếu muốn
            // Destroy(gameObject); 
        }
    }
}
