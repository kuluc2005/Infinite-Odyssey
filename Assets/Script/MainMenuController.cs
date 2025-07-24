using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelKhoiDau;
    public GameObject panelTiepTuc;

    void Start()
    {
        // Kiểm tra xem có dữ liệu lưu không
        if (PlayerPrefs.HasKey("SaveScene"))
        {
            ShowPanelTiepTuc();
        }
        else
        {
            ShowPanelKhoiDau();
        }
    }

    public void ShowPanelKhoiDau()
    {
        panelKhoiDau.SetActive(true);
        panelTiepTuc.SetActive(false);
    }

    public void ShowPanelTiepTuc()
    {
        panelKhoiDau.SetActive(false);
        panelTiepTuc.SetActive(true);
    }

    public void OnClickHanhTrinhMoi()
    {
        Debug.Log("Bắt đầu hành trình mới.");
        // Xóa dữ liệu cũ
        PlayerPrefs.DeleteKey("SaveScene");
        // Load scene mặc định cho hành trình mới
        SceneManager.LoadScene("Demo");
    }

    public void OnClickTiepTucHanhTrinh()
    {
        Debug.Log("Tiếp tục hành trình đã lưu.");
        // Load scene đã lưu
        string savedScene = PlayerPrefs.GetString("SaveScene", "Demo");
        SceneManager.LoadScene(savedScene);
    }

    public void OnClickCaiDat()
    {
        Debug.Log("Cài đặt.");
        // Show panel cài đặt nếu có
    }

    public void OnClickRoiBoHanhTrinh()
    {
        Debug.Log("Thoát game, lưu scene hiện tại...");
        // Lưu scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SaveScene", currentScene);
        PlayerPrefs.Save();

        // Thoát game
        Application.Quit();
    }

//    public void OnClickRoiBoHanhTrinh()
//    {
//        Debug.Log("Thoát game (WebGL). Lưu scene hiện tại...");
//        string currentScene = SceneManager.GetActiveScene().name;
//        PlayerPrefs.SetString("SaveScene", currentScene);
//        PlayerPrefs.Save();

//#if UNITY_WEBGL && !UNITY_EDITOR
//    // Đổi URL thành trang muốn redirect (ví dụ Google)
//    Application.ExternalEval("window.location.href='https://www.google.com';");
//#else
//        Application.Quit();
//#endif
//    }
}
