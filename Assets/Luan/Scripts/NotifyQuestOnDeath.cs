using UnityEngine;
using System.Reflection;
using UnityEngine.Events;
using Invector.vCharacterController.vActions;
using Invector.vCharacterController;

public class NotifyQuestOnDeath : MonoBehaviour
{
    void Start()
    {
        var onDeadTrigger = GetComponent<vOnDeadTrigger>();
        if (onDeadTrigger != null)
        {
            // Lấy field "onDead" dạng UnityEvent<GameObject>
            var onDeadField = typeof(vOnDeadTrigger).GetField("onDead", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (onDeadField != null)
            {
                // Chuyển thành UnityEvent<GameObject>
                UnityEvent<GameObject> onDeadEvent = onDeadField.GetValue(onDeadTrigger) as UnityEvent<GameObject>;

                if (onDeadEvent != null)
                {
                    onDeadEvent.AddListener(OnEnemyDeath);
                }
                else
                {
                    Debug.LogWarning("onDeadEvent null: không thể cast thành UnityEvent<GameObject>");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy field 'onDead' trong vOnDeadTrigger.");
            }
        }
    }

    // Hàm phải có đúng tham số như UnityEvent<GameObject>
    void OnEnemyDeath(GameObject sender)
    {
        Debug.Log("🔔 Enemy chết → báo về QuestManager");
        if (QuestManager.instance != null)
        {
            QuestManager.instance.EnemyKilled();
        }
    }
}
