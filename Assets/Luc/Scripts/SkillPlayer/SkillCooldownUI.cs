using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public Image cooldownMask;
    public Image skillIcon;

    [Header("Manager source")]
    public MonoBehaviour skillManager;                 
    public bool autoFindInParent = true;            

    [Header("Which skill")]
    public int skillIndex = 1;                     

    [Header("UX options")]
    public bool requireTargetForSkill3 = false;

    [Header("Extra conditions")]
    public bool requireWeaponForSkill1 = true;

    private PlayerSkillManager pm;
    private PlayerStaffSkillManager ps;

    void Awake()
    {
        // Nếu lỡ gán nhầm controller → bỏ để auto-find
        if (skillManager && !(skillManager is PlayerSkillManager) && !(skillManager is PlayerStaffSkillManager))
            skillManager = null;

        TryBindManager();
    }
    void Start()
    {
        if (pm == null && ps == null) TryBindManager();
    }
    void TryBindManager()
    {
        if (skillManager == null && autoFindInParent)
        {
            ps = GetComponentInParent<PlayerStaffSkillManager>(true);
            pm = GetComponentInParent<PlayerSkillManager>(true);
            if (ps) skillManager = ps; else if (pm) skillManager = pm;
        }
        if (skillManager == null) 
        {
            ps = Object.FindFirstObjectByType<PlayerStaffSkillManager>(FindObjectsInactive.Include);
            pm = Object.FindFirstObjectByType<PlayerSkillManager>(FindObjectsInactive.Include);

            if (ps && ps.isActiveAndEnabled) skillManager = ps;
            else if (pm && pm.isActiveAndEnabled) skillManager = pm;
        }
        pm = skillManager as PlayerSkillManager;
        ps = skillManager as PlayerStaffSkillManager;
    }

    void Update()
    {
        float fillAmount = 0f;
        bool isUsable = false;

        if (pm != null)
        {
            switch (skillIndex)
            {
                case 1:
                    fillAmount = pm.GetSkill1CooldownPercent();
                    isUsable = pm.IsSkill1Ready() && (!requireWeaponForSkill1 || pm.HasWeaponForSkill1());
                    break;
                case 2:
                    fillAmount = pm.GetSkill2CooldownPercent();
                    isUsable = pm.IsSkill2Ready();
                    break;
                case 3:
                    fillAmount = Mathf.Clamp01(pm.skill3CooldownTimer / Mathf.Max(0.0001f, pm.skill3Cooldown));
                    bool ready = pm.skill3CooldownTimer <= 0f;
                    isUsable = requireTargetForSkill3 ? (ready && pm.IsEnemyInSkill3Range()) : ready;
                    break;
            }
        }
        else if (ps != null)
        {
            switch (skillIndex)
            {
                case 1:
                    fillAmount = ps.GetSkill1CooldownPercent();
                    isUsable = ps.IsSkill1Ready() && (!requireWeaponForSkill1 || ps.HasWeaponForSkill1()); 
                    break;
                case 2:
                    fillAmount = ps.GetSkill2CooldownPercent();
                    isUsable = ps.IsSkill2Ready();
                    break;
                case 3:
                    fillAmount = ps.GetSkill3CooldownPercent();   
                    bool ready = ps.IsSkill3Ready();
                    isUsable = requireTargetForSkill3 ? (ready && ps.IsEnemyInSkill3Range()) : ready;
                    break;
            }
        }

        if (cooldownMask) cooldownMask.fillAmount = fillAmount;

        if (skillIcon)
        {
            var c = skillIcon.color;
            c.a = isUsable ? 1f : 0.5f;
            skillIcon.color = c;
        }
    }

    void OnValidate()
    {
        if (cooldownMask && cooldownMask.type != Image.Type.Filled)
            Debug.LogWarning("[SkillCooldownUI] cooldownMask.Image.Type nên để 'Filled' để fillAmount hoạt động.");
    }


}
