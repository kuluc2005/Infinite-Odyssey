using UnityEngine;

[RequireComponent(typeof(Animation))]
public class ChestLegacyPlayerPro : MonoBehaviour
{
    public Animation anim;
    public AnimationClip openClip;   // kéo Fantasy_Polygon_Chest_Animation vào đây
    public float crossFade = 0.15f;

    void Awake()
    {
        if (!anim) anim = GetComponent<Animation>();
        anim.playAutomatically = false;

        // đảm bảo state tồn tại trong Animation
        if (openClip && anim[openClip.name] == null)
            anim.AddClip(openClip, openClip.name);
    }

    public void PlayOpen()
    {
        if (!anim || !openClip) return;

        if (anim[openClip.name] == null)
        {
            Debug.LogError("State not found on THIS Animation: " + openClip.name + " (on " + name + ")");
            foreach (AnimationState s in anim) Debug.Log("Has state: " + s.name);
            return;
        }

        anim.Stop();
        anim.CrossFade(openClip.name, crossFade); // hoặc anim.Play(openClip.name);
    }
}
