using UnityEngine;

public class MiniMapToggle : MonoBehaviour
{
    [SerializeField] private GameObject miniMapRoot;

    public void Toggle()
    {
        if (miniMapRoot == null) return;
        miniMapRoot.SetActive(!miniMapRoot.activeSelf);
    }

    public void Show()
    {
        if (miniMapRoot == null) return;
        miniMapRoot.SetActive(true);
    }

    public void Hide()
    {
        if (miniMapRoot == null) return;
        miniMapRoot.SetActive(false);
    }
}
