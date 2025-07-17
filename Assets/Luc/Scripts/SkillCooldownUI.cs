using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public Image cooldownMask; // Nền xanh
    public PlayerSkillManager skillManager;
    public int skillIndex = 1; 

    void Update()
    {
        float fillAmount = 0f;
        switch (skillIndex)
        {
            case 1:
                fillAmount = skillManager.GetSkill1CooldownPercent();
                break;
            case 2:
                fillAmount = skillManager.GetSkill2CooldownPercent();
                break;
            case 3:
                fillAmount = Mathf.Clamp01(skillManager.skill3CooldownTimer / skillManager.skill3Cooldown);
                break;
        }

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = fillAmount; // 1 → 0 khi hồi chiêu
        }
    }
}
