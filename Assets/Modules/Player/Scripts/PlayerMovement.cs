using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private LayerMask _collisionLayer;
    [SerializeField] private bool _canMove = true;

    [SerializeField] private TileManager _tileManager;
    [SerializeField] private CropManager _cropManager;

    private Vector2 _movement;
    private Rigidbody2D _rb;
    private Animator _animator;
    private BoxCollider2D _collider;

    // ✅ Other scripts can read this (tools, interaction, etc.)
    public Vector2 LastMotionVector { get; private set; } = Vector2.down;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _lastHorizontal = "LastHorizontal";
    private const string _lastVertical = "LastVertical";

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        if (_cropManager == null) _cropManager = FindFirstObjectByType<CropManager>();
    }

    private void Update()
    {
        ReadMovementInput();

        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);

        // ✅ Update last facing direction for both animator + tools
        if (_movement != Vector2.zero)
        {
            LastMotionVector = _movement;

            _animator.SetFloat(_lastHorizontal, _movement.x);
            _animator.SetFloat(_lastVertical, _movement.y);
        }

        if (InputManager.InteractPressed)
        {
            // Try the tile you're on
            Vector3Int cell = _tileManager.WorldToCell(transform.position);

            // Harvest FIRST (if ready), then stop so no other action happens
            if (_cropManager != null && _cropManager.TryHarvest(cell))
                return;

            // Your existing tile interaction logic
            if (_tileManager.IsInteractable(cell))
            {
                Debug.Log("Interacted with tile at " + cell);
                _tileManager.SetInteracted(cell);
            }
        }
    }

    private void FixedUpdate()
    {
        TryMove(_movement);
    }

    private void ReadMovementInput()
    {
        if (!_canMove)
        {
            _movement = Vector2.zero;
            return;
        }

        _movement = InputManager.Movement.normalized;
    }

    private void TryMove(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return;

        Vector2 targetPos = _rb.position + direction * _moveSpeed * Time.fixedDeltaTime;

        if (!IsColliding(direction))
        {
            _rb.MovePosition(targetPos);
        }
    }

    private bool IsColliding(Vector2 direction)
    {
        float distance = _moveSpeed * Time.fixedDeltaTime + 0.01f;

        RaycastHit2D hit = Physics2D.BoxCast(
            (Vector2)_collider.bounds.center,
            _collider.bounds.size,
            0f,
            direction.normalized,
            distance,
            _collisionLayer
        );

        return hit.collider != null;
    }

    
    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;

        if (!canMove)
            _movement = Vector2.zero;
    }

}
