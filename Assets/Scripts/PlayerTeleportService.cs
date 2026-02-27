using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerTeleportService : MonoBehaviour
{
    [Header("Camera Fix")]
    [SerializeField] private CinemachineConfiner2D _confiner; // optional, auto-found if null

    private MethodInfo _invalidateMethod;

    private void Awake()
    {
        if (_confiner == null) _confiner = FindObjectOfType<CinemachineConfiner2D>();

        CacheInvalidateMethod();
    }

    public void Teleport(Transform player, Transform target)
    {
        if (player == null || target == null) return;

        Vector3 oldPos = player.position;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = target.position;
            rb.rotation = target.eulerAngles.z;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            player.position = target.position;
            player.rotation = target.rotation;
        }

        // Ensure collider/physics positions update immediately (important for OverlapPoint checks)
        Physics2D.SyncTransforms();

        // Fix: after teleport, update the camera confiner boundary based on new position
        Vector3 newPos = player.position;
        UpdateConfinerBoundaryForPosition(newPos);

        // (Optional) if you want damping to not “drag”, you can later add a warp notify,
        // but the main bug you described is the confiner boundary staying on the old frame.
    }

    private void UpdateConfinerBoundaryForPosition(Vector2 worldPos)
    {
        if (_confiner == null) _confiner = FindObjectOfType<CinemachineConfiner2D>();
        if (_confiner == null) return;

        CacheInvalidateMethod();

        PolygonCollider2D best = FindBestBoundaryAt(worldPos);
        if (best == null) return;

        _confiner.BoundingShape2D = best;
        _invalidateMethod?.Invoke(_confiner, null);
    }

    private PolygonCollider2D FindBestBoundaryAt(Vector2 worldPos)
    {
        // Choose the "most specific" boundary that contains the point:
        // smallest bounds area that overlaps the position.
        var polys = FindObjectsOfType<PolygonCollider2D>();
        PolygonCollider2D best = null;
        float bestArea = float.PositiveInfinity;

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

        var t = _confiner.GetType();
        _invalidateMethod =
            t.GetMethod("InvalidateCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? t.GetMethod("InvalidatePathCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}