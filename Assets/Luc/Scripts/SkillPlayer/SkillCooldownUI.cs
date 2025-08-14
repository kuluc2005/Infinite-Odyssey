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
    [Tooltip("1 = Skill1, 2 = Skill2, 3 = Skill3")]
    public int skillIndex = 1;

    [Header("UX options (common)")]
    [Tooltip("Nếu bật, Skill 3 chỉ sáng khi có mục tiêu trong tầm.")]
    public bool requireTargetForSkill3 = false;

    [Header("Extra conditions (common)")]
    [Tooltip("Yêu cầu vũ khí cho Skill 1 (nam dùng Katana, nữ dùng Staff).")]
    public bool requireWeaponForSkill1 = true;

    [Header("Male-only conditions")]
    [Tooltip("NAM: yêu cầu đang cầm Katana cho Skill 3.")]
    public bool requireKatanaForSkill3 = true;

    [Header("Female-only conditions")]
    [Tooltip("NỮ: yêu cầu đang cầm Staff cho Skill 2.")]
    public bool requireStaffForSkill2 = true;
    [Tooltip("NỮ: yêu cầu đang cầm Staff cho Skill 3.")]
    public bool requireStaffForSkill3 = true;

    private PlayerSkillManager pm;          // Nam
    private PlayerStaffSkillManager ps;     // Nữ

    void Awake()
    {
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
#if UNITY_2023_1_OR_NEWER
            ps = Object.FindFirstObjectByType<PlayerStaffSkillManager>(FindObjectsInactive.Include);
            pm = Object.FindFirstObjectByType<PlayerSkillManager>(FindObjectsInactive.Include);
#else
            ps = FindObjectOfType<PlayerStaffSkillManager>();
            pm = FindObjectOfType<PlayerSkillManager>();
#endif
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

        if (pm != null) // === NAM ===
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
                    bool ready3 = pm.skill3CooldownTimer <= 0f;
                    bool weaponOK3 = !requireKatanaForSkill3 || pm.HasWeaponForSkill1(); // dùng check Katana sẵn có
                    bool targetOK3 = !requireTargetForSkill3 || pm.IsEnemyInSkill3Range();
                    isUsable = ready3 && weaponOK3 && targetOK3;
                    break;
            }
        }
        else if (ps != null) // === NỮ ===
        {
            switch (skillIndex)
            {
                case 1:
                    fillAmount = ps.GetSkill1CooldownPercent();
                    isUsable = ps.IsSkill1Ready() && (!requireWeaponForSkill1 || ps.HasWeaponForSkill1());
                    break;

                case 2:
                    fillAmount = ps.GetSkill2CooldownPercent();
                    bool weaponOK2 = !requireStaffForSkill2 || ps.HasWeaponForSkill1();
                    isUsable = ps.IsSkill2Ready() && weaponOK2;
                    break;

                case 3:
                    fillAmount = ps.GetSkill3CooldownPercent();
                    bool ready3 = ps.IsSkill3Ready();
                    bool weaponOK3 = !requireStaffForSkill3 || ps.HasWeaponForSkill1();
                    bool targetOK3 = !requireTargetForSkill3 || ps.IsEnemyInSkill3Range();
                    isUsable = ready3 && weaponOK3 && targetOK3;
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
