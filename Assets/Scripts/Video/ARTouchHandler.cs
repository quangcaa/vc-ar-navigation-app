using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARTouchHandler : MonoBehaviour
{
    public Camera arCamera;  // gán ARCamera

    void Update()
    {
        // Dùng chuột (editor) hoặc touch (mobile)
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTouch(Input.GetTouch(0).position);
        }
#endif
    }

    void HandleTouch(Vector2 screenPos)
    {
        
        if (HandleUIRaycast(screenPos))
        {
            return;
        }

        
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Kiểm tra xem object bị chạm có script VideoToggleButton ko
            VideoToggleButton btn = hit.collider.GetComponent<VideoToggleButton>();
            if (btn != null)
            {
                btn.OnButtonPressed();
            }
        }
    }

   
    bool HandleUIRaycast(Vector2 screenPos)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Kiểm tra StorePOI trong UI
            StorePOI storePOI = result.gameObject.GetComponentInParent<StorePOI>();
            if (storePOI != null)
            {
                storePOI.OnClickFromButton();
                return true;
            }

            // Kiểm tra VideoToggleButton trong UI
            VideoToggleButton btn = result.gameObject.GetComponent<VideoToggleButton>();
            if (btn != null)
            {
                btn.OnButtonPressed();
                return true;
            }
        }

        return false;
    }
}
