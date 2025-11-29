using UnityEngine;
using TMPro;

public class UIScreenSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject screen1;
    [SerializeField] private GameObject screen2;
    [Header("Screen2 UI Parts (optional)")]
    [SerializeField] private GameObject pinObject;
    [SerializeField] private GameObject meterObject;
    [SerializeField] private TextMeshProUGUI arrivalText;

    // Store selected location data
    private LocationData selectedLocation;

    //Nút Start
    public void ShowScreen2()
    {
        // Default: show all UI parts
        ShowScreen2(false, false, false);
    }

    /// <summary>
    /// Show screen 2 and optionally hide certain UI parts (pin, meter, arrival text)
    /// </summary>
    public void ShowScreen2(bool hidePin, bool hideMeter, bool hideArrivalText)
    {
        if (screen1 != null)
            screen1.SetActive(false);
        if (screen2 != null)
            screen2.SetActive(true);

        // Toggle optional parts
        if (pinObject != null)
            pinObject.SetActive(!hidePin);

        if (meterObject != null)
            meterObject.SetActive(!hideMeter);

        if (arrivalText != null)
            arrivalText.gameObject.SetActive(!hideArrivalText);
    }

    /// <summary>
    /// Navigate to screen 2 with selected location
    /// </summary>
    public void ShowScreen2(LocationData location)
    {
        selectedLocation = location;
        ShowScreen2();
    }

    /// <summary>
    /// Get the selected location
    /// </summary>
    public LocationData GetSelectedLocation()
    {
        return selectedLocation;
    }

    /// <summary>
    /// Show screen 1 and hide screen 2.
    /// Use this to navigate back to Screen 1.
    /// </summary>
    public void ShowScreen1()
    {
        if (screen2 != null)
            screen2.SetActive(false);
        if (screen1 != null)
            screen1.SetActive(true);
    }
}