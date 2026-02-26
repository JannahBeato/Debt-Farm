using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerEnergy))]
public class ToolsCharacterController : MonoBehaviour
{
    private PlayerMovement character;
    private Rigidbody2D rgbd2d;
    private PlayerEnergy energy;

    [Header("Use Point")]
    [SerializeField] private float offsetDistance = 1f;
    [SerializeField] private float sizeOfInteractableArea = 1.2f;

    [Header("References")]
    [SerializeField] private HotbarController hotbarController;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private CropManager cropManager;

    private void Awake()
    {
        character = GetComponent<PlayerMovement>();
        rgbd2d = GetComponent<Rigidbody2D>();
        energy = GetComponent<PlayerEnergy>();

        if (hotbarController == null) hotbarController = FindFirstObjectByType<HotbarController>();
        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();
        if (cropManager == null) cropManager = FindFirstObjectByType<CropManager>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryUseSelectedItem();
    }

    private void TryUseSelectedItem()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (hotbarController == null) return;

        // Compute origin first
        Vector2 facing = character.LastMotionVector;
        if (facing.sqrMagnitude < 0.001f)
            facing = Vector2.down;

        Vector2 origin = rgbd2d.position + facing.normalized * offsetDistance;

        // Harvest first
        if (TryHarvestAt(origin))
            return;

        // Then tools/items
        GameObject selectedItemGO = hotbarController.GetSelectedItemObject();
        if (selectedItemGO == null) return;

        IItemUse usable = selectedItemGO.GetComponent<IItemUse>();
        if (usable == null) return;

        var ctx = new UseContext
        {
            Player = transform,
            Facing = facing.normalized,
            ToolOrigin = origin,
            ToolRadius = sizeOfInteractableArea,
            TileManager = tileManager,
            CropManager = cropManager
        };

        int cost = usable.EnergyCost;
        if (cost > 0 && energy.CurrentEnergy < cost) return;

        bool success = usable.TryUse(ctx);
        if (!success) return;

        if (cost > 0) energy.TrySpend(cost);

        if (usable.ConsumesItem)
            hotbarController.ConsumeSelectedItem();
    }

    // ✅ THIS MUST BE INSIDE THE CLASS
    private bool TryHarvestAt(Vector2 origin)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, sizeOfInteractableArea);

        foreach (Collider2D c in colliders)
        {
            // works even if collider is on a child
            CropHarvestHit harvest = c.GetComponent<CropHarvestHit>();
            if (harvest == null) harvest = c.GetComponentInParent<CropHarvestHit>();

            if (harvest != null)
            {
                harvest.Hit();
                return true;
            }
        }
        return false;
    }
}