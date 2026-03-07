using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject cropVisualPrefab;

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

    private void Awake()
    {
        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();
    }

    public bool HasCrop(Vector3Int cell) => crops.ContainsKey(cell);

    public bool Plant(CropDefinitionSO def, Vector3Int cell)
    {
        if (def == null || tileManager == null) return false;
        if (HasCrop(cell)) return false;

        string state = tileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;

        Vector3 world = tileManager.GetCellCenterWorld(cell);
        GameObject v = Instantiate(cropVisualPrefab, world, Quaternion.identity, transform);

        // Set initial sprite
        var sr = v.GetComponent<SpriteRenderer>();
        if (sr != null && def.stageSprites != null && def.stageSprites.Length > 0)
            sr.sprite = def.stageSprites[0];

        // Ensure it can be clicked
        var col = v.GetComponent<Collider2D>();
        if (col == null)
        {
            var box = v.AddComponent<BoxCollider2D>();
            box.isTrigger = true; // doesn’t block movement; set false if you want crops to block the player
        }

        var clickable = v.GetComponent<CropClickable>();
        if (clickable == null) clickable = v.AddComponent<CropClickable>();
        clickable.Init(this, cell);

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

    // Click harvest calls this
    public bool TryHarvest(Vector3Int cell)
    {
        if (!crops.TryGetValue(cell, out var c)) return false;

        int maxStage = (c.def.stageSprites == null) ? 0 : Mathf.Max(0, c.def.stageSprites.Length - 1);
        if (c.stage < maxStage) return false; // only harvest when fully grown

        // Remove crop visual + data
        if (c.visual != null) Destroy(c.visual);
        crops.Remove(cell);

        // Tile after harvest: back to soil
        if (tileManager != null)
            tileManager.TrySetState(cell, "soil");

        // Drop 1 potato pickup
        if (potatoDropPrefab != null && tileManager != null)
        {
            Vector3 dropPos = tileManager.GetCellCenterWorld(cell);
            dropPos.z = 0f;

            GameObject drop = Instantiate(potatoDropPrefab, dropPos, Quaternion.identity);

            // Ensure it's detectable by PlayerItemCollector
            drop.tag = "Item";

            // Ensure it has an Item component (your collector requires this)
            if (drop.GetComponent<Item>() == null)
                Debug.LogWarning("Dropped potato has no Item component. Add Item script to the potato prefab/variant.");

            // Ensure it has a trigger collider
            Collider2D col = drop.GetComponent<Collider2D>();
            if (col == null) col = drop.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Ensure trigger callbacks happen reliably (at least one Rigidbody2D involved)
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
                var sr = c.visual.GetComponent<SpriteRenderer>();
                if (sr != null && c.def.stageSprites != null && c.stage < c.def.stageSprites.Length)
                    sr.sprite = c.def.stageSprites[c.stage];
            }

            c.wateredToday = false;
        }
    }
}