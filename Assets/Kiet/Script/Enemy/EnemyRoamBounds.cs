using UnityEngine;
using UnityEngine.AI;

public class EnemyRoamBounds : MonoBehaviour
{
    [Header("Tâm vùng roam (đặt = Spawner)")]
    public Transform center;

    [Header("Phạm vi theo X/Z (tính từ tâm)")]
    public Vector2 xRange = new Vector2(-8, 8);
    public Vector2 zRange = new Vector2(-8, 8);

    public float sampleRadius = 2f;

    Vector3 spawnCenter;

    void Awake()
    {
        spawnCenter = center ? center.position : transform.position;
    }

    public Vector3 GetRandomPointOnNav()
    {
        for (int i = 0; i < 25; i++)
        {
            float x = Random.Range(xRange.x, xRange.y);
            float z = Random.Range(zRange.x, zRange.y);
            Vector3 candidate = spawnCenter + new Vector3(x, 0, z);

            if (NavMesh.SamplePosition(candidate, out var hit, sampleRadius * 2.5f, NavMesh.AllAreas))
                return hit.position;
        }

        var tri = NavMesh.CalculateTriangulation();
        if (tri.vertices != null && tri.vertices.Length > 0)
            return tri.vertices[Random.Range(0, tri.vertices.Length)];

        return transform.position;
    }

    // KHÔNG vẽ Gizmos ở đây để tránh 2 vùng chồng nhau
    // (Spawner đã vẽ vùng duy nhất rồi)
}
