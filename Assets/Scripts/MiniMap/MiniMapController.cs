using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private GameObject miniMapRoot; // MiniMapPanel
    [SerializeField] private Camera miniMapCamera;   // MiniMapCamera (optional)
    [SerializeField] private GameObject btn2D;

    public bool IsVisible => miniMapRoot != null && miniMapRoot.activeSelf;

        void Start()
    {
        SetVisible(false);
    }

    public void Toggle()
    {
        SetVisible(!IsVisible);
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (miniMapRoot != null) miniMapRoot.SetActive(visible);
        if (miniMapCamera != null) miniMapCamera.enabled = visible; 
    }
}
