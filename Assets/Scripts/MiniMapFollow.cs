using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    [Header("Target để mini-map follow (thường là Camera trong XR Origin)")]
    public Transform target;

    [Header("Độ cao của MiniMapCamera so với target")]
    public float height = 20f;

    [Header("Có xoay theo hướng của target không")]
    public bool rotateWithTarget = true;

    void LateUpdate()
    {
        if (target == null) return;

        // Vị trí: luôn nằm trên đầu target
        Vector3 newPos = target.position;
        newPos.y += height;
        transform.position = newPos;

        if (rotateWithTarget)
        {
            // Xoay mini-map theo hướng nhìn của target (trục Y)
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            // Chỉ nhìn thẳng xuống, không xoay
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
