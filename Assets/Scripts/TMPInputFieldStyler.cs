using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Styles TMP_InputField to remove borders, shadows, and backgrounds - only show text
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class TMPInputFieldStyler : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Manual Setup")]
    [SerializeField] private bool removeBackground = true;
    [SerializeField] private bool removeShadow = true;
    [SerializeField] private bool removeOutline = true;

    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        if (autoSetupOnStart)
        {
            StyleInputField();
        }
    }

    /// <summary>
    /// Styles the input field to remove borders, shadows, and backgrounds
    /// </summary>
    [ContextMenu("Style Input Field")]
    public void StyleInputField()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                Debug.LogWarning("TMP_InputField not found!");
                return;
            }
        }

        // Remove background image
        if (removeBackground)
        {
            RemoveBackground();
        }

        // Remove shadow and outline from text
        if (removeShadow || removeOutline)
        {
            RemoveTextEffects();
        }
    }

    /// <summary>
    /// Removes background image from input field
    /// </summary>
    private void RemoveBackground()
    {
        // Remove background from main input field
        Image backgroundImage = inputField.GetComponent<Image>();
        if (backgroundImage != null)
        {
            // Make it transparent instead of removing (to keep the component)
            backgroundImage.color = new Color(0, 0, 0, 0);
            // Or remove the sprite
            backgroundImage.sprite = null;
        }

        // Also check the text area/viewport
        if (inputField.textViewport != null)
        {
            Image viewportImage = inputField.textViewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.color = new Color(0, 0, 0, 0);
                viewportImage.sprite = null;
            }
        }

        // Check text component's background
        if (inputField.textComponent != null)
        {
            Image textImage = inputField.textComponent.GetComponent<Image>();
            if (textImage != null)
            {
                textImage.color = new Color(0, 0, 0, 0);
                textImage.sprite = null;
            }
        }
    }

    /// <summary>
    /// Removes shadow and outline effects from text
    /// </summary>
    private void RemoveTextEffects()
    {
        if (inputField.textComponent != null)
        {
            // Remove Shadow component
            if (removeShadow)
            {
                Shadow shadow = inputField.textComponent.GetComponent<Shadow>();
                if (shadow != null)
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(shadow);
                    #else
                    Destroy(shadow);
                    #endif
                }

                // Also check for Outline (which is a type of Shadow)
                Outline outline = inputField.textComponent.GetComponent<Outline>();
                if (outline != null && removeOutline)
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(outline);
                    #else
                    Destroy(outline);
                    #endif
                }
            }
            else if (removeOutline)
            {
                // Only remove outline, keep shadow
                Outline outline = inputField.textComponent.GetComponent<Outline>();
                if (outline != null)
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(outline);
                    #else
                    Destroy(outline);
                    #endif
                }
            }

            // Disable shadow/outline in material if using TextMeshPro effects
            inputField.textComponent.enableVertexGradient = false;
        }

        // Also check placeholder text
        if (inputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                if (removeShadow)
                {
                    Shadow shadow = placeholderText.GetComponent<Shadow>();
                    if (shadow != null)
                    {
                        #if UNITY_EDITOR
                        DestroyImmediate(shadow);
                        #else
                        Destroy(shadow);
                        #endif
                    }
                }

                if (removeOutline)
                {
                    Outline outline = placeholderText.GetComponent<Outline>();
                    if (outline != null)
                    {
                        #if UNITY_EDITOR
                        DestroyImmediate(outline);
                        #else
                        Destroy(outline);
                        #endif
                    }
                }
            }
        }
    }

    /// <summary>
    /// Resets the input field to default appearance (if needed)
    /// </summary>
    [ContextMenu("Reset to Default")]
    public void ResetToDefault()
    {
        // This method can be used to restore default appearance if needed
        // Implementation depends on what default values you want
        Debug.Log("Reset to default - implement as needed");
    }
}

