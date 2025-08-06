using UnityEngine;

public class Skill5Effect : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 90f;

    void Update()
    {
        // Ví dụ: bay tới trước và xoay
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
