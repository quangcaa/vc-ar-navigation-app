using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Generates spoken navigation instructions by watching the NavMesh path.
/// </summary>
[RequireComponent(typeof(VoicePromptPlayer))]
public class VoiceNavigationGuide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavigationController navigationController;
    [SerializeField] private VoicePromptPlayer promptPlayer;

    [Header("Thresholds (meters)")]
    [SerializeField] private float turnWarningDistance = 2f;
    [SerializeField] private float turnCompletionDistance = 0.4f;
    [SerializeField] private float straightAnnouncementDelta = 1f;

    [Header("Turn Angle Thresholds (deg)")]
    [SerializeField] private float slightTurnThreshold = 3f;
    [SerializeField] private float veerThreshold = 45f;

    [Header("Path Calculation")]
    [SerializeField] private float pathUpdateFrequency = 0.5f;

    [Header("Corner Grouping")]
    [Tooltip("Corners within this distance are grouped as one turn")]
    [SerializeField] private float cornerGroupingDistance = 1f;

    [Header("Announcement Timing")]
    [Tooltip("Minimum time (seconds) to wait after a turn announcement before saying 'Continue straight'")]
    [SerializeField] private float postTurnAnnouncementDelay = 3f;

    private POI activeDestination;
    private int nextCornerIndex = 1;
    private int currentGroupEndIndex = -1; // Track end of current corner group
    private bool awaitingCornerCompletion;
    private bool pendingInitialAnnouncement;
    private float lastStraightAnnouncement = -1f;

    // Self-calculated path (same approach as ShowPath)
    private NavMeshPath calculatedPath;
    private float pathUpdateTimer = 0f;

    // Timer to prevent "Continue straight" from interrupting turn announcements
    private float lastTurnAnnouncementTime = -999f;

    // Track previous path to detect significant changes
    private int previousCornerCount = 0;
    private Vector3 previousFirstCorner = Vector3.zero;
    private Vector3 previousLastCorner = Vector3.zero;

    private void Awake()
    {
        if (promptPlayer == null)
        {
            promptPlayer = GetComponent<VoicePromptPlayer>();
        }
        calculatedPath = new NavMeshPath();
    }

    private void OnEnable()
    {
        if (navigationController == null)
        {
            navigationController = NavigationController.instance;
        }

        if (navigationController != null)
        {
            navigationController.DestinationArrived.AddListener(OnDestinationArrived);
        }
    }

    private void OnDisable()
    {
        if (navigationController != null)
        {
            navigationController.DestinationArrived.RemoveListener(OnDestinationArrived);
        }
    }

    private void Update()
    {
        if (navigationController == null || promptPlayer == null)
        {
            Debug.LogWarning("[VoiceNavigationGuide] Missing references. NavigationController:"
                + (navigationController ? navigationController.name : "null")
                + ", PromptPlayer:"
                + (promptPlayer ? promptPlayer.name : "null"));
            return;
        }

        DetectDestinationChange();

        if (!navigationController.IsCurrentlyNavigating())
        {
            return;
        }

        var agent = navigationController.agent;
        if (agent == null || activeDestination == null || activeDestination.poiCollider == null)
        {
            return;
        }

        // Recalculate path periodically (same approach as ShowPath)
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateFrequency)
        {
            pathUpdateTimer = 0f;
            NavMesh.CalculatePath(
                agent.transform.position,
                activeDestination.poiCollider.transform.position,
                NavMesh.AllAreas,
                calculatedPath
            );

            // Check if path changed significantly and reset corner tracking
            DetectPathChangeAndReset(calculatedPath.corners);
        }

        var corners = calculatedPath.corners;
        if (corners == null || corners.Length < 2)
        {
            return;
        }

        if (pendingInitialAnnouncement)
        {
            AnnounceStraightDistance(corners);
            pendingInitialAnnouncement = false;
        }

        // Ensure nextCornerIndex is valid for current path
        nextCornerIndex = Mathf.Clamp(nextCornerIndex, 1, Mathf.Max(1, corners.Length - 1));

        if (!awaitingCornerCompletion && nextCornerIndex < corners.Length - 1)
        {
            float distanceToCorner = Vector3.Distance(agent.transform.position, corners[nextCornerIndex]);
            if (distanceToCorner <= turnWarningDistance)
            {
                Debug.Log($"[VoiceGuide] Near corner {nextCornerIndex}, distance: {distanceToCorner:F2}m");

                // Find the end of the corner group (multiple corners close together)
                int groupEnd = FindCornerGroupEnd(corners, nextCornerIndex);
                Debug.Log($"[VoiceGuide] Corner group: {nextCornerIndex} to {groupEnd}");

                if (SpeakTurnInstructionForGroup(corners, nextCornerIndex, groupEnd))
                {
                    currentGroupEndIndex = groupEnd;
                    awaitingCornerCompletion = true;
                }
                else
                {
                    // No meaningful turn, skip entire group.
                    Debug.Log($"[VoiceGuide] No announcement, skipping to corner {groupEnd + 1}");
                    nextCornerIndex = groupEnd + 1;
                }
            }
        }
        else if (awaitingCornerCompletion)
        {
            // Check if we've passed the last corner in the group
            int checkIndex = currentGroupEndIndex >= 0 ? currentGroupEndIndex : nextCornerIndex;
            float distance = Vector3.Distance(agent.transform.position, corners[Mathf.Min(checkIndex, corners.Length - 1)]);

            if (distance <= turnCompletionDistance)
            {
                awaitingCornerCompletion = false;
                nextCornerIndex = Mathf.Min(checkIndex + 1, corners.Length - 1);
                currentGroupEndIndex = -1;
                AnnounceStraightDistance(corners);
            }
        }
    }

    private void DetectDestinationChange()
    {
        var currentPoi = navigationController.currentDestination;
        if (currentPoi == activeDestination)
        {
            return;
        }

        activeDestination = currentPoi;
        nextCornerIndex = 1;
        currentGroupEndIndex = -1;
        awaitingCornerCompletion = false;
        pendingInitialAnnouncement = activeDestination != null;
        lastStraightAnnouncement = -1f;
        lastTurnAnnouncementTime = -999f; // Allow initial straight announcement immediately

        // Reset path tracking
        previousCornerCount = 0;
        previousFirstCorner = Vector3.zero;
        previousLastCorner = Vector3.zero;

        if (activeDestination != null)
        {
            // Force immediate path calculation when destination changes
            var agent = navigationController.agent;
            if (agent != null && activeDestination.poiCollider != null)
            {
                NavMesh.CalculatePath(
                    agent.transform.position,
                    activeDestination.poiCollider.transform.position,
                    NavMesh.AllAreas,
                    calculatedPath
                );
                pathUpdateTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Detects if path has changed significantly and resets corner tracking if needed.
    /// </summary>
    private void DetectPathChangeAndReset(Vector3[] corners)
    {
        if (corners == null || corners.Length < 2)
        {
            return;
        }

        bool pathChanged = false;

        // Check if corner count changed
        if (corners.Length != previousCornerCount)
        {
            pathChanged = true;
            Debug.Log($"[VoiceGuide] Path changed: corner count {previousCornerCount} -> {corners.Length}");
        }
        // Check if first corner moved significantly (agent moved past old corners)
        else if (previousCornerCount > 0 && Vector3.Distance(corners[0], previousFirstCorner) > 1f)
        {
            pathChanged = true;
            Debug.Log($"[VoiceGuide] Path changed: first corner moved");
        }
        // Check if path structure changed (last corner before destination moved)
        else if (corners.Length > 1 && Vector3.Distance(corners[corners.Length - 1], previousLastCorner) > 1f)
        {
            pathChanged = true;
            Debug.Log($"[VoiceGuide] Path changed: destination corner moved");
        }

        if (pathChanged)
        {
            // Reset corner tracking but don't re-announce if we're mid-navigation
            if (!awaitingCornerCompletion)
            {
                // Find the nearest upcoming corner based on agent position
                var agentPos = navigationController.agent.transform.position;
                int newNextCorner = 1;

                for (int i = 1; i < corners.Length - 1; i++)
                {
                    float distToCorner = Vector3.Distance(agentPos, corners[i]);
                    if (distToCorner > turnWarningDistance)
                    {
                        newNextCorner = i;
                        break;
                    }
                    newNextCorner = i + 1;
                }

                nextCornerIndex = Mathf.Clamp(newNextCorner, 1, corners.Length - 1);
                Debug.Log($"[VoiceGuide] Reset nextCornerIndex to {nextCornerIndex}");
            }
            currentGroupEndIndex = -1;
        }

        // Update tracking for next comparison
        previousCornerCount = corners.Length;
        if (corners.Length > 0)
        {
            previousFirstCorner = corners[0];
            previousLastCorner = corners[corners.Length - 1];
        }
    }

    private void AnnounceStraightDistance(IReadOnlyList<Vector3> corners)
    {
        if (activeDestination == null || corners == null || corners.Count < 2)
        {
            return;
        }

        // Don't interrupt turn announcement - wait for delay to pass
        if (Time.time - lastTurnAnnouncementTime < postTurnAnnouncementDelay)
        {
            return;
        }

        var agentPos = navigationController.agent.transform.position;

        // Calculate distance to next corner (next turn), not to final destination
        int targetCornerIndex = Mathf.Min(nextCornerIndex, corners.Count - 1);
        float distanceToNextCorner = Vector3.Distance(agentPos, corners[targetCornerIndex]);

        if (distanceToNextCorner <= 0.5f)
        {
            return;
        }

        if (lastStraightAnnouncement < 0 || Mathf.Abs(distanceToNextCorner - lastStraightAnnouncement) >= straightAnnouncementDelta)
        {
            lastStraightAnnouncement = distanceToNextCorner;
            int rounded = Mathf.Max(1, Mathf.RoundToInt(distanceToNextCorner));
            promptPlayer.Speak($"Continue straight for {rounded} meters.");
        }
    }

    /// <summary>
    /// Finds the last corner index in a group of closely-spaced corners.
    /// Corners within cornerGroupingDistance of each other are considered one group.
    /// </summary>
    private int FindCornerGroupEnd(IReadOnlyList<Vector3> corners, int startIndex)
    {
        int endIndex = startIndex;

        while (endIndex < corners.Count - 2)
        {
            float distToNext = Vector3.Distance(corners[endIndex], corners[endIndex + 1]);
            if (distToNext > cornerGroupingDistance)
            {
                break;
            }
            endIndex++;
        }

        return endIndex;
    }

    /// <summary>
    /// Speaks turn instruction for a group of corners (handles multiple close corners as one turn).
    /// </summary>
    private bool SpeakTurnInstructionForGroup(IReadOnlyList<Vector3> corners, int groupStartIndex, int groupEndIndex)
    {
        Debug.Log($"[VoiceGuide] Checking turn at corners[{groupStartIndex}] to corners[{groupEndIndex}], total corners: {corners.Count}");

        if (groupStartIndex <= 0 || groupEndIndex >= corners.Count - 1)
        {
            Debug.Log($"[VoiceGuide] Skipped: boundary check failed (start={groupStartIndex}, end={groupEndIndex}, count={corners.Count})");
            return false;
        }

        // Calculate overall turn direction: from point BEFORE first corner TO point AFTER last corner
        Vector3 entryPoint = corners[groupStartIndex - 1];
        Vector3 firstCorner = corners[groupStartIndex];
        Vector3 lastCorner = corners[groupEndIndex];
        Vector3 exitPoint = corners[groupEndIndex + 1];

        // Direction entering the group (before -> first corner)
        Vector2 entryDir = new Vector2(firstCorner.x - entryPoint.x, firstCorner.z - entryPoint.z).normalized;
        // Direction exiting the group (last corner -> after)
        Vector2 exitDir = new Vector2(exitPoint.x - lastCorner.x, exitPoint.z - lastCorner.z).normalized;

        if (entryDir.sqrMagnitude <= Mathf.Epsilon || exitDir.sqrMagnitude <= Mathf.Epsilon)
        {
            Debug.Log($"[VoiceGuide] Skipped: direction vector too small");
            return false;
        }

        // Calculate signed angle on XZ plane using cross product
        float cross = entryDir.x * exitDir.y - entryDir.y * exitDir.x;
        float dot = Vector2.Dot(entryDir, exitDir);
        float angleRad = Mathf.Atan2(cross, dot);
        float angle = angleRad * Mathf.Rad2Deg;
        float absAngle = Mathf.Abs(angle);

        Debug.Log($"[VoiceGuide] Angle calculated: {angle:F1}° (abs: {absAngle:F1}°), threshold: {slightTurnThreshold}°");

        if (absAngle <= slightTurnThreshold)
        {
            Debug.Log($"[VoiceGuide] Skipped: angle {absAngle:F1}° <= threshold {slightTurnThreshold}° (treated as straight)");
            return false; // Treat as straight.
        }

        // cross > 0 means exitDir is to the LEFT of entryDir
        string direction = cross > 0 ? "left" : "right";

        // Calculate distance to first corner in group
        float distanceToTurn = Vector3.Distance(navigationController.agent.transform.position, firstCorner);
        int roundedDistance = Mathf.Max(1, Mathf.RoundToInt(distanceToTurn));

        string message;

        if (absAngle < veerThreshold)
        {
            message = $"Veer {direction} in {roundedDistance} meters.";
            Debug.Log($"[VoiceGuide] Announcing: VEER {direction} in {roundedDistance}m ({absAngle:F1}° < {veerThreshold}°)");
        }
        else
        {
            message = $"Turn {direction} in {roundedDistance} meters.";
            Debug.Log($"[VoiceGuide] Announcing: TURN {direction} in {roundedDistance}m ({absAngle:F1}° >= {veerThreshold}°)");
        }

        promptPlayer.Speak(message);
        lastTurnAnnouncementTime = Time.time;
        return true;
    }

    private static float ComputeRemainingDistance(Vector3 agentPos, IReadOnlyList<Vector3> corners, int startCorner)
    {
        if (corners == null || corners.Count == 0)
        {
            return 0f;
        }

        float distance = 0f;
        distance += Vector3.Distance(agentPos, corners[startCorner]);

        for (int i = startCorner; i < corners.Count - 1; i++)
        {
            distance += Vector3.Distance(corners[i], corners[i + 1]);
        }

        return distance;
    }

    private void OnDestinationArrived()
    {
        if (activeDestination != null)
        {
            promptPlayer.Speak($"You have arrived at {activeDestination.poiName}.");
        }

        activeDestination = null;
        awaitingCornerCompletion = false;
        lastStraightAnnouncement = -1f;
        lastTurnAnnouncementTime = -999f;
        nextCornerIndex = 1;
        currentGroupEndIndex = -1;
        pathUpdateTimer = 0f;

        // Reset path tracking
        previousCornerCount = 0;
        previousFirstCorner = Vector3.zero;
        previousLastCorner = Vector3.zero;
    }
}

