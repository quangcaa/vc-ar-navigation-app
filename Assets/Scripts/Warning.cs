using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class AlarmNearestExit : MonoBehaviour, IPointerClickHandler
{
    [Header("Tham chiếu")]
    public NavMeshAgent agent;
    public float searchRadius = 100f;

   public void OnPointerClick(PointerEventData eventData)
    {
        NavigateToNearestExit();
    }

    public void NavigateToNearestExit()
    {
        if (agent == null)
            agent = NavigationController.instance != null ? NavigationController.instance.agent : FindObjectOfType<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogWarning("[AlarmNearestExit] Không tìm thấy NavMeshAgent.");
            return;
        }

        var exits = Object.FindObjectsOfType<POI>()
                        .Where(p => p != null && p.type == POIType.Exit && p.poiCollider != null)
                        .ToList();
        if (exits.Count == 0)
        {
            Debug.LogWarning("[AlarmNearestExit] Không tìm thấy POI type Exit.");
            return;
        }

        Vector3 myPos = agent.transform.position;
        POI bestPoi = null;
        Vector3 bestPos = Vector3.zero;
        float bestDist = float.MaxValue;
        var path = new NavMeshPath();

        foreach (var poi in exits)
        {
            var targetPos = poi.poiCollider.transform.position;

            if (NavMesh.CalculatePath(myPos, targetPos, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                float pathLen = 0f;
                for (int i = 0; i < path.corners.Length - 1; i++)
                    pathLen += Vector3.Distance(path.corners[i], path.corners[i + 1]);

                if (pathLen < bestDist)
                {
                    bestDist = pathLen;
                    bestPoi = poi;
                    bestPos = targetPos;
                }
            }
            else
            {
                float dist = Vector3.Distance(myPos, targetPos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPoi = poi;
                    bestPos = targetPos;
                }
            }
        }

        if (bestPoi == null)
        {
            Debug.LogWarning("[AlarmNearestExit] Không tìm được đường hợp lệ tới lối ra.");
            return;
        }

        // 1) Kích hoạt điều hướng qua NavigationController (UI + path + arrived handling)
        if (NavigationController.instance != null)
        {
            NavigationController.instance.SetPOIForNavigation(bestPoi);
        }
        else
        {
            // Fallback: trực tiếp đặt đích cho agent nếu controller chưa có
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(bestPos);
        }

        Debug.Log($"[AlarmNearestExit] Dẫn tới Exit gần nhất: {bestPoi.poiName} (~{bestDist:F1}m)");

        // 2) Bật Navigation UI nếu có
        var navUI = GameObject.Find("NavigationUI");
        if (navUI != null && !navUI.activeSelf)
            navUI.SetActive(true);
    }
}