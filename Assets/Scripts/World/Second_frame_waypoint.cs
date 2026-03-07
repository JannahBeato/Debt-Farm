using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SecondFrameWaypoint : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private PolygonCollider2D _mapBoundary;
    private CinemachineConfiner2D _confiner;

    [Header("Player Move Offset")]
    [SerializeField] private Direction _direction;
    [SerializeField] private float _offsetAmount = 2f;

    private MethodInfo _invalidateMethod;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private const string _playerTag = "Player";

    private void Awake()
    {
        _confiner = FindObjectOfType<CinemachineConfiner2D>();

        if (_confiner != null)
        {
            // Cinemachine 2: InvalidatePathCache()
            // Cinemachine 3: InvalidateCache()
            var t = _confiner.GetType();
            _invalidateMethod =
                t.GetMethod("InvalidateCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? t.GetMethod("InvalidatePathCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(_playerTag))
            return;

        UpdateCameraConfiner();
        UpdatePlayerPosition(collision.gameObject);
    }

    private void UpdateCameraConfiner()
    {
        if (_confiner == null || _mapBoundary == null)
            return;

        _confiner.BoundingShape2D = _mapBoundary;

        // IMPORTANT: refresh confiner cache so the camera uses the new boundary immediately
        _invalidateMethod?.Invoke(_confiner, null);
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (!player.TryGetComponent(out Rigidbody2D rb))
            return;

        Vector2 offset = GetOffsetVector(_direction) * _offsetAmount;
        Vector2 newPos = rb.position + offset;

        rb.MovePosition(newPos);
    }

    private static Vector2 GetOffsetVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            _ => Vector2.zero
        };
    }
}