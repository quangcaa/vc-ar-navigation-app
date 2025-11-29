using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component that displays search results in a dropdown list
/// </summary>
public class SearchDropdown : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private Transform resultsContainer;
    [SerializeField] private GameObject resultItemPrefab;
    [SerializeField] private TextMeshProUGUI noResultsText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] private int maxResults = 10;
    [SerializeField] private float itemHeight = 50f;

    private List<GameObject> currentResultItems = new List<GameObject>();
    private System.Action<LocationData> onResultSelected;

    void Awake()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
        
        if (noResultsText != null)
            noResultsText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows search results in the dropdown
    /// </summary>
    public void ShowResults(List<LocationData> results, System.Action<LocationData> onSelected)
    {
        onResultSelected = onSelected;
        
        ClearResults();

        if (results == null || results.Count == 0)
        {
            ShowNoResults();
            return;
        }

        if (dropdownPanel != null)
            dropdownPanel.SetActive(true);

        if (noResultsText != null)
            noResultsText.gameObject.SetActive(false);

        // Limit results
        int displayCount = Mathf.Min(results.Count, maxResults);
        
        for (int i = 0; i < displayCount; i++)
        {
            CreateResultItem(results[i]);
        }

        // Update container height
        UpdateContainerHeight(displayCount);
    }

    /// <summary>
    /// Hides the dropdown
    /// </summary>
    public void Hide()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
        
        ClearResults();
    }

    /// <summary>
    /// Creates a single result item
    /// </summary>
    private void CreateResultItem(LocationData location)
    {
        if (resultItemPrefab == null || resultsContainer == null)
        {
            Debug.LogWarning("SearchDropdown: resultItemPrefab or resultsContainer is not assigned!");
            return;
        }

        GameObject item = Instantiate(resultItemPrefab, resultsContainer);
        currentResultItems.Add(item);

        // Set up button click
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnResultItemClicked(location));
        }

        // Set text
        TextMeshProUGUI textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = location.name;
        }

        item.SetActive(true);
    }

    /// <summary>
    /// Handles result item click
    /// </summary>
    private void OnResultItemClicked(LocationData location)
    {
        onResultSelected?.Invoke(location);
        Hide();
    }

    /// <summary>
    /// Shows "no results" message
    /// </summary>
    private void ShowNoResults()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(true);

        if (noResultsText != null)
        {
            noResultsText.text = "Không có địa điểm cần tìm";
            noResultsText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Clears all result items
    /// </summary>
    private void ClearResults()
    {
        foreach (GameObject item in currentResultItems)
        {
            if (item != null)
                Destroy(item);
        }
        currentResultItems.Clear();
    }

    /// <summary>
    /// Updates the container height based on number of results
    /// </summary>
    private void UpdateContainerHeight(int itemCount)
    {
        if (resultsContainer == null) return;

        RectTransform containerRect = resultsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, itemCount * itemHeight);
        }

        // Reset scroll position
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}

