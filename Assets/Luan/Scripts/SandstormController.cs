using UnityEngine;

public class SandstormController : MonoBehaviour
{
    public DayNightCycle dayCycle;
    public GameObject sandstormPrefab;
    private GameObject stormInstance;

    public float startHour = 14f;
    public float endHour = 17f;

    void Update()
    {
        float h = dayCycle.timeOfDay;
        bool shouldBeOn = (h >= startHour && h < endHour);

        if (shouldBeOn && stormInstance == null)
        {
            stormInstance = Instantiate(sandstormPrefab);
        }
        else if (!shouldBeOn && stormInstance != null)
        {
            Destroy(stormInstance);
        }
    }
}
