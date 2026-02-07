using System;
using Unity.VisualScripting;
using UnityEngine;

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


    // Update is called once per frame
    void Update()
    {
        EnableMovement();

        //Animation
        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);

        //Last Direction
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

    public void EnableMovement()
    {
        if (!_canMove)
        {
            _movement = Vector2.zero;
        }
        else
        {
            _movement = InputManager.Movement;
        }
    }

    private void TryMove(Vector2 direction)
    {
        //Skips uneccessary calculations
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

        float distance = _moveSpeed * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.BoxCast(
            _rb.position,
            _collider.size,
            0f,
            direction,
            distance,
            _collisionLayer);

        return hit.collider != null;

    }
}
