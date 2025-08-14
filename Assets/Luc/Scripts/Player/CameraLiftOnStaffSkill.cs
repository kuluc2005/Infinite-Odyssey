using UnityEngine;
using Invector.vCamera;   

public class CameraLiftOnStaffSkill : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private vThirdPersonCamera cam;   // Kéo vThirdPersonCamera trong scene vào
    [SerializeField] private Animator animator;        // Tự lấy từ Player 

    [Header("Animator state names của skill bay")]
    public string[] skillStates = { "StaffSkill2", "StaffSkill3" };
    public int layerIndex = 0; // Base Layer = 0 (đổi nếu state ở layer khác)

    [Header("Thiết lập camera khi bay")]
    public float airPivotY = 1f;     // cao hơn khi bay
    public float airDistance = 1.5f;   // lùi xa hơn khi bay
    public float smooth = 8f;

    private float groundPivotY;
    private float groundDistance;
    private bool cached;
    private bool isInSkill;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!cam)
        {
#if UNITY_6000_0_OR_NEWER
            cam = FindFirstObjectByType<vThirdPersonCamera>();
#else
            cam = FindObjectOfType<vThirdPersonCamera>();
#endif
        }
        if (cam)
        {
            groundPivotY = cam.offSetPlayerPivot; 
            groundDistance = cam.distance;
            cached = true;
        }
    }

    void Update()
    {
        if (!cached || !animator) return;

        var s = animator.GetCurrentAnimatorStateInfo(layerIndex);
        bool nowInSkill = false;
        foreach (var n in skillStates)
        {
            if (!string.IsNullOrEmpty(n) && (s.IsName(n) || s.IsName("Base Layer." + n)))
            {
                nowInSkill = true; break;
            }
        }
        isInSkill = nowInSkill;
    }

    void LateUpdate()
    {
        if (!cached || !cam) return;

        float targetY = isInSkill ? airPivotY : groundPivotY;
        float targetDist = isInSkill ? airDistance : groundDistance;

        cam.offSetPlayerPivot = Mathf.Lerp(cam.offSetPlayerPivot, targetY, Time.deltaTime * smooth);
        cam.distance = Mathf.Lerp(cam.distance, targetDist, Time.deltaTime * smooth);

        // đồng bộ lại defaultDistance của state hiện tại (Invector dùng biến này nội bộ)
        if (cam.currentState != null) cam.currentState.defaultDistance = cam.distance;
    }

    // Nếu muốn “reset cứng” (vd. khi đổi scene), có thể gọi hàm này
    public void ResetCameraDefaults()
    {
        if (!cam || !cached) return;
        cam.offSetPlayerPivot = groundPivotY;
        cam.distance = groundDistance;
        if (cam.currentState != null) cam.currentState.defaultDistance = cam.distance;
    }
}
