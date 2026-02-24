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
        if (HasCrop(cell)) return false;

        string state = tileManager.GetState(cell);
        if (state != "soil" && state != "watered") return false;

        Vector3 world = tileManager.GetCellCenterWorld(cell);
        GameObject v = Instantiate(cropVisualPrefab, world, Quaternion.identity, transform);

        var sr = v.GetComponent<SpriteRenderer>();
        sr.sprite = def.stageSprites[0];

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

            if (c.wateredToday && c.stage < 4)
            {
                c.stage++;
                var sr = c.visual.GetComponent<SpriteRenderer>();
                sr.sprite = c.def.stageSprites[c.stage];
            }

            c.wateredToday = false;
        }
    }
}