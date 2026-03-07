using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject cropVisualPrefab;

    // Add every crop definition you want to be loadable here
    [SerializeField] private List<CropDefinitionSO> cropDefinitions = new();

    // Assign your potato WORLD pickup prefab here (must be tagged "Item")
    [SerializeField] private GameObject potatoDropPrefab;

    private class CropInstance
    {
        public CropDefinitionSO def;
        public GameObject visual;
        public int stage;
        public bool wateredToday;
    }

    private readonly Dictionary<Vector3Int, CropInstance> crops = new();
    private readonly Dictionary<string, CropDefinitionSO> cropLookup = new();

    private void Awake()
    {
        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();
        RebuildCropLookup();
    }

    private void RebuildCropLookup()
    {
        cropLookup.Clear();

        foreach (var def in cropDefinitions)
        {
            if (def == null) continue;

            if (!cropLookup.ContainsKey(def.name))
                cropLookup.Add(def.name, def);
        }
    }

    public bool HasCrop(Vector3Int cell) => crops.ContainsKey(cell);

    public bool Plant(CropDefinitionSO def, Vector3Int cell)
    {
        if (def == null || tileManager == null) return false;
        if (HasCrop(cell)) return false;

        string state = tileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;

        GameObject v = CreateCropVisual(def, cell, 0);

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

    public bool TryHarvest(Vector3Int cell)
    {
        if (!crops.TryGetValue(cell, out var c)) return false;

        int maxStage = (c.def.stageSprites == null) ? 0 : Mathf.Max(0, c.def.stageSprites.Length - 1);
        if (c.stage < maxStage) return false;

        if (c.visual != null) Destroy(c.visual);
        crops.Remove(cell);

        if (tileManager != null)
            tileManager.TrySetState(cell, "soil");

        if (potatoDropPrefab != null && tileManager != null)
        {
            Vector3 dropPos = tileManager.GetCellCenterWorld(cell);
            dropPos.z = 0f;

            GameObject drop = Instantiate(potatoDropPrefab, dropPos, Quaternion.identity);
            drop.tag = "Item";

            if (drop.GetComponent<Item>() == null)
                Debug.LogWarning("Dropped potato has no Item component. Add Item script to the potato prefab/variant.");

            Collider2D col = drop.GetComponent<Collider2D>();
            if (col == null) col = drop.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb == null) rb = drop.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
        else if (potatoDropPrefab == null)
        {
            Debug.LogWarning("CropManager: potatoDropPrefab is not assigned, so no potato will drop.");
        }

        return true;
    }

    public void AdvanceDay()
    {
        foreach (var kv in crops)
        {
            CropInstance c = kv.Value;

            int maxStage = (c.def.stageSprites == null) ? 0 : Mathf.Max(0, c.def.stageSprites.Length - 1);

            if (c.wateredToday && c.stage < maxStage)
            {
                c.stage++;
                ApplyStageSprite(c);
            }

            c.wateredToday = false;
        }
    }

    public List<CropSaveData> GetSavedCrops()
    {
        List<CropSaveData> result = new();

        foreach (var kv in crops)
        {
            Vector3Int cell = kv.Key;
            CropInstance crop = kv.Value;

            if (crop == null || crop.def == null) continue;

            result.Add(new CropSaveData
            {
                x = cell.x,
                y = cell.y,
                z = cell.z,
                cropDefinitionName = crop.def.name,
                stage = crop.stage,
                wateredToday = crop.wateredToday
            });
        }

        return result;
    }

    public void LoadSavedCrops(List<CropSaveData> savedCrops)
    {
        ClearAllCrops();
        RebuildCropLookup();

        if (savedCrops == null || savedCrops.Count == 0)
            return;

        foreach (var data in savedCrops)
        {
            if (data == null || string.IsNullOrEmpty(data.cropDefinitionName))
                continue;

            if (!cropLookup.TryGetValue(data.cropDefinitionName, out var def) || def == null)
            {
                Debug.LogWarning($"CropManager: Could not find CropDefinitionSO named '{data.cropDefinitionName}'. Add it to cropDefinitions.");
                continue;
            }

            Vector3Int cell = new Vector3Int(data.x, data.y, data.z);
            int clampedStage = GetClampedStage(def, data.stage);

            GameObject v = CreateCropVisual(def, cell, clampedStage);

            crops[cell] = new CropInstance
            {
                def = def,
                visual = v,
                stage = clampedStage,
                wateredToday = data.wateredToday
            };
        }
    }

    private void ClearAllCrops()
    {
        foreach (var kv in crops)
        {
            if (kv.Value != null && kv.Value.visual != null)
                Destroy(kv.Value.visual);
        }

        crops.Clear();
    }

    private GameObject CreateCropVisual(CropDefinitionSO def, Vector3Int cell, int stage)
    {
        Vector3 world = tileManager.GetCellCenterWorld(cell);
        GameObject v = Instantiate(cropVisualPrefab, world, Quaternion.identity, transform);

        var sr = v.GetComponent<SpriteRenderer>();
        if (sr != null && def.stageSprites != null && def.stageSprites.Length > 0)
        {
            int clampedStage = GetClampedStage(def, stage);
            sr.sprite = def.stageSprites[clampedStage];
        }

        var col = v.GetComponent<Collider2D>();
        if (col == null)
        {
            var box = v.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }

        var clickable = v.GetComponent<CropClickable>();
        if (clickable == null) clickable = v.AddComponent<CropClickable>();
        clickable.Init(this, cell);

        return v;
    }

    private void ApplyStageSprite(CropInstance crop)
    {
        if (crop == null || crop.visual == null || crop.def == null) return;

        var sr = crop.visual.GetComponent<SpriteRenderer>();
        if (sr == null || crop.def.stageSprites == null || crop.def.stageSprites.Length == 0) return;

        int clampedStage = GetClampedStage(crop.def, crop.stage);
        sr.sprite = crop.def.stageSprites[clampedStage];
    }

    private int GetClampedStage(CropDefinitionSO def, int stage)
    {
        if (def == null || def.stageSprites == null || def.stageSprites.Length == 0)
            return 0;

        return Mathf.Clamp(stage, 0, def.stageSprites.Length - 1);
    }
}