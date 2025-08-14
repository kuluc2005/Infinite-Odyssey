using UnityEngine;
using System.Collections.Generic;
using Invector.vCharacterController.AI;   // <- để bắt event onDead

public class EnemySpawner1 : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;             // Prefab enemy
    public int maxEnemyCount = 10;             // Giới hạn tối đa
    public int initialEnemyCount = 10;         // Số lượng spawn ban đầu (nên = max)

    [Header("Spawn Points (chỉ dùng cho respawn)")]
    public List<Transform> spawnPoints;        // Điểm spawn cho enemy đã chết

    [Header("Vùng spawn ban đầu & vùng tồn tại (3D)")]
    public Vector2 xRange = new Vector2(-5f, 5f);
    public Vector2 yRange = new Vector2(0f, 0f);
    public Vector2 zRange = new Vector2(-5f, 5f);

    private readonly List<GameObject> currentEnemies = new List<GameObject>();

    void Start()
    {
        initialEnemyCount = Mathf.Clamp(initialEnemyCount, 0, maxEnemyCount);
        SpawnInitialEnemiesInVolume();
    }

    // --- Spawn N con đầu ngẫu nhiên trong vùng 3D ---
    void SpawnInitialEnemiesInVolume()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("⚠️ Chưa gán EnemyPrefab!");
            return;
        }

        for (int i = 0; i < initialEnemyCount; i++)
        {
            if (currentEnemies.Count >= maxEnemyCount) break;
            Vector3 pos = GetRandomPositionInVolume();
            SpawnEnemyAt(pos);
        }
    }

    // --- Respawn: chỉ dùng spawn point ---
    void SpawnEnemyAtSpawnPoint()
    {
        PruneNulls();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("⚠️ Chưa gán EnemyPrefab!");
            return;
        }
        if (currentEnemies.Count >= maxEnemyCount) return; // đủ số lượng

        Vector3 pos;
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Count)];
            pos = p.position;
        }
        else
        {
            // Fallback: nếu chưa cấu hình spawn point, respawn trong vùng
            pos = GetRandomPositionInVolume();
            Debug.LogWarning("ℹ️ Chưa có SpawnPoints, tạm respawn trong vùng.");
        }

        SpawnEnemyAt(pos);
    }

    // --- Tạo enemy + gắn limiter + bắt event chết từ Invector ---
    void SpawnEnemyAt(Vector3 position)
    {
        PruneNulls();
        if (currentEnemies.Count >= maxEnemyCount) return;

        GameObject e = Instantiate(enemyPrefab, position, Quaternion.identity);
        currentEnemies.Add(e);

        // Bắt event onDead từ Invector để biết khi nào enemy chết
        var ai = e.GetComponent<vSimpleMeleeAI_Controller>();
        if (ai != null)
        {
            // Đăng ký một listener chung cho tất cả enemy
            ai.onDead.AddListener(HandleEnemyDeadEvent);
        }
        else
        {
            Debug.LogWarning("⚠️ Enemy prefab không có vSimpleMeleeAI_Controller. Không thể respawn bằng onDead.");
        }

        // Giữ enemy trong vùng
        var limiter = e.GetComponent<StayInBounds3D>();
        if (limiter == null) limiter = e.AddComponent<StayInBounds3D>();
        limiter.xRange = xRange;
        limiter.yRange = yRange;
        limiter.zRange = zRange;
    }

    // Listener dùng chung cho tất cả enemy Invector
    // Invector gọi với tham số là GameObject vừa chết
    void HandleEnemyDeadEvent(GameObject enemyGO)
    {
        // Xóa khỏi list nếu còn trong list
        if (enemyGO != null && currentEnemies.Contains(enemyGO))
            currentEnemies.Remove(enemyGO);

        // Respawn bù tại spawn point (giữ tối đa 10 con)
        SpawnEnemyAtSpawnPoint();
    }

    // Public cho nơi khác cần gọi thẳng (không bắt buộc dùng)
    public void OnEnemyKilled(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy))
            currentEnemies.Remove(enemy);
        SpawnEnemyAtSpawnPoint();
    }

    Vector3 GetRandomPositionInVolume()
    {
        float x = Random.Range(xRange.x, xRange.y);
        float y = Random.Range(yRange.x, yRange.y);
        float z = Random.Range(zRange.x, zRange.y);
        return new Vector3(x, y, z);
    }

    // Loại bỏ các phần tử null khỏi currentEnemies (bị Destroy ở nơi khác)
    void PruneNulls()
    {
        for (int i = currentEnemies.Count - 1; i >= 0; i--)
        {
            if (currentEnemies[i] == null)
                currentEnemies.RemoveAt(i);
        }
    }

    // Vẽ hộp vùng 3D
    void OnDrawGizmosSelected()
    {
        float cx = (xRange.x + xRange.y) * 0.5f;
        float cy = (yRange.x + yRange.y) * 0.5f;
        float cz = (zRange.x + zRange.y) * 0.5f;

        float sx = Mathf.Abs(xRange.y - xRange.x);
        float sy = Mathf.Abs(yRange.y - yRange.x);
        float sz = Mathf.Abs(zRange.y - zRange.x);

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.12f);
        Gizmos.DrawCube(new Vector3(cx, cy, cz), new Vector3(sx, sy, sz));
        Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
        Gizmos.DrawWireCube(new Vector3(cx, cy, cz), new Vector3(sx, sy, sz));
    }
}
