using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasToggleButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject infoPrefab;      // Prefab cần hiện
    public float rightOffset = 0.25f;  // khoảng cách sang phải (mét)

    private GameObject spawned;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu chưa có -> spawn + hiện
        if (spawned == null)
        {
            spawned = Instantiate(infoPrefab);

             // Đặt ngay bên phải object vừa bấm
            spawned.transform.position = transform.position + transform.right * rightOffset;

            // Cho cùng hướng với image 3D
            spawned.transform.rotation = transform.rotation;

            return;
        }

        // Nếu đã có -> tắt + xoá
        Destroy(spawned);
        spawned = null;
    }
}
