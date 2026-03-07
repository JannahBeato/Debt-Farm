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
        // Don’t use tools while clicking UI (dragging items, menus, etc.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (hotbarController == null) return;

        // 1) No selected item -> do nothing (this solves your “no item = cannot click”)
        GameObject selectedItemGO = hotbarController.GetSelectedItemObject();
        if (selectedItemGO == null) return;

        // 2) Selected item must have a “use” component
        IItemUse usable = selectedItemGO.GetComponent<IItemUse>();
        if (usable == null) return;

        Vector2 facing = character.LastMotionVector;
        if (facing.sqrMagnitude < 0.001f)
            facing = Vector2.down;

        Vector2 origin = rgbd2d.position + facing.normalized * offsetDistance;

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

        // 3) If too tired -> do nothing (no energy spent)
        if (cost > 0 && energy.CurrentEnergy < cost)
            return;

        // 4) Try to use. If it FAILS -> no energy spent.
        bool success = usable.TryUse(ctx);
        if (!success) return;

        // 5) Success -> now spend energy
        if (cost > 0)
            energy.TrySpend(cost);

        // 6) If it consumes the item (seeds) -> remove from hotbar
        if (usable.ConsumesItem)
            hotbarController.ConsumeSelectedItem();
    }
}