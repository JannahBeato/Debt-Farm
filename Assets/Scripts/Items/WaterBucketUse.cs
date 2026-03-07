using UnityEngine;

public class WaterBucketUse : MonoBehaviour, IItemUse
{
    [SerializeField] private int energyCost = 1;

    public int EnergyCost => energyCost;
    public bool ConsumesItem => false;

    public bool TryUse(UseContext context)
    {
        if (context.TileManager == null) return false;

        Vector3Int cell = context.TileManager.WorldToCell(context.ToolOrigin);

        if (!context.TileManager.IsInteractable(cell)) return false;

        // only water soil
        string state = context.TileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;

        bool ok = context.TileManager.TrySetState(cell, "watered");
        if (!ok) return false;

        // if crop exists there, mark wateredToday
        context.CropManager?.Water(cell);

        return true;
    }
}