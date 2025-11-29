using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controller for search field with dropdown suggestions
/// </summary>
public class SearchDropdownController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private Transform dropdownContent;
    [SerializeField] private GameObject suggestionItemPrefab;

    [Header("Example Data")]
    [SerializeField] private List<string> examplePlaces = new List<string>
    {
        "KFC - Cầu Giấy",
        "KFC - Ba Đình",
        "KFC - Hoàn Kiếm",
        "McDonald's - Trần Duy Hưng",
        "Lotteria - Times City",
        "Pizza Hut - Royal City",
        "Highlands Coffee - Vincom",
        "The Coffee House - Lotte Center",
        "VinMart - Cầu Giấy",
        "Big C - Thăng Long",
        "Aeon Mall - Long Biên",
        "Vincom Center - Ba Đình",
        "Lotte Center - Liễu Giai",
        "Tràng Tiền Plaza",
        "Bệnh viện Bạch Mai",
        "Bệnh viện Việt Đức",
        "Trường Đại học Bách Khoa",
        "Trường Đại học Kinh tế Quốc dân"
    };

    [Header("Settings")]
    [SerializeField] private int maxSuggestions = 5;
    [SerializeField] private float dropdownOffset = 5f;
    [SerializeField] private float itemHeight = 40f; // Chiều cao mỗi suggestion item
    [SerializeField] private float panelPadding = 10f; // Padding top + bottom của panel

    private List<GameObject> currentSuggestions = new List<GameObject>();
    private bool isDropdownVisible = false;

    private void Awake()
    {
        // Try to find components if not assigned
        if (searchInputField == null)
        {
            searchInputField = GetComponentInChildren<TMP_InputField>();
        }

        if (dropdownPanel == null)
        {
            // Try to find dropdown panel in children
            Transform dropdown = transform.Find("DropdownPanel");
            if (dropdown != null)
            {
                dropdownPanel = dropdown.gameObject;
                dropdownContent = dropdown.Find("Content");
            }
        }

        // Ensure dropdown panel has Content Size Fitter if it exists
        if (dropdownPanel != null)
        {
            ContentSizeFitter sizeFitter = dropdownPanel.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = dropdownPanel.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        // Hide dropdown initially
        if (dropdownPanel != null)
        {
            dropdownPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Subscribe to input field events
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
            searchInputField.onSelect.AddListener(OnSearchFieldSelected);
            searchInputField.onDeselect.AddListener(OnSearchFieldDeselected);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.RemoveListener(OnSearchValueChanged);
            searchInputField.onSelect.RemoveListener(OnSearchFieldSelected);
            searchInputField.onDeselect.RemoveListener(OnSearchFieldDeselected);
        }
    }

    /// <summary>
    /// Called when search input value changes
    /// </summary>
    private void OnSearchValueChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            ShowAllSuggestions();
        }
        else
        {
            FilterSuggestions(searchText);
        }
    }

    /// <summary>
    /// Called when search field is selected
    /// </summary>
    private void OnSearchFieldSelected(string value)
    {
        ShowDropdown();
    }

    /// <summary>
    /// Called when search field is deselected
    /// </summary>
    private void OnSearchFieldDeselected(string value)
    {
        // Delay hiding to allow clicking on suggestions
        Invoke(nameof(HideDropdown), 0.2f);
    }

    /// <summary>
    /// Shows all example suggestions
    /// </summary>
    private void ShowAllSuggestions()
    {
        ShowSuggestions(examplePlaces.Take(maxSuggestions).ToList());
    }

    /// <summary>
    /// Filters suggestions based on search text
    /// </summary>
    private void FilterSuggestions(string searchText)
    {
        string lowerSearchText = searchText.ToLower();
        List<string> filtered = examplePlaces
            .Where(place => place.ToLower().Contains(lowerSearchText))
            .Take(maxSuggestions)
            .ToList();

        ShowSuggestions(filtered);
    }

    /// <summary>
    /// Displays suggestions in dropdown
    /// </summary>
    private void ShowSuggestions(List<string> suggestions)
    {
        // Clear existing suggestions
        ClearSuggestions();

        if (suggestions.Count == 0)
        {
            HideDropdown();
            return;
        }

        // Create dropdown if it doesn't exist
        if (dropdownPanel == null)
        {
            CreateDropdownUI();
        }

        // Create suggestion items
        foreach (string suggestion in suggestions)
        {
            CreateSuggestionItem(suggestion);
        }

        // Update dropdown height based on number of items
        StartCoroutine(UpdateDropdownHeightCoroutine(suggestions.Count));

        ShowDropdown();
    }

    /// <summary>
    /// Creates a suggestion item in the dropdown
    /// </summary>
    private void CreateSuggestionItem(string text)
    {
        GameObject item;
        
        if (suggestionItemPrefab != null)
        {
            item = Instantiate(suggestionItemPrefab, dropdownContent);
        }
        else
        {
            // Create a simple button if no prefab is assigned
            item = new GameObject("SuggestionItem");
            item.transform.SetParent(dropdownContent, false);
            
            // Add RectTransform
            RectTransform rectTransform = item.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 40);
            
            // Add Button
            Button button = item.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.8f);
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            button.colors = colors;
            
            // Add Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(item.transform, false);
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.color = Color.black;
            textComponent.alignment = TextAlignmentOptions.Left;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // Add padding
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            // Add click listener
            button.onClick.AddListener(() => OnSuggestionSelected(text));
        }

        // If prefab doesn't have a button, try to find and configure it
        Button itemButton = item.GetComponentInChildren<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => OnSuggestionSelected(text));
        }

        // Set text if prefab has TextMeshProUGUI
        TextMeshProUGUI textMesh = item.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = text;
        }

        currentSuggestions.Add(item);
    }

    /// <summary>
    /// Creates dropdown UI if it doesn't exist
    /// </summary>
    private void CreateDropdownUI()
    {
        // Create dropdown panel
        GameObject panel = new GameObject("DropdownPanel");
        panel.transform.SetParent(transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, -dropdownOffset);
        
        // Position below search field
        if (searchInputField != null)
        {
            RectTransform inputRect = searchInputField.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                panelRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, 0);
                panelRect.anchoredPosition = new Vector2(0, -inputRect.sizeDelta.y - dropdownOffset);
            }
        }
        
        // Add Image for background
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 0.95f);
        
        // Add Vertical Layout Group
        VerticalLayoutGroup layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 2;
        layoutGroup.padding = new RectOffset(5, 5, 5, 5);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        // Add Content Size Fitter
        ContentSizeFitter sizeFitter = panel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;
        
        dropdownPanel = panel;
        dropdownContent = content.transform;
    }

    /// <summary>
    /// Coroutine to update dropdown height while preserving Y position
    /// </summary>
    private IEnumerator UpdateDropdownHeightCoroutine(int itemCount)
    {
        if (dropdownPanel == null || itemCount == 0)
            yield break;

        RectTransform panelRect = dropdownPanel.GetComponent<RectTransform>();
        if (panelRect == null)
            yield break;

        // Save original settings
        float originalY = panelRect.anchoredPosition.y;
        float originalX = panelRect.anchoredPosition.x;
        float originalWidth = panelRect.sizeDelta.x;
        Vector2 originalPivot = panelRect.pivot;
        float originalHeight = panelRect.sizeDelta.y;

        // Calculate the original top edge Y position
        // Top edge Y = anchoredPosition.y + (height * (1 - pivot.y))
        float originalTopEdgeY = originalY + (originalHeight * (1f - originalPivot.y));

        // Set pivot to top (Y = 1) so that when height changes, top position stays fixed
        // This way only bottom will move when height changes
        if (panelRect.pivot.y != 1f)
        {
            // Set pivot to top
            panelRect.pivot = new Vector2(panelRect.pivot.x, 1f);
        }

        // Disable Content Size Fitter temporarily to prevent automatic position changes
        ContentSizeFitter sizeFitter = dropdownPanel.GetComponent<ContentSizeFitter>();
        bool sizeFitterWasEnabled = false;
        if (sizeFitter != null)
        {
            sizeFitterWasEnabled = sizeFitter.enabled;
            sizeFitter.enabled = false;
        }

        // Get Vertical Layout Group to calculate spacing
        VerticalLayoutGroup layoutGroup = dropdownPanel.GetComponent<VerticalLayoutGroup>();
        float spacing = layoutGroup != null ? layoutGroup.spacing : 0f;
        float padding = layoutGroup != null ? (layoutGroup.padding.top + layoutGroup.padding.bottom) : panelPadding;

        // Try to get actual item height from first item if available
        float actualItemHeight = itemHeight;
        if (currentSuggestions.Count > 0 && currentSuggestions[0] != null)
        {
            RectTransform itemRect = currentSuggestions[0].GetComponent<RectTransform>();
            if (itemRect != null)
            {
                actualItemHeight = itemRect.sizeDelta.y;
            }
        }

        // Calculate total height: (item height * count) + (spacing * (count - 1)) + padding
        float totalHeight = (actualItemHeight * itemCount) + (spacing * (itemCount - 1)) + padding;

        // Set height directly (only update Y component, keep X)
        panelRect.sizeDelta = new Vector2(originalWidth, totalHeight);

        // Wait for end of frame to ensure layout calculations are done
        yield return new WaitForEndOfFrame();

        // Force layout update
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }
        
        if (dropdownContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownContent as RectTransform);
        }

        // Wait one more frame
        yield return new WaitForEndOfFrame();

        // Restore original top edge Y position
        // With pivot at top (Y=1), anchoredPosition.y directly represents the top edge
        // So we can set it directly to the original top edge position
        panelRect.anchoredPosition = new Vector2(originalX, originalTopEdgeY);

        // Wait one more frame to ensure position is applied
        yield return new WaitForEndOfFrame();

        // Double-check and restore position one more time after all layout updates
        panelRect.anchoredPosition = new Vector2(originalX, originalTopEdgeY);

        // Re-enable Content Size Fitter if it was enabled before
        if (sizeFitter != null && sizeFitterWasEnabled)
        {
            sizeFitter.enabled = true;
        }
    }


    /// <summary>
    /// Shows the dropdown
    /// </summary>
    private void ShowDropdown()
    {
        if (dropdownPanel != null)
        {
            dropdownPanel.SetActive(true);
            isDropdownVisible = true;
            CancelInvoke(nameof(HideDropdown));
        }
    }

    /// <summary>
    /// Hides the dropdown
    /// </summary>
    private void HideDropdown()
    {
        if (dropdownPanel != null)
        {
            dropdownPanel.SetActive(false);
            isDropdownVisible = false;
        }
    }

    /// <summary>
    /// Clears all suggestion items
    /// </summary>
    private void ClearSuggestions()
    {
        foreach (GameObject suggestion in currentSuggestions)
        {
            if (suggestion != null)
            {
                Destroy(suggestion);
            }
        }
        currentSuggestions.Clear();
    }

    /// <summary>
    /// Called when a suggestion is selected
    /// </summary>
    private void OnSuggestionSelected(string selectedText)
    {
        if (searchInputField != null)
        {
            searchInputField.text = selectedText;
            searchInputField.DeactivateInputField();
        }
        
        HideDropdown();
        
        // You can add additional logic here, like starting navigation
        Debug.Log($"Selected place: {selectedText}");
    }

    /// <summary>
    /// Adds a new place to the example list
    /// </summary>
    public void AddExamplePlace(string place)
    {
        if (!examplePlaces.Contains(place))
        {
            examplePlaces.Add(place);
        }
    }

    /// <summary>
    /// Removes a place from the example list
    /// </summary>
    public void RemoveExamplePlace(string place)
    {
        examplePlaces.Remove(place);
    }
}

