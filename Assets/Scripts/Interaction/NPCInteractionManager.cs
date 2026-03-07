using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class NPCInteractionManager : MonoBehaviour
{
    [Header("NPC Detection")]
    [SerializeField] private LayerMask npcLayerMask;     // Set this to your NPC layer
    [SerializeField] private float interactDistance = 1.2f;
    [SerializeField] private Vector2 originOffset = new Vector2(0f, 0.15f);

    [Header("Fallback (helps with bigger NPC colliders)")]
    [SerializeField] private float overlapRadius = 0.5f;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // NPC-only button: E
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        TryInteractWithNpc();
    }

    private void TryInteractWithNpc()
    {
        Vector2 origin = (Vector2)transform.position + originOffset;

        Vector2 dir = playerMovement != null ? playerMovement.LastMotionVector.normalized : Vector2.down;
        if (dir == Vector2.zero) dir = Vector2.down;

        // 1) Raycast in front (Stardew-like)
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, interactDistance, npcLayerMask);
        if (hit.collider != null && TryGetInteractable(hit.collider, out var interactable) && interactable.CanInteract())
        {
            interactable.Interact();
            return;
        }

        // 2) Small overlap circle slightly in front (more forgiving)
        Vector2 checkPos = origin + dir * Mathf.Min(interactDistance, 0.8f);
        Collider2D[] results = new Collider2D[8];
        int count = Physics2D.OverlapCircleNonAlloc(checkPos, overlapRadius, results, npcLayerMask);

        IInteractable best = null;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var col = results[i];
            if (col == null) continue;

            if (!TryGetInteractable(col, out var candidate)) continue;
            if (!candidate.CanInteract()) continue;

            float d = Vector2.Distance(origin, col.ClosestPoint(origin));
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }

        if (best != null)
            best.Interact();
    }

    // Works even if the collider is on a child object of the NPC
    private bool TryGetInteractable(Collider2D col, out IInteractable interactable)
    {
        interactable = null;

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
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();

        Vector2 origin = (Vector2)transform.position + originOffset;
        Vector2 dir = (playerMovement != null && playerMovement.LastMotionVector != Vector2.zero)
            ? playerMovement.LastMotionVector.normalized
            : Vector2.down;

        Gizmos.DrawLine(origin, origin + dir * interactDistance);
        Gizmos.DrawWireSphere(origin + dir * Mathf.Min(interactDistance, 0.8f), overlapRadius);
    }
#endif
}