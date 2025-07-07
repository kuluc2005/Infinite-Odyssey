using UnityEngine;
using TMPro;

public class ErrorPanelManager : MonoBehaviour
{
    public GameObject errorPanel;
    public TMP_Text errorText;

    // Gọi hàm này để hiện thông báo, truyền thêm màu (nếu không có thì mặc định đỏ)
    public void ShowError(string message, Color? color = null)
    {
        errorText.text = message;
        errorText.color = color ?? Color.red;  // Mặc định đỏ nếu không truyền
        errorPanel.SetActive(true);
    }

    // Gán vào nút Close
    public void HideError()
    {
        errorPanel.SetActive(false);
    }
}
