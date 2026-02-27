using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class CropClickable : MonoBehaviour
{
    private CropManager cropManager;
    private Vector3Int cell;

    private Collider2D col;
    private Camera cam;

    public void Init(CropManager cropManager, Vector3Int cell)
    {
        this.cropManager = cropManager;
        this.cell = cell;
    }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (cropManager == null) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 screen = Mouse.current.position.ReadValue();

        // Convert to world point at the same Z plane as this crop
        float depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        world.z = transform.position.z;

        // If the click point is inside this crop collider, harvest it
        if (col.OverlapPoint(world))
        {
            cropManager.TryHarvest(cell);
        }
    }
}