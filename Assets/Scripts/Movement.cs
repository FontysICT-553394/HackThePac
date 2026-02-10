using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public float speed = 8.0f;
    public float speedMultiplier = 1.0f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;
    public new Rigidbody2D rigidbody { get; private set; }
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }
    
    public bool noclipEnabled = false;
    
    private void Awake()
    {
        this.rigidbody = GetComponent<Rigidbody2D>();
        this.startingPosition = this.transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    /// <summary>
    /// Resets the player position, speed and direction along with enabling the movement.
    /// </summary>
    /// <returns></returns>
    public void ResetState()
    {
        this.direction = this.initialDirection;
        this.nextDirection = Vector2.zero;
        this.transform.position = this.startingPosition;
        this.rigidbody.position = this.startingPosition;
        this.rigidbody.linearVelocity = Vector2.zero;
        this.rigidbody.isKinematic = false;
        this.enabled = true;
    }

    private void Update()
    {
        if (this.nextDirection != Vector2.zero)
        {
            SetDirection(this.nextDirection);
        }
    }

    private void FixedUpdate()
    {
        if (this.direction == Vector2.zero || !this.enabled) {
            return;
        }

        Vector2 position = this.rigidbody.position;
        Vector2 translation = this.speed * this.speedMultiplier * Time.fixedDeltaTime * this.direction;
    
        this.rigidbody.MovePosition(position + translation);
    }

    /// <summary>
    /// Sets the direction the player should be going to if not hitting a wall. If the wall IS being hit, you can set if the player can go this way anyway by setting forced to true
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="forced"></param>
    /// <returns></returns>
    public void SetDirection(Vector2 direction, bool forced = false)
    {
        if (forced || !Occupied(direction))
        {
            this.direction = direction;
            this.nextDirection = Vector2.zero;
        }
        else
        {
            this.nextDirection = direction;
        }
    }

    /// <summary>
    /// Checks if the player is or is not hitting a wall in the direction they want to go to.
    /// </summary>
    /// <returns></returns>
    public bool Occupied(Vector2 direction)
    {
        if (noclipEnabled) return false;
        
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position, Vector2.one * 0.75f, 0f, direction, 1.5f, this.obstacleLayer);
        return hit.collider != null;
    }
}
