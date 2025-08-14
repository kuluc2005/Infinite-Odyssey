using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Invector;

public class EnemyUIController : MonoBehaviour
{
    [Header("Thông tin Enemy")]
    public string enemyName = "Nightmare Dragon";
    public float detectionRadius = 12f;

    [Header("HUD góc phải")]
    public GameObject enemyHUD;
    public Slider healthSlider;
    public Slider subHealthSlider;
    public Text nameText;
    public Text damageDisplayText;

    [Header("Tùy chỉnh HUD")]
    [Tooltip("Tốc độ tụt của thanh phụ (điểm máu/giây)")]
    public float smoothDamageDelay = 60f;      
    public float damageCounterTimer = 1.5f;
    public bool showDamageType = true;

    [HideInInspector] public GameObject hudInstance;

    private Transform player;
    private bool isUIActive = false;
    private bool inDelay = false;
    private vHealthController healthControl;

    void Start()
    {
        if (EnemyHUDManager.Instance != null && EnemyHUDManager.Instance.enemyHUDPrefab != null)
        {
            enemyHUD = Instantiate(EnemyHUDManager.Instance.enemyHUDPrefab, EnemyHUDManager.Instance.hudParent);
            hudInstance = enemyHUD;
            enemyHUD.SetActive(false);

            // Gán lại component sau khi clone
            healthSlider = enemyHUD.transform.Find("healthBar").GetComponent<Slider>();
            subHealthSlider = enemyHUD.transform.Find("subhealth").GetComponent<Slider>();
            nameText = enemyHUD.transform.Find("NameText").GetComponent<Text>();
            damageDisplayText = enemyHUD.transform.Find("damageDisplay").GetComponent<Text>();


        }

        StartCoroutine(FindPlayer());

        healthControl = GetComponent<vHealthController>();
        if (healthControl != null)
            healthControl.onReceiveDamage.AddListener(OnReceiveDamageFromInvector);

        SetupSliders();

        if (nameText != null) nameText.text = enemyName;
        if (damageDisplayText != null) damageDisplayText.text = "";
    }

    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            var obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SetupSliders()
    {
        if (healthControl == null) return;

        if (healthSlider != null)
        {
            healthSlider.maxValue = healthControl.maxHealth;
            healthSlider.value = healthControl.currentHealth;
        }
        if (subHealthSlider != null)
        {
            subHealthSlider.maxValue = healthControl.maxHealth;
            subHealthSlider.value = healthControl.currentHealth;
        }
    }

    void Update()
    {
        if (player == null || healthControl == null) return;

        if (isUIActive && healthSlider != null)
            healthSlider.value = Mathf.Lerp(healthSlider.value, healthControl.currentHealth, Time.deltaTime * 10f);

        float dist = Vector3.Distance(player.position, transform.position);

        if (!isUIActive && dist <= detectionRadius)
            ShowHUD(true);   
        else if (isUIActive && dist > detectionRadius + 3f)
            HideHUD();
    }

    void ShowHUD(bool initSliders = false)
    {
        isUIActive = true;
        if (enemyHUD != null) enemyHUD.SetActive(true);
        if (initSliders) SetupSliders();
    }

    void HideHUD()
    {
        isUIActive = false;
        if (enemyHUD != null) enemyHUD.SetActive(false);
    }

    private void OnReceiveDamageFromInvector(vDamage damage)
    {
        ShowHUD(false);

        // Hiện số damage
        if (damageDisplayText != null)
        {
            damageDisplayText.text = $"-{Mathf.RoundToInt(damage.damageValue)}" +
                (showDamageType && !string.IsNullOrEmpty(damage.damageType) ? $" ({damage.damageType})" : "");
        }

        if (healthSlider != null && healthControl != null)
            healthSlider.value = healthControl.currentHealth;

        if (!inDelay)
            StartCoroutine(DamageDelay());
    }

    IEnumerator DamageDelay()
    {
        inDelay = true;

        ShowHUD(false);

        while (subHealthSlider != null && subHealthSlider.value > healthControl.currentHealth)
        {
            subHealthSlider.value = Mathf.MoveTowards(
                subHealthSlider.value,
                healthControl.currentHealth,
                smoothDamageDelay * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(damageCounterTimer);

        if (damageDisplayText != null)
            damageDisplayText.text = "";

        inDelay = false;
    }

    void OnDestroy()
    {
        if (healthControl != null)
            healthControl.onReceiveDamage.RemoveListener(OnReceiveDamageFromInvector);

        if (enemyHUD != null)
            Destroy(enemyHUD);
    }
}
