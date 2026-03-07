// PlayerInteractor.cs
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private bool requireFacing = true;      // Stardew-like: must face the NPC
    [SerializeField] private float interactDistance = 1.2f;   // Used if requireFacing = true
    [SerializeField] private float interactRadius = 1.2f;     // Used if requireFacing = false
    [SerializeField] private Vector2 originOffset = new Vector2(0f, 0.1f);

    [Header("Optional: Tile fallback (your existing tile interaction)")]
    [SerializeField] private bool fallbackToTiles = true;
    [SerializeField] private TileManager tileManager;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!InputManager.InteractPressed) return;

        
        if (TryInteractWithWorldObject())
            return;

        
        if (fallbackToTiles && tileManager != null)
        {
            Vector3Int cellPos = tileManager.WorldToCell(transform.position);
            if (tileManager.IsInteractable(cellPos))
            {
                Debug.Log("Interacted with tile at " + cellPos);
                tileManager.SetInteracted(cellPos);
            }
        }
    }

    private bool TryInteractWithWorldObject()
    {
        Vector2 origin = (Vector2)transform.position + originOffset;

        if (requireFacing)
        {
            Vector2 dir = playerMovement != null ? playerMovement.LastMotionVector.normalized : Vector2.down;
            if (dir == Vector2.zero) dir = Vector2.down;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, interactDistance, interactableLayers);
            if (hit.collider == null) return false;

            if (TryGetInteractable(hit.collider, out IInteractable interactable) && interactable.CanInteract())
            {
                interactable.Interact();
                return true;
            }

            return false;
        }
        else
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, interactRadius, interactableLayers);
            if (hits == null || hits.Length == 0) return false;

            // Pick the closest interactable
            IInteractable best = null;
            float bestDist = float.PositiveInfinity;

            foreach (var col in hits)
            {
                if (!TryGetInteractable(col, out IInteractable interactable)) continue;
                if (!interactable.CanInteract()) continue;

                float d = Vector2.Distance(origin, col.ClosestPoint(origin));
                if (d < bestDist)
                {
                    bestDist = d;
                    best = interactable;
                }
            }

            if (best != null)
            {
                best.Interact();
                return true;
            }

            return false;
        }
    }

    // Robust: works even if Unity version doesn't support GetComponent<IInterface>()
    private bool TryGetInteractable(Collider2D col, out IInteractable interactable)
    {
        interactable = null;

        // Look on this collider and parents (NPC colliders often live on child objects)
        var behaviours = col.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b is IInteractable i)
            {
                interactable = i;
                return true;
            }
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + originOffset;

        if (requireFacing)
        {
            Vector2 dir = Vector2.down;
            var pm = GetComponent<PlayerMovement>();
            if (pm != null && pm.LastMotionVector != Vector2.zero) dir = pm.LastMotionVector.normalized;

            Gizmos.DrawLine(origin, origin + dir * interactDistance);
        }
        else
        {
            Gizmos.DrawWireSphere(origin, interactRadius);
        }
    }
#endif
}