using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource normalBGM;
    public AudioSource battleBGM;

    [Header("Detect")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";     
    public LayerMask enemyLayers = ~0;     
    public float checkRadius = 18f;       
    public float scanInterval = 0.2f;    

    [Header("Fade")]
    public float fadeTime = 1.0f;

    // ---- internal ----
    Transform player;
    bool inBattle = false;
    Coroutine fadeRoutine;

    float NormalVol => PlayerPrefs.GetFloat("MusicVolume", 1f);
    float CombatVol => PlayerPrefs.GetFloat("MusicCombat", 1f);

    void Start()
    {
        if (normalBGM) { normalBGM.loop = true; normalBGM.volume = NormalVol; normalBGM.Play(); }
        if (battleBGM) { battleBGM.loop = true; battleBGM.volume = 0f; battleBGM.Stop(); }

        StartCoroutine(ScanLoop());
    }

    IEnumerator ScanLoop()
    {
        while (true)
        {
            if (!player)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go) player = go.transform;
            }

            if (player)
            {
                bool foundEnemy = IsEnemyWithinRadius();
                if (foundEnemy && !inBattle)
                {
                    inBattle = true;
                    StartFade(SwitchToBattle());
                }
                else if (!foundEnemy && inBattle)
                {
                    inBattle = false;
                    StartFade(SwitchToNormal());
                }
            }

            yield return new WaitForSeconds(scanInterval);
        }
    }

    bool IsEnemyWithinRadius()
    {
        bool anyHit = false;

        if (enemyLayers != 0)
        {
            var hits = Physics.OverlapSphere(player.position, checkRadius, enemyLayers, QueryTriggerInteraction.Ignore);
            anyHit = hits != null && hits.Length > 0;
        }

        if (!anyHit && !string.IsNullOrEmpty(enemyTag))
        {
            var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (var e in enemies)
            {
                if (!e) continue;
                float d = Vector3.Distance(player.position, e.transform.position);
                if (d <= checkRadius) { anyHit = true; break; }
            }
        }

        return anyHit;
    }

    void StartFade(IEnumerator routine)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(routine);
    }

    IEnumerator SwitchToBattle()
    {
        if (battleBGM && !battleBGM.isPlaying) battleBGM.Play();

        float t = 0f;
        float startN = normalBGM ? normalBGM.volume : 0f;
        float startB = battleBGM ? battleBGM.volume : 0f;
        float targetN = 0f;
        float targetB = CombatVol;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (normalBGM) normalBGM.volume = Mathf.Lerp(startN, targetN, t / fadeTime);
            if (battleBGM) battleBGM.volume = Mathf.Lerp(startB, targetB, t / fadeTime);
            yield return null;
        }
        if (normalBGM) { normalBGM.volume = 0f; normalBGM.Stop(); }
    }

    IEnumerator SwitchToNormal()
    {
        if (normalBGM && !normalBGM.isPlaying) normalBGM.Play();

        float t = 0f;
        float startN = normalBGM ? normalBGM.volume : 0f;
        float startB = battleBGM ? battleBGM.volume : 0f;
        float targetN = NormalVol;
        float targetB = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (normalBGM) normalBGM.volume = Mathf.Lerp(startN, targetN, t / fadeTime);
            if (battleBGM) battleBGM.volume = Mathf.Lerp(startB, targetB, t / fadeTime);
            yield return null;
        }
        if (battleBGM) { battleBGM.volume = 0f; battleBGM.Stop(); }
    }

    public void UpdateNormalVolumeFromPrefs() { if (!inBattle && normalBGM) normalBGM.volume = NormalVol; }
    public void UpdateCombatVolumeFromPrefs() { if (inBattle && battleBGM) battleBGM.volume = CombatVol; }

    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        Gizmos.color = Color.yellow;
        Vector3 center = player ? player.position : transform.position;
        Gizmos.DrawWireSphere(center, checkRadius);
    }
}
