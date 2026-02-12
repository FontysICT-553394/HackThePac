using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float raycastDistance = 0.25f;
    [SerializeField] private Vector2 boxCastSize = Vector2.one * 0.75f;

    private Vector2 currentDirection = Vector2.zero;
    private Vector2 queuedDirection = Vector2.zero;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            queuedDirection = Vector2.up;
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            queuedDirection = Vector2.down;
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            queuedDirection = Vector2.left;
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            queuedDirection = Vector2.right;
    }

    private void TryQueuedDirection()
    {
        if (queuedDirection != Vector2.zero && CanMove(queuedDirection))
        {
            currentDirection = queuedDirection;
            queuedDirection = Vector2.zero;
        }
    }

    private void Move()
    {
        if (currentDirection != Vector2.zero && CanMove(currentDirection))
        {
            RotateToDirection(currentDirection);
            
            float finalSpeed = moveSpeed;
            if (GameSettings.instance.selectedCharacter.Equals("pacman"))
            {
                finalSpeed += GameSettings.instance.pacmanSpeed;
            }
            else
            {
                finalSpeed += GameSettings.instance.ghostSpeed;
            }
            
            Vector2 translation = finalSpeed * Time.fixedDeltaTime * currentDirection;
            rb.MovePosition(rb.position + translation);
        }
    }

    private void RotateToDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool CanMove(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, direction, raycastDistance, wallLayer);
        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, currentDirection * raycastDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, queuedDirection * raycastDistance);
    }
}