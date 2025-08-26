using UnityEngine;

public class FireBreathSweep : MonoBehaviour
{
    public float sweepSpeed = 2f;
    public float sweepDistance = 4f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float progress = 0f;

    void Start()
    {
        startPosition = transform.localPosition;
        targetPosition = startPosition + (Vector3.left * sweepDistance); // sang trái
    }

    void Update()
    {
        progress += Time.deltaTime * sweepSpeed;
        transform.localPosition = Vector3.Lerp(startPosition, targetPosition, progress);
    }
}
