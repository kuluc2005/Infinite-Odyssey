using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class OrbitalFireballs : MonoBehaviour
{
    public Transform targetPlayer;
    public float radius = 2f;
    public float rotateSpeed = 50f;
    public float heightOffset = 1f;
    public float homingSpeed = 5f;
    public int damage = 20;

    [Header("Floating")]
    public float floatAmplitude = 0.3f; 
    public float floatFrequency = 2f;   

    private float baseHeightOffset;
    private float floatTimeOffset; 
    private float angle;
    private bool isFlying = false;
    private Transform targetEnemy;

    // ===================== SFX =====================
    [Header("SFX (Impact)")]
    [Tooltip("AudioClip phát khi orb trúng địch (fallback nếu không dùng owner).")]
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;
    [Tooltip("Ngẫu nhiên pitch ±range để đỡ lặp (fallback).")]
    [Range(0f, 0.5f)] public float impactRandomPitchRange = 0.08f;

    [Tooltip("Nếu set, orb sẽ gọi Owner.SFX_Skill2_Impact() thay vì tự phát clip.")]
    public PlayerSkillManager owner;

    private bool _hit; 

    // ==============================================

    public void Init(Transform player, float startAngle, float radius, float height = 1f)
    {
        targetPlayer = player;
        this.radius = radius;
        heightOffset = height;
        baseHeightOffset = height;
        angle = startAngle;
        floatTimeOffset = Random.Range(0f, Mathf.PI * 2f);
        UpdatePosition();
    }

    /// <summary>
    /// Cho PlayerSkillManager truyền sẵn owner + clip impact.
    /// </summary>
    public void SetupSFX(PlayerSkillManager owner, AudioClip impact)
    {
        this.owner = owner;
        this.impactClip = impact;
    }

    void Update()
    {
        if (isFlying && targetEnemy != null)
        {
            Vector3 dir = (targetEnemy.position + Vector3.up * 1f - transform.position).normalized;
            transform.position += dir * homingSpeed * Time.deltaTime;
        }
        else if (targetPlayer != null)
        {
            angle += rotateSpeed * Time.deltaTime;
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;

        float yOffset = baseHeightOffset + Mathf.Sin(Time.time * floatFrequency + floatTimeOffset) * floatAmplitude;
        offset.y = yOffset;

        transform.position = targetPlayer.position + offset;
    }

    public void FlyToTarget(Transform enemy)
    {
        targetEnemy = enemy;
        isFlying = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hit) return; 

        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Invector.vHealthController>();
            if (health != null)
            {
                var dmg = new Invector.vDamage
                {
                    damageValue = this.damage,
                    sender = transform,
                    hitPosition = transform.position,
                    hitReaction = true
                };
                health.TakeDamage(dmg);
            }

            PlayImpactSFX();
            Destroy(gameObject);
        }
    }

    // ===================== SFX helpers =====================

    private void PlayImpactSFX()
    {
        _hit = true;

        if (owner != null)
        {
            owner.SFX_Skill2_Impact();
            return;
        }

        if (impactClip != null)
        {
            StartCoroutine(PlayClipAtPointVarPitch(impactClip, transform.position, impactVolume, impactRandomPitchRange));
        }
    }

    private IEnumerator PlayClipAtPointVarPitch(AudioClip clip, Vector3 pos, float volume, float pitchRnd)
    {
        GameObject go = new GameObject("SFX_ImpactTemp");
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f; // 3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);

        if (pitchRnd > 0f)
            src.pitch = Mathf.Clamp(1f + Random.Range(-pitchRnd, pitchRnd), 0.5f, 2f);

        src.Play();
        Destroy(go, clip.length + 0.1f);
        yield return null;
    }
}
