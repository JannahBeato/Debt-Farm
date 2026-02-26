using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject cropVisualPrefab; 

    private class CropInstance
    {
        public CropDefinitionSO def;
        public GameObject visual;
        public int stage;           
        public bool wateredToday;
    }

    private readonly Dictionary<Vector3Int, CropInstance> crops = new();

    private void Awake()
    {
        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();
    }

    public bool HasCrop(Vector3Int cell) => crops.ContainsKey(cell);

    public bool Plant(CropDefinitionSO def, Vector3Int cell)
    {
        if (def == null || tileManager == null) return false;
        if (cropVisualPrefab == null)
        {
            Debug.LogError("CropManager: cropVisualPrefab is not assigned.");
            return false;
        }
        if (def.stageSprites == null || def.stageSprites.Length == 0)
        {
            Debug.LogError($"CropManager: CropDefinitionSO '{def.cropId}' has no stageSprites.");
            return false;
        }

        if (HasCrop(cell)) return false;

        string state = tileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;
        
        Vector3 world = tileManager.GetCellCenterWorld(cell);
        GameObject v = Instantiate(cropVisualPrefab, world, Quaternion.identity, transform);

        var sr = v.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("CropManager: cropVisualPrefab is missing a SpriteRenderer component.");
            Destroy(v);
            return false;
        }

        sr.sprite = def.stageSprites[0];

        var hit = v.GetComponent<CropHarvestHit>();
        if (hit == null) hit = v.AddComponent<CropHarvestHit>();
        hit.Init(this, cell);

        crops[cell] = new CropInstance
        {
            def = def,
            visual = v,
            stage = 0,
            wateredToday = (state == "watered")
        };

        return true;

    }

    public void Water(Vector3Int cell)
    {
        if (crops.TryGetValue(cell, out var crop))
            crop.wateredToday = true;
    }

    public void AdvanceDay()
    {
        foreach (var kv in crops)
        {
            CropInstance c = kv.Value;

            int maxStage = (c.def.stageSprites != null) ? c.def.stageSprites.Length - 1 : 0;

            if (c.wateredToday && c.stage < maxStage)
            {
                c.stage++;
                var sr = c.visual.GetComponent<SpriteRenderer>();
                if (sr != null && c.def.stageSprites != null && c.stage < c.def.stageSprites.Length)
                    sr.sprite = c.def.stageSprites[c.stage];
            }

            c.wateredToday = false;
        }
    }

    public bool TryHarvest(Vector3Int cell)
    {
        if (!crops.TryGetValue(cell, out var c)) return false;

        int maxStage = (c.def.stageSprites != null) ? c.def.stageSprites.Length - 1 : 0;
        if (c.stage < maxStage) return false; // not grown yet

        if (c.def.harvestItemPrefab == null)
        {
            Debug.LogWarning($"Crop {c.def.cropId} has no harvestItemPrefab assigned.");
            return false;
        }

        var inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null)
        {
            Debug.LogError("CropManager: No InventoryController found in scene.");
            return false;
        }

        // add harvestAmount items
        for (int i = 0; i < Mathf.Max(1, c.def.harvestAmount); i++)
        {
            bool added = inventory.AddItem(c.def.harvestItemPrefab);
            if (!added) return false; // inventory full -> stop
        }

        // remove crop from world + dictionary
        Destroy(c.visual);
        crops.Remove(cell);

        // optional: after plucking, keep soil (and remove watered look)
        tileManager.TrySetState(cell, "soil");

        return true;
    }
}