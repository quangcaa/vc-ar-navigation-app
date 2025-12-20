using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebugUIRaycast : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current == null)
        {
            Debug.LogError("NO EventSystem in scene!");
            return;
        }

        var data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0)
        {
            Debug.Log("UI Raycast: NOTHING hit (click not reaching UI)");
        }
        else
        {
            Debug.Log("UI Raycast hit (top -> bottom):");
            foreach (var r in results)
                Debug.Log($"- {r.gameObject.name} (module: {r.module}, depth: {r.depth})");
        }
    }
}
