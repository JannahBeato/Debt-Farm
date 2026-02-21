using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
        {
            [SerializeField] private Tilemap interactableMap;
            [SerializeField] private TileBase hiddenInteractableTile;

            [SerializeField] private TileBase interactedTile;

            private Dictionary<Vector3Int, string> modifiedTileStates = new Dictionary<Vector3Int, string>();

            void Start()
            {
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
                TileBase tile = interactableMap.GetTile(position);

                if(tile != null)
                {
                    if(tile.name == "Interactable")
                    {
                        return true;
                    }
                }

                return false;
            }

            public Vector3Int WorldToCell(Vector3 worldPosition)
            {
                return interactableMap.WorldToCell(worldPosition);
            }

            public void SetInteracted(Vector3Int position)
            {
                interactableMap.SetTile(position, interactedTile);
                modifiedTileStates[position] = "interacted";
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
                
                if (savedTiles == null)
                    return;
                
                foreach (TileStateSaveData tileData in savedTiles)
                {
                    Vector3Int pos = tileData.GetPosition();

                    modifiedTileStates[pos] = tileData.stateId;

                    if (tileData.stateId == "interacted")
                    {
                        interactableMap.SetTile(pos, interactedTile);
                    }

                }


            }


        }

