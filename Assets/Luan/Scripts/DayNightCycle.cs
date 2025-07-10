using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public Gradient lightColor;      // màu sáng từ sáng -> tối -> sáng
    public AnimationCurve lightIntensity;  // cường độ sáng theo thời gian

    [Range(0f, 24f)]
    public float timeOfDay = 12f;
    public float realSecondsPerGameHour = 60f;
    [HideInInspector] public float dayProgress;

    void Update()
    {
        timeOfDay += Time.deltaTime / realSecondsPerGameHour;
        if (timeOfDay >= 24f) timeOfDay -= 24f;
        dayProgress = timeOfDay / 24f;

        UpdateLighting();
    }

    void UpdateLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.color = lightColor.Evaluate(dayProgress);
            directionalLight.intensity = lightIntensity.Evaluate(dayProgress);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((dayProgress * 360f) - 90f, 170f, 0));
        }
    }

}
