using UnityEngine;

public class SeedUse : MonoBehaviour, IItemUse
{
    [SerializeField] private CropDefinitionSO crop;

    public int EnergyCost => 0;
    public bool ConsumesItem => true; // you said 1 per slot for now

    public bool TryUse(UseContext context)
    {
        if (context.TileManager == null || context.CropManager == null) return false;

        Vector3Int cell = context.TileManager.WorldToCell(context.ToolOrigin);

        // must be soil/watered
        string state = context.TileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;

        // no double plant
        if (context.CropManager.HasCrop(cell)) return false;

        return context.CropManager.Plant(crop, cell);
    }
}