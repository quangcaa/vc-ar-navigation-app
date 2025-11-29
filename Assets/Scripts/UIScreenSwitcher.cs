using UnityEngine;

/// <summary>
/// Manages switching between different UI screens (start, home, navigateScreen)
/// </summary>
public class UIScreenSwitcher : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject navigateScreen;

    [Header("Settings")]
    [SerializeField] private bool hideOtherScreensOnStart = true;

    private void Awake()
    {
        // Try to find screens if not assigned
        if (startScreen == null)
        {
            GameObject found = GameObject.Find("StartScreen");
            if (found != null) startScreen = found;
        }

        if (homeScreen == null)
        {
            GameObject found = GameObject.Find("HomeScreen");
            if (found != null) homeScreen = found;
        }

        if (navigateScreen == null)
        {
            GameObject found = GameObject.Find("NavigateScreen");
            if (found != null) navigateScreen = found;
        }
    }

    private void Start()
    {
        if (hideOtherScreensOnStart)
        {
            // Show only start screen by default
            ShowStartScreen();
        }
    }

    /// <summary>
    /// Shows the start screen and hides others
    /// </summary>
    public void ShowStartScreen()
    {
        SetScreenActive(startScreen, true);
        SetScreenActive(homeScreen, false);
        SetScreenActive(navigateScreen, false);
    }

    /// <summary>
    /// Shows the home screen and hides others
    /// Can be called directly from button's OnClick event
    /// </summary>
    public void ShowHomeScreen()
    {
        SetScreenActive(startScreen, false);
        SetScreenActive(homeScreen, true);
        SetScreenActive(navigateScreen, false);
    }

    /// <summary>
    /// Shows the navigate screen and hides others
    /// </summary>
    public void ShowNavigateScreen()
    {
        SetScreenActive(startScreen, false);
        SetScreenActive(homeScreen, false);
        SetScreenActive(navigateScreen, true);
    }

    /// <summary>
    /// Helper method to safely set screen active state
    /// </summary>
    private void SetScreenActive(GameObject screen, bool active)
    {
        if (screen != null)
        {
            screen.SetActive(active);
        }
    }

    // Legacy method for backward compatibility
    /// <summary>
    /// Shows Screen 2 (navigateScreen) with optional UI element hiding
    /// </summary>
    public void ShowScreen2(bool hidePin = false, bool hideMeter = false, bool hideArrivalText = false)
    {
        ShowNavigateScreen();
        // Additional logic for hiding specific UI elements can be added here if needed
    }
}