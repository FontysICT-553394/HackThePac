using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] private float raycastDistance = 0.225f;
    [SerializeField] private Vector2 boxCastSize = Vector2.one * 0.75f;
    
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _queuedDirection = Vector2.zero;
    private Rigidbody2D _rb;
    private bool _isPacman = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if(GameSettings.instance.selectedCharacter == "pacman")
            _isPacman = true;
    }

    private void Update()
    {
        HandleInput();
        TryQueuedDirection();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.up;
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.down;
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.left;
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.right;
    }

    private void TryQueuedDirection()
    {
        if (_queuedDirection != Vector2.zero && CanMove(_queuedDirection))
        {
            _currentDirection = _queuedDirection;
            _queuedDirection = Vector2.zero;
        }
    }

    private void Move()
    {
        if (_currentDirection != Vector2.zero && CanMove(_currentDirection))
        {
            float finalSpeed = moveSpeed;
            if (_isPacman)
            {
                finalSpeed += GameSettings.instance.pacmanSpeed;
            }
            else
            {
                finalSpeed += GameSettings.instance.ghostSpeed;
            }
            
            Vector2 translation = finalSpeed * Time.fixedDeltaTime * _currentDirection;
            _rb.MovePosition(_rb.position + translation);
        }
    }

    private bool CanMove(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, direction, raycastDistance, wallLayer);
        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _currentDirection * raycastDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _queuedDirection * raycastDistance);
    }
}