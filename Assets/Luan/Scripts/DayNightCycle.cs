using UnityEngine;
public class DayNightCycle : MonoBehaviour
{
    [Range(0f, 24f)]
    public float timeOfDay = 12f;
    public float realSecondsPerGameHour = 60f;
    [HideInInspector] public float dayProgress;

    void Update()
    {
        timeOfDay += Time.deltaTime / realSecondsPerGameHour;
        if (timeOfDay >= 24f) timeOfDay -= 24f;
        dayProgress = timeOfDay / 24f;
    }
}
