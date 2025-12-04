using UnityEngine;
using UnityEngine.EventSystems;

// Attach this component to POI GameObjects (UI button or 3D object).
// Set `storeId` to the id (or storeName) from stores.json.
public class StorePOI : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Set the store id (or storeName) that matches stores.json")] 
    public string storeId;

    // For UI Buttons: you can call OnClickFromButton() from the Button's OnClick()
    public void OnClickFromButton()
    {
        // Gọi static method trực tiếp qua tên lớp
        StoreDetailsUI.ShowStoreById(storeId);
    }

    // For EventSystem pointer clicks (UI / world-space canvases)
    public void OnPointerClick(PointerEventData eventData)
    {
        StoreDetailsUI.ShowStoreById(storeId);
    }

    // For simple 3D object clicks in playmode (with collider)
    void OnMouseDown()
    {
        StoreDetailsUI.ShowStoreById(storeId);
    }
}
