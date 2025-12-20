using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private GameObject miniMapRoot; // MiniMapPanel
    [SerializeField] private Camera miniMapCamera;   // MiniMapCamera (optional)

    public bool IsVisible => miniMapRoot != null && miniMapRoot.activeSelf;

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
        if (miniMapCamera != null) miniMapCamera.enabled = visible; // tiết kiệm hiệu năng
    }
}
