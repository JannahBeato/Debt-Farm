using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactableMap;
    [SerializeField] private TileBase hiddenInteractableTile;

    [SerializeField] private TileBase soilTile; // plowed
    [SerializeField] private TileBase wateredTile; // plowed + watered 



    private Dictionary<Vector3Int, string> modifiedTileStates = new Dictionary<Vector3Int, string>();

    private void Start()
    {
        if (interactableMap == null || hiddenInteractableTile == null)
        {
            Debug.LogError("TileManager missing references (interactableMap or hiddenInteractableTile).");
            return;
        }

        // Hide all interactable cells at startup
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            if (interactableMap.HasTile(position))
            {
                interactableMap.SetTile(position, hiddenInteractableTile);
            }
        }
    }

    public bool IsInteractable(Vector3Int position)
    {
        return interactableMap != null && interactableMap.HasTile(position);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return interactableMap.WorldToCell(worldPosition);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cell)
    {
        return interactableMap.GetCellCenterWorld(cell);
    }

    public string GetState(Vector3Int pos)
    {
        if (modifiedTileStates.TryGetValue(pos, out string state))
        {
            if (state == "interacted") return "soil"; 
            return state;
        }

        return "dirt";
    }

    public bool TrySetState(Vector3Int pos, string stateId)
    {
        if (!IsInteractable(pos)) return false;
        
        if (stateId == "soil")
        {
            if (soilTile == null)
            {
                Debug.LogError("TileManager: soilTile not assigned.");
                return false;
            }
            
            interactableMap.SetTile(pos, soilTile);
            modifiedTileStates[pos] = "soil";
            return true;
        }

        if (stateId == "watered")
        {
            if (wateredTile == null)
            {
                Debug.LogError("TileManager: wateredTile not assigned.");
                return false;
            }

            interactableMap.SetTile(pos, wateredTile);
            modifiedTileStates[pos] = "watered";
            return true;
        }

        if (stateId == "dirt")
        {
            interactableMap.SetTile(pos, hiddenInteractableTile);
            modifiedTileStates.Remove(pos);
            return true;
        }

        return false;
    }

    public void SetInteracted(Vector3Int position)
    {
        TrySetState(position, "soil");
    }

    public void ClearDailyWatered()
    {
        var keys = new List<Vector3Int>(modifiedTileStates.Keys);
        foreach (var k in keys)
        {
            if (modifiedTileStates[k] == "watered")
            {
                modifiedTileStates[k] = "soil";
                interactableMap.SetTile(k, soilTile);
            }
            else if (modifiedTileStates[k] == "interacted")
            {
                // legacy cleanup
                modifiedTileStates[k] = "soil";
                interactableMap.SetTile(k, soilTile);
            }
        }
    }

    public List<TileStateSaveData> GetModifiedTiles()
    {
        List<TileStateSaveData> list = new List<TileStateSaveData>();

        foreach (var pair in modifiedTileStates)
        {
            list.Add(new TileStateSaveData(pair.Key, pair.Value));
        }

        return list;
    }

    public void LoadModifiedTiles(List<TileStateSaveData> savedTiles)
    {
        if (interactableMap == null)
        {
            Debug.LogError("InteractableMap is not assigned in TileManager");
            return;
        }

        modifiedTileStates.Clear();
        if (savedTiles == null) return;

        foreach (TileStateSaveData tileData in savedTiles)
        {
            Vector3Int pos = tileData.GetPosition();

            string id = tileData.stateId;

            if (id == "interacted") id = "soil"; 

            modifiedTileStates[pos] = id;


            if (id == "soil" )
            {
                if (soilTile == null)
                {
                    Debug.LogError("TileManager: soilTile not assigned");
                    continue;
                }
                interactableMap.SetTile(pos, soilTile);
            }
            else if (id == "watered")
            {
                if (wateredTile == null)
                {
                    Debug.LogError("TileManager: wateredTile not assigned");
                    continue;
                }
                interactableMap.SetTile(pos, wateredTile);
            }
            else
            {
                interactableMap.SetTile(pos, hiddenInteractableTile);
            }
        }
    }
}