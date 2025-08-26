using UnityEngine;
using TMPro;
using Invector.vCharacterController;

public class HealthStaminaNumberUI : MonoBehaviour
{
    [Header("Tham chiếu đến Invector Controller")]
    public vThirdPersonController controller;  

    [Header("Text hiển thị")]
    public TMP_Text hpText;         
    public TMP_Text hpPercentText;  
    public TMP_Text staminaText;   

    [Header("Định dạng")]
    [Tooltip("{0}=current, {1}=max")]
    public string hpFormat = "{0} / {1}";
    public string staminaFormat = "{0} / {1}";
    public string percentFormat = "{0}%";

    void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<vThirdPersonController>();
    }

    void LateUpdate()
    {
        if (controller == null) return;

        int curHP = Mathf.RoundToInt(controller.currentHealth);
        int maxHP = Mathf.RoundToInt(controller.maxHealth);

        int curSta = Mathf.RoundToInt(controller.currentStamina);
        int maxSta = Mathf.RoundToInt(controller.maxStamina);

        if (hpText != null)
            hpText.text = string.Format(hpFormat, curHP, Mathf.Max(1, maxHP));

        if (hpPercentText != null && maxHP > 0)
        {
            int pct = Mathf.Clamp(Mathf.RoundToInt((curHP * 100f) / maxHP), 0, 100);
            hpPercentText.text = string.Format(percentFormat, pct);
        }

        if (staminaText != null)
            staminaText.text = string.Format(staminaFormat, curSta, Mathf.Max(1, maxSta));
    }
}
