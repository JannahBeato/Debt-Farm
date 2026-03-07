using System.Collections;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [SerializeField] private EndOfDayService endOfDay;

    [Header("Camera Fix (Confiner)")]
    [SerializeField] private bool fixCameraAfterSleepOrPassOut = true;
    [SerializeField] private int refreshAfterFrames = 2; // wait a couple frames for respawn/teleport to finish
    [SerializeField] private string playerTag = "Player";

    private CinemachineConfiner2D _confiner;
    private MethodInfo _invalidateMethod;

    private void Awake()
    {
        if (endOfDay == null) endOfDay = FindFirstObjectByType<EndOfDayService>();

        if (fixCameraAfterSleepOrPassOut)
        {
            _confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            CacheInvalidateMethod();
        }
    }

    public void GoToNextDay()
    {
        if (endOfDay == null)
        {
            Debug.LogError("SleepManager: EndOfDayService missing.");
            return;
        }

        endOfDay.SleepNow();

        // If sleep/pass-out teleports the player without crossing a waypoint trigger,
        // the confiner boundary might still be set to the "old" frame.
        if (fixCameraAfterSleepOrPassOut)
            StartCoroutine(RefreshCameraConfinerAfterRespawn());
    }

    private IEnumerator RefreshCameraConfinerAfterRespawn()
    {
        // Wait a couple frames so whatever service teleports/respawns the player finishes first
        int frames = Mathf.Max(1, refreshAfterFrames);
        for (int i = 0; i < frames; i++)
            yield return null;

        ForceConfinerToPlayerArea();
    }

    private void ForceConfinerToPlayerArea()
    {
        if (_confiner == null) _confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (_confiner == null) return;

        CacheInvalidateMethod();

        Transform player = FindPlayer();
        if (player == null) return;

        // Make sure physics/transform state is up to date before OverlapPoint checks
        Physics2D.SyncTransforms();

        PolygonCollider2D bestBoundary = FindBestBoundaryAt(player.position);
        if (bestBoundary == null) return;

        _confiner.BoundingShape2D = bestBoundary;

        // IMPORTANT: confiner must rebuild its cache when boundary changes at runtime
        _invalidateMethod?.Invoke(_confiner, null);
    }

    private Transform FindPlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag(playerTag);
        return go != null ? go.transform : null;
    }

    private PolygonCollider2D FindBestBoundaryAt(Vector2 worldPos)
    {
        // Pick the smallest PolygonCollider2D that contains the player.
        // This avoids accidentally selecting a huge "world" collider when multiple overlap.
        PolygonCollider2D best = null;
        float bestArea = float.PositiveInfinity;

        var polys = FindObjectsOfType<PolygonCollider2D>();
        for (int i = 0; i < polys.Length; i++)
        {
            var p = polys[i];
            if (p == null) continue;
            if (!p.enabled) continue;
            if (!p.gameObject.activeInHierarchy) continue;

            if (!p.OverlapPoint(worldPos)) continue;

            Vector2 size = p.bounds.size;
            float area = size.x * size.y;

            if (area < bestArea)
            {
                bestArea = area;
                best = p;
            }
        }

        return best;
    }

    private void CacheInvalidateMethod()
    {
        if (_confiner == null) return;

        // Cinemachine 3: InvalidateCache()
        // Cinemachine 2: InvalidatePathCache()
        var t = _confiner.GetType();
        _invalidateMethod =
            t.GetMethod("InvalidateCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? t.GetMethod("InvalidatePathCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}