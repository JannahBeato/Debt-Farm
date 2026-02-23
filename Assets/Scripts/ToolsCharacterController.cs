using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody2D))]
public class ToolsCharacterController : MonoBehaviour
{
    private PlayerMovement character;
    private Rigidbody2D rgbd2d;

    [SerializeField] private float offsetDistance = 1f;
    [SerializeField] private float sizeOfInteractableArea = 1.2f;

    private void Awake()
    {
        character = GetComponent<PlayerMovement>();
        rgbd2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // ✅ NEW input system mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            UseTool();
        }
    }

    private void UseTool()
    {
        Vector2 facing = character.LastMotionVector;
        Vector2 position = rgbd2d.position + facing * offsetDistance;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, sizeOfInteractableArea);

        foreach (Collider2D c in colliders)
        {
            ToolHit hit = c.GetComponent<ToolHit>();
            if (hit != null)
            {
                hit.Hit();
                break;
            }
        }
    }
}
