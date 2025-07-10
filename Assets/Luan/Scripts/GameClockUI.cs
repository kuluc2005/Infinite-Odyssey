using UnityEngine;
using TMPro;

public class GameClockUI : MonoBehaviour
{
    public DayNightCycle dayCycle; // Gắn script quản lý thời gian
    public TextMeshProUGUI clockText;

    void Update()
    {
        float hour = dayCycle.timeOfDay;
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);
        clockText.text = $"{h:00}:{m:00}";
    }
}
