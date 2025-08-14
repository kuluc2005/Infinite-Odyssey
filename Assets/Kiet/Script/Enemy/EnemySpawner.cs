using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemySpawner1 : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Prefab phải có Animator (gắn các StateMachineBehaviour)")]
    public GameObject enemyPrefab;

    [Range(1, 500)]
    public int maxEnemyCount = 10;

    [Range(0, 500)]
    public int initialEnemyCount = 10;

    [Header("Respawn (tùy chọn)")]
    public List<Transform> spawnPoints;
    public bool useSpawnPointsForRespawn = true;   // nếu true, respawn ưu tiên spawnPoints (nhưng vẫn kẹp vào vùng)

    [Header("VÙNG DUY NHẤT (spawn + roam)")]
    [Tooltip("Nếu bật, dùng min/max theo toạ độ THẾ GIỚI; nếu tắt, dùng khoảng lệch so với vị trí Spawner")]
    public bool useWorldSpaceBounds = true;

    // World-space bounds (min..max). Dùng khi useWorldSpaceBounds = true
    public Vector2 worldX = new Vector2(-24.3f, -5.32f);
    public Vector2 worldZ = new Vector2(-76.6f, -66f);

    // Offset tính từ Spawner. Dùng khi useWorldSpaceBounds = false
    public Vector2 xRange = new Vector2(-12, 12);
    public Vector2 zRange = new Vector2(-12, 12);

    [Header("NavMesh Sampling")]
    [Tooltip("Khoảng tìm NavMesh quanh điểm yêu cầu. Nên nhỏ (1–2) để tránh kéo ra ngoài vùng.")]
    public float navSampleRadius = 1.5f;

    [Tooltip("Số lần thử tối đa để tìm điểm hợp lệ trong vùng")]
    public int maxTries = 30;

    readonly List<GameObject> currentEnemies = new();

    // --- Helpers: lấy min/max theo chế độ ---
    float MinX => useWorldSpaceBounds ? Mathf.Min(worldX.x, worldX.y) : transform.position.x + Mathf.Min(xRange.x, xRange.y);
    float MaxX => useWorldSpaceBounds ? Mathf.Max(worldX.x, worldX.y) : transform.position.x + Mathf.Max(xRange.x, xRange.y);
    float MinZ => useWorldSpaceBounds ? Mathf.Min(worldZ.x, worldZ.y) : transform.position.z + Mathf.Min(zRange.x, zRange.y);
    float MaxZ => useWorldSpaceBounds ? Mathf.Max(worldZ.x, worldZ.y) : transform.position.z + Mathf.Max(zRange.x, zRange.y);

    void Start()
    {
        initialEnemyCount = Mathf.Clamp(initialEnemyCount, 0, maxEnemyCount);
        for (int i = 0; i < initialEnemyCount; i++)
        {
            Vector3 pos = GetRandomNavPointInBounds();
            SpawnEnemyAt(pos);
        }
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        currentEnemies.Remove(enemy);
        if (currentEnemies.Count < maxEnemyCount)
            SpawnEnemyAtSpawnPoint();
    }

    void SpawnEnemyAtSpawnPoint()
    {
        Vector3 pos;
        if (useSpawnPointsForRespawn && spawnPoints != null && spawnPoints.Count > 0)
        {
            pos = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            // kẹp vào vùng:
            pos = ClampToBounds(pos);
            // snap vào NavMesh nếu có:
            if (NavMesh.SamplePosition(pos, out var hit, navSampleRadius, NavMesh.AllAreas)) pos = hit.position;
        }
        else
        {
            pos = GetRandomNavPointInBounds();
        }

        SpawnEnemyAt(pos);
    }

    void SpawnEnemyAt(Vector3 position)
    {
        if (!enemyPrefab) return;

        // Snap lần cuối thật ngắn để không kéo ra khỏi vùng
        if (NavMesh.SamplePosition(position, out var hit, navSampleRadius, NavMesh.AllAreas))
            position = hit.position;

        // Xoay ngẫu nhiên để nhìn tự nhiên
        var enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        currentEnemies.Add(enemy);

        // Ensure NavMeshAgent
        var agent = enemy.GetComponent<NavMeshAgent>() ?? enemy.AddComponent<NavMeshAgent>();
        agent.stoppingDistance = 1.6f;
        agent.angularSpeed = 720f;
        agent.acceleration = 24f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 70);

        // Gán vùng roam giống y vùng spawn
        var roam = enemy.GetComponent<EnemyRoamBounds>() ?? enemy.AddComponent<EnemyRoamBounds>();
        roam.center = transform;
        // chuyển world-bounds về offset theo Spawner để roam dùng đúng
        roam.xRange = new Vector2(MinX - transform.position.x, MaxX - transform.position.x);
        roam.zRange = new Vector2(MinZ - transform.position.z, MaxZ - transform.position.z);
        roam.sampleRadius = navSampleRadius;

        // Đăng ký respawn khi chết
        var reg = enemy.GetComponent<EnemyZoneRegister>() ?? enemy.AddComponent<EnemyZoneRegister>();
        reg.spawner = this;
    }

    // --- Core: tìm điểm NavMesh bên TRONG vùng, không kéo ra ngoài ---
    Vector3 GetRandomNavPointInBounds()
    {
        for (int i = 0; i < maxTries; i++)
        {
            // chọn điểm mục tiêu bên trong hình chữ nhật
            float rx = Random.Range(MinX, MaxX);
            float rz = Random.Range(MinZ, MaxZ);
            Vector3 request = new Vector3(rx, transform.position.y, rz);

            // Snap rất gần; nếu snap ra ngoài vùng thì bỏ, thử lại
            if (NavMesh.SamplePosition(request, out var hit, navSampleRadius, NavMesh.AllAreas))
            {
                if (IsInsideBounds(hit.position)) return hit.position;
            }
        }

        // Fallback: trả về tâm vùng (nhưng vẫn bên trong)
        return GetBoundsCenter();
    }

    Vector3 GetBoundsCenter()
    {
        float cx = (MinX + MaxX) * 0.5f;
        float cz = (MinZ + MaxZ) * 0.5f;
        return new Vector3(cx, transform.position.y, cz);
    }

    Vector3 ClampToBounds(Vector3 p)
    {
        p.x = Mathf.Clamp(p.x, MinX, MaxX);
        p.z = Mathf.Clamp(p.z, MinZ, MaxZ);
        return p;
    }

    bool IsInsideBounds(Vector3 p)
    {
        return (p.x >= MinX && p.x <= MaxX && p.z >= MinZ && p.z <= MaxZ);
    }


    public bool IsPointInsideBounds(Vector3 point)
    {
        return (point.x >= MinX && point.x <= MaxX &&
                point.z >= MinZ && point.z <= MaxZ);
    }

    void OnDrawGizmosSelected()
    {
        // VẼ VÙNG DUY NHẤT theo chế độ
        Vector3 center = GetBoundsCenter();
        float sx = Mathf.Abs(MaxX - MinX);
        float sz = Mathf.Abs(MaxZ - MinZ);

        Gizmos.color = new Color(1f, .5f, 0f, .12f);
        Gizmos.DrawCube(center + Vector3.up * 0.05f, new Vector3(sx, .1f, sz));
        Gizmos.color = new Color(1f, .5f, 0f, 1f);
        Gizmos.DrawWireCube(center + Vector3.up * 0.05f, new Vector3(sx, .1f, sz));
    }
}
