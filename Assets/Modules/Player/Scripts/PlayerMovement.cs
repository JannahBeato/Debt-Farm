using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private LayerMask _collisionLayer;
    [SerializeField] private bool _canMove = true;

    private Vector2 _movement;
    private Rigidbody2D _rb;
    private Animator _animator;
    private BoxCollider2D _collider;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _lastHorizontal = "LastHorizontal";
    private const string _lastVertical = "LastVertical";

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ReadMovementInput();

        // Animation
        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);

        // Last Direction
        if (_movement != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, _movement.x);
            _animator.SetFloat(_lastVertical, _movement.y);
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

        // Make sure diagonals aren't faster and casts are consistent
        _movement = InputManager.Movement.normalized;
    }

    private void TryMove(Vector2 direction)
    {
        // Skip unnecessary calculations
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
        // Small "skin" helps prevent tiny overlaps due to timestep/rounding
        float distance = _moveSpeed * Time.fixedDeltaTime + 0.01f;

        RaycastHit2D hit = Physics2D.BoxCast(
            (Vector2)_collider.bounds.center, // correct origin (handles offset)
            _collider.bounds.size,            // correct size in world units (handles scale)
            0f,
            direction.normalized,
            distance,
            _collisionLayer
        );

        return hit.collider != null;
    }
}
