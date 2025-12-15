using UnityEngine;
using UnityEngine.EventSystems;

public class StorePOI : MonoBehaviour, IPointerClickHandler
{
    public string storeId;

    public void OnPointerClick(PointerEventData eventData)
    {
        StoreDetailsUI.Instance.ShowStoreById(storeId, transform);
    }

    void OnMouseDown()
    {
        StoreDetailsUI.Instance.ShowStoreById(storeId, transform);
    }

    public void OnClickFromButton()
    {
        StoreDetailsUI.Instance.ShowStoreById(storeId, transform);
    }
}
