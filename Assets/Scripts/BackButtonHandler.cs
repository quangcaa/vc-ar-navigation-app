using UnityEngine;
using UnityEngine.UI;

public class BackButtonHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button backButton; // Kéo nút Back Button từ Screen 2 vào đây
    [SerializeField] private UIScreenSwitcher screenSwitcher; // Kéo UIScreenSwitcher vào đây
    [SerializeField] private HomeScreenController homeScreenController; // Kéo HomeScreenController (trên Screen 1) vào đây

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            Debug.LogWarning("BackButtonHandler: backButton chưa được assign!");
        }
    }

    private void OnBackButtonClicked()
    {
        // Use screen switcher API to go back to screen 1
        if (screenSwitcher != null)
        {
            screenSwitcher.ShowScreen1();
        }

        // Ask home screen to reload/reset its data/UI
        if (homeScreenController != null)
        {
            homeScreenController.ReloadData();
            Debug.Log("Đã quay lại Screen 1 và reload dữ liệu!");
        }
        else
        {
            Debug.LogWarning("BackButtonHandler: homeScreenController chưa được assign!");
        }
    }
}