using UnityEngine;

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
}
