using UnityEngine;

public class StayInBounds3D : MonoBehaviour
{
    [Header("Giới hạn vị trí 3D")]
    public Vector2 xRange = new Vector2(-5f, 5f);
    public Vector2 yRange = new Vector2(0f, 0f);
    public Vector2 zRange = new Vector2(-5f, 5f);

    void LateUpdate()
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, xRange.x, xRange.y);
        p.y = Mathf.Clamp(p.y, yRange.x, yRange.y);
        p.z = Mathf.Clamp(p.z, zRange.x, zRange.y);
        transform.position = p;
    }
}
