using UnityEngine;

public class HoeUse : MonoBehaviour, IItemUse
{
    [SerializeField] private int energyCost = 2;

    public int EnergyCost => energyCost;
    public bool ConsumesItem => false;

    public bool TryUse(UseContext context)
    {
        if (context.TileManager == null) return false;

        Vector3Int cell = context.TileManager.WorldToCell(context.ToolOrigin);

        if (!context.TileManager.IsInteractable(cell)) return false;

        // only dirt -> soil
        if (context.TileManager.GetState(cell) != "dirt") return false;

        return context.TileManager.TrySetState(cell, "soil");
    }
}