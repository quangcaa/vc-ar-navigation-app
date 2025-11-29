using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Controller for the Home screen, handles search functionality
/// </summary>
public class HomeScreenController : MonoBehaviour
{
    [Header("Search UI References")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private SearchDropdown searchDropdown;
    [SerializeField] private GameObject searchContainer; // The rectangle container with placeholder text

    [Header("Data Provider")]
    [SerializeField] private ISearchableData dataProvider;

    [Header("Screen Navigation")]
    [SerializeField] private UIScreenSwitcher screenSwitcher;

    [Header("Settings")]
    [SerializeField] private float searchDelay = 0.3f; // Delay before searching (debounce)

    private float lastSearchTime;
    private string lastSearchQuery = "";
    private LocationData selectedLocation;

    void Awake()
    {
        // Try to find data provider if not assigned
        if (dataProvider == null)
        {
            dataProvider = FindObjectOfType<MockSearchDataProvider>();
        }

        // Try to find screen switcher if not assigned
        if (screenSwitcher == null)
        {
            screenSwitcher = FindObjectOfType<UIScreenSwitcher>();
        }
    }

    void Start()
    {
        SetupSearchInput();
        
        if (searchDropdown != null)
        {
            searchDropdown.Hide();
        }
    }

    void Update()
    {
        // Handle search input changes
        if (searchInputField != null && searchInputField.isFocused)
        {
            string currentQuery = searchInputField.text;
            
            if (currentQuery != lastSearchQuery)
            {
                lastSearchQuery = currentQuery;
                lastSearchTime = Time.time;
            }

            // Debounce search
            if (Time.time - lastSearchTime >= searchDelay && !string.IsNullOrEmpty(currentQuery))
            {
                PerformSearch(currentQuery);
            }
            else if (string.IsNullOrEmpty(currentQuery))
            {
                if (searchDropdown != null)
                    searchDropdown.Hide();
            }
        }
    }

    /// <summary>
    /// Sets up the search input field
    /// </summary>
    private void SetupSearchInput()
    {
        if (searchInputField == null)
        {
            Debug.LogWarning("HomeScreenController: searchInputField is not assigned!");
            return;
        }

        // Set placeholder text if needed
        if (searchInputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText = searchInputField.placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null && string.IsNullOrEmpty(placeholderText.text))
            {
                placeholderText.text = "Enter the place you want to go";
            }
        }

        // Add listeners
        searchInputField.onSelect.AddListener(OnSearchInputSelected);
        searchInputField.onDeselect.AddListener(OnSearchInputDeselected);
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
    }

    /// <summary>
    /// Called when search input is selected (clicked)
    /// </summary>
    private void OnSearchInputSelected(string value)
    {
        // Hide placeholder container if exists
        if (searchContainer != null)
        {
            searchContainer.SetActive(false);
        }

        // Show dropdown if there's text
        if (!string.IsNullOrEmpty(value))
        {
            PerformSearch(value);
        }
    }

    /// <summary>
    /// Called when search input is deselected
    /// </summary>
    private void OnSearchInputDeselected(string value)
    {
        // Optionally hide dropdown when input loses focus
        // Uncomment if you want dropdown to hide when clicking outside
        // if (searchDropdown != null)
        //     searchDropdown.Hide();
    }

    /// <summary>
    /// Called when search input value changes
    /// </summary>
    private void OnSearchInputChanged(string value)
    {
        lastSearchQuery = value;
        lastSearchTime = Time.time;

        // Immediately perform search as the user types (case-insensitive substring match
        // is handled by the data provider implementations).
        if (!string.IsNullOrEmpty(value))
        {
            PerformSearch(value);
        }
        else
        {
            if (searchDropdown != null)
                searchDropdown.Hide();
        }
    }

    /// <summary>
    /// Performs the search operation
    /// </summary>
    private void PerformSearch(string query)
    {
        if (dataProvider == null)
        {
            Debug.LogWarning("HomeScreenController: No data provider assigned!");
            return;
        }

        if (searchDropdown == null)
        {
            Debug.LogWarning("HomeScreenController: searchDropdown is not assigned!");
            return;
        }

        List<LocationData> results = dataProvider.SearchLocations(query);
        
        searchDropdown.ShowResults(results, OnLocationSelected);
    }

    /// <summary>
    /// Called when a location is selected from search results
    /// </summary>
    private void OnLocationSelected(LocationData location)
    {
        selectedLocation = location;
        Debug.Log($"Location selected: {location.name}");

        // Navigate to Android Compact 2 screen
        if (screenSwitcher != null)
        {
            screenSwitcher.ShowScreen2();
        }
        else
        {
            Debug.LogWarning("HomeScreenController: screenSwitcher is not assigned! Cannot navigate to next screen.");
        }
    }

    /// <summary>
    /// Called by the 'Tour' button on Screen 1.
    /// Shows Screen 2 but hides pin, meter and the "you have arrived" text.
    /// Wire your Tour button to this method in the Inspector.
    /// </summary>
    public void OnTourButtonPressed()
    {
        if (screenSwitcher != null)
        {
            // hidePin = true, hideMeter = true, hideArrivalText = true
            screenSwitcher.ShowScreen2(true, true, true);
            Debug.Log("Tour pressed: navigating to Screen 2 with UI parts hidden.");
        }
        else
        {
            Debug.LogWarning("HomeScreenController: screenSwitcher is not assigned! Cannot navigate to Screen 2.");
        }
    }

    /// <summary>
    /// Gets the currently selected location
    /// </summary>
    public LocationData GetSelectedLocation()
    {
        return selectedLocation;
    }

    /// <summary>
    /// Called when returning to this screen to reset UI state and reload data.
    /// Clears selected location, resets search input and dropdown, and refreshes provider data.
    /// </summary>
    public void ReloadData()
    {
        // Clear selection
        selectedLocation = null;

        // Clear search input
        if (searchInputField != null)
        {
            searchInputField.text = "";
        }

        // Hide dropdown
        if (searchDropdown != null)
        {
            searchDropdown.Hide();
        }

        // Refresh data from provider if supported
        if (dataProvider != null)
        {
            if (dataProvider is RealDataSearchProvider realProvider)
            {
                realProvider.RefreshLocations();
            }
            else if (dataProvider is MockSearchDataProvider mockProvider)
            {
                // Reset mock data to defaults (MockSearchDataProvider initializes defaults in Awake)
                // If you want a custom reset, modify MockSearchDataProvider to expose a ResetDefaults() method.
                mockProvider.SetLocations(mockProvider.GetAllLocations());
            }
        }
    }

    /// <summary>
    /// Sets the data provider (useful for dependency injection)
    /// </summary>
    public void SetDataProvider(ISearchableData provider)
    {
        dataProvider = provider;
    }
}

