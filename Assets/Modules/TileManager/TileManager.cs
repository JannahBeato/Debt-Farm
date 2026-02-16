using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
        {
            [SerializeField] private Tilemap interactableMap;
            [SerializeField] private TileBase hiddenInteractableTile;

            [SerializeField] private TileBase interactedTile;

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
            }



        }

