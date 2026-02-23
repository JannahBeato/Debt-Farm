using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerEnergy))]
public class ToolsCharacterController : MonoBehaviour
{
    private PlayerMovement character;
    private Rigidbody2D rgbd2d;
    private PlayerEnergy energy;

    [SerializeField] private int energyCostPerUse = 2;
    [SerializeField] private float offsetDistance = 1f;
    [SerializeField] private float sizeOfInteractableArea = 1.2f;

    private void Awake()
    {
        character = GetComponent<PlayerMovement>();
        rgbd2d = GetComponent<Rigidbody2D>();
        energy = GetComponent<PlayerEnergy>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            UseTool();
    }

    private void UseTool()
    {
        // No energy? No action.
        if (!energy.TrySpend(energyCostPerUse))
        {
            // Optional: play "too tired" sound / UI popup here
            return;
        }

        Vector2 facing = character.LastMotionVector;

        // Safety: if LastMotionVector can ever be (0,0), pick a default so tools still work.
        if (facing.sqrMagnitude < 0.001f)
            facing = Vector2.down;

        Vector2 position = rgbd2d.position + facing.normalized * offsetDistance;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, sizeOfInteractableArea);

        foreach (Collider2D c in colliders)
        {
            ToolHit hit = c.GetComponent<ToolHit>();
            if (hit != null)
            {
                hit.Hit();
                return;
            }
        }

        // Optional: If you only want to spend energy when something is actually hit,
        // then move TrySpend() down into the "hit != null" branch instead.
    }
}