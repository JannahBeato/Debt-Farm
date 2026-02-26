using UnityEngine;

public class CropHarvestHit : ToolHit
{
    private CropManager manager;
    private Vector3Int cell;

    public void Init(CropManager m, Vector3Int c)
    {
        manager = m;
        cell = c;
    }

    // NEW: lets caller know if harvest succeeded
    public bool TryHarvest()
    {
        return manager != null && manager.TryHarvest(cell);
    }

    public override void Hit()
    {
        TryHarvest();
    }
}