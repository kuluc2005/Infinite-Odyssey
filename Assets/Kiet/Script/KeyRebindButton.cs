using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeyRebindButton : MonoBehaviour
{
    public string actionName; // Tên hành động: "Jump", "Sprint", v.v.
    public TMP_Text keyText;  // Text hiển thị phím đang gán
    public Button rebindButton;

    private bool waitingForKey = false;

    void Start()
    {
        // Lấy phím hiện tại và hiển thị lên UI
        KeyCode current = InputOverrideManager.GetKeyCode(actionName, DefaultInputMapping.defaultKeys[actionName]);
        keyText.text = current.ToString();

        // Khi bấm nút, bắt đầu quá trình chờ phím
        rebindButton.onClick.AddListener(() => StartCoroutine(WaitForKey()));
    }

    IEnumerator WaitForKey()
    {
        waitingForKey = true;
        keyText.text = "Press any key...";

        while (waitingForKey)
        {
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                {
                    InputOverrideManager.SetKey(actionName, k);
                    keyText.text = k.ToString();
                    waitingForKey = false;
                    yield break;
                }
            }
            yield return null;
        }
    }
}
