using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MiniMapPathCopyFrom3D : MonoBehaviour
{
    [Header("LineRenderer đang vẽ đường trên map 3D (ở NavigationController)")]
    public LineRenderer source;

    [Header("Nâng đường lên để không bị map che")]
    public float yOffset = 0.6f;

    [Header("Tần suất cập nhật")]
    public float updateInterval = 0.2f;

    LineRenderer target;
    float t;

    void Awake()
    {
        target = GetComponent<LineRenderer>();
        target.useWorldSpace = true;
    }

    void Update()
    {
        if (source == null) return;

        t += Time.deltaTime;
        if (t < updateInterval) return;
        t = 0f;

        int n = source.positionCount;
        if (n < 2)
        {
            target.positionCount = 0;
            return;
        }

        target.positionCount = n;

        for (int i = 0; i < n; i++)
        {
            Vector3 p = source.GetPosition(i);
            p.y += yOffset;
            target.SetPosition(i, p);
        }
    }
}
