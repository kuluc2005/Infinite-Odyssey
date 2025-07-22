using UnityEngine;
using UnityEngine.VFX;

public class AutoPlayVFX : MonoBehaviour
{
    private VisualEffect vfx;

    void OnEnable()
    {
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        if (vfx != null)
            vfx.Play();
    }
}
