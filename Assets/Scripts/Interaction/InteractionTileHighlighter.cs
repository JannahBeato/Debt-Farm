using UnityEngine;

[DisallowMultipleComponent]
public class InteractionTileHighlighter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private TileManager tileManager;

    [Header("Display")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private float inset = 0.06f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 500;
    [SerializeField] private float zOffset = -0.1f;

    private LineRenderer lineRenderer;
    private Vector3Int lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (tileManager == null)
            tileManager = FindFirstObjectByType<TileManager>();

        CreateLineRenderer();
        HideHighlight();
    }

    private void Update()
    {
        if (playerMovement == null || tileManager == null)
        {
            HideHighlight();
            return;
        }

        if (!InputManager.ShouldShowInteractionTile())
        {
            HideHighlight();
            return;
        }

        Vector3Int targetCell = GetFrontCell();

        if (!lineRenderer.enabled || targetCell != lastCell)
        {
            DrawCellOutline(targetCell);
            lastCell = targetCell;
        }

        lineRenderer.enabled = true;
    }

    private Vector3Int GetFrontCell()
    {
        Vector3Int currentCell = tileManager.WorldToCell(playerMovement.transform.position);
        Vector2 facing = playerMovement.LastMotionVector;

        if (facing == Vector2.zero)
            facing = Vector2.down;

        Vector2Int offset;

        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
            offset = new Vector2Int(facing.x > 0 ? 1 : -1, 0);
        else
            offset = new Vector2Int(0, facing.y > 0 ? 1 : -1);

        return currentCell + new Vector3Int(offset.x, offset.y, 0);
    }

    private void CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("InteractionTileOutline");
        lineObj.transform.SetParent(transform, false);

        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.positionCount = 5;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = highlightColor;
        lineRenderer.endColor = highlightColor;
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    private void DrawCellOutline(Vector3Int cell)
    {
        Vector3 center = tileManager.GetCellCenterWorld(cell);

        Vector3 rightCenter = tileManager.GetCellCenterWorld(cell + Vector3Int.right);
        Vector3 upCenter = tileManager.GetCellCenterWorld(cell + Vector3Int.up);

        float width = Mathf.Abs(rightCenter.x - center.x);
        float height = Mathf.Abs(upCenter.y - center.y);

        if (width <= 0.0001f) width = 1f;
        if (height <= 0.0001f) height = 1f;

        float halfWidth = Mathf.Max(0.01f, (width * 0.5f) - inset);
        float halfHeight = Mathf.Max(0.01f, (height * 0.5f) - inset);

        float z = center.z + zOffset;

        Vector3 bottomLeft = new Vector3(center.x - halfWidth, center.y - halfHeight, z);
        Vector3 topLeft = new Vector3(center.x - halfWidth, center.y + halfHeight, z);
        Vector3 topRight = new Vector3(center.x + halfWidth, center.y + halfHeight, z);
        Vector3 bottomRight = new Vector3(center.x + halfWidth, center.y - halfHeight, z);

        lineRenderer.SetPosition(0, bottomLeft);
        lineRenderer.SetPosition(1, topLeft);
        lineRenderer.SetPosition(2, topRight);
        lineRenderer.SetPosition(3, bottomRight);
        lineRenderer.SetPosition(4, bottomLeft);
    }

    private void HideHighlight()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void OnDisable()
    {
        HideHighlight();
    }
}