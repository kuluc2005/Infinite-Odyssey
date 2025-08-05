using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public Image cooldownMask;        // Hình nền cooldown (xoay)
    public Image skillIcon;           // Icon skill (hiệu ứng xám/sáng)
    public PlayerSkillManager skillManager;
    public int skillIndex = 1;        // 1 = Skill1, 2 = Skill2, 3 = Skill3

    void Update()
    {
        float fillAmount = 0f;
        bool isUsable = false;

        switch (skillIndex)
        {
            case 1:
                fillAmount = skillManager.GetSkill1CooldownPercent();
                isUsable = skillManager.IsSkill1Ready();
                break;

            case 2:
                fillAmount = skillManager.GetSkill2CooldownPercent();
                isUsable = skillManager.IsSkill2Ready();
                break;

            case 3:
                fillAmount = Mathf.Clamp01(skillManager.skill3CooldownTimer / skillManager.skill3Cooldown);
                isUsable = skillManager.skill3CooldownTimer <= 0f && skillManager.IsEnemyInSkill3Range();
                break;
        }

        if (cooldownMask != null)
            cooldownMask.fillAmount = fillAmount;

        if (skillIcon != null)
        {
            Color iconColor = skillIcon.color;
            iconColor.a = isUsable ? 1f : 0.5f;
            skillIcon.color = iconColor;
        }
    }
}
