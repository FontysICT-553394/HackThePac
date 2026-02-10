using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer blue;
    public SpriteRenderer white;

    private Vector2 direction;
    private float maxDistance;

    public bool eaten { get; private set; }

    public override void Enable(float duration)
    {
        base.Enable(duration);
        
        FindObjectOfType<GameManager>().SetGhostFrightened(true);

        this.body.enabled = false;
        this.eyes.enabled = false;
        this.blue.enabled = true;
        this.white.enabled = false;
        
        Invoke(nameof(Flash), duration / 2.0f);
    }

    public override void Disable()
    {
        base.Disable();
        
        FindObjectOfType<GameManager>().SetGhostFrightened(false);

        this.body.enabled = true;
        this.eyes.enabled = true;
        this.blue.enabled = false;
        this.white.enabled = false;
    }
    
    private void Flash()
    {
        if (!this.eaten)
        {
            this.blue.enabled = false;
            this.white.enabled = true;
            this.white.GetComponent<AnimatedSprite>().Restart();
        }
    }

    /// <summary>
    /// If the Ghost is eaten, it will return to the Home (center of the maze) as nothing but eyes (and will stay in the Home) for a set amount of time.
    /// </summary>
    /// <returns></returns>
    private void Eaten()
    {
        
        if (!this.eaten) {
            FindObjectOfType<GameManager>().SetGhostFrightened(false);
        }
        
        
        this.eaten = true;
        
        Vector3 position = this.ghost.home.inside.position;
        position.z = this.ghost.transform.position.z;
        this.ghost.transform.position = position;
        this.ghost.home.Enable(this.duration);
        
        this.body.enabled = false;
        this.eyes.enabled = true;
        this.blue.enabled = false;
        this.white.enabled = false;
    }
    private void OnEnable()
    {
        blue.GetComponent<AnimatedSprite>().Restart();
        this.ghost.movement.speedMultiplier = 0.5f;
        this.eaten = false;
    }

    private void OnDisable()
    {
        this.ghost.movement.speedMultiplier = 1.0f;
        this.eaten = false;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (this.enabled)
            {
                Eaten();
            }
        }
    }

    /// <summary>
    /// Sets up the AI of the Ghost if not being controlled by a player when the Ghost is frightened. Uses Node to find the nearest available directions.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public void GhostAI(Node node)
    {
        direction = Vector2.zero;
        maxDistance = float.MinValue;

        Pacman[] allPacmen = FindObjectsOfType<Pacman>();
        Transform closestPacman = null;
        float minDistanceToPacman = float.MaxValue;

        foreach (Pacman p in allPacmen)
        {
            if (!p.gameObject.activeSelf) continue;
            float d = (p.transform.position - this.transform.position).sqrMagnitude;
            if (d < minDistanceToPacman)
            {
                minDistanceToPacman = d;
                closestPacman = p.transform;
            }
        }

        if (closestPacman == null)
        {
            int index = Random.Range(0, node.availableDirections.Count);
            this.ghost.movement.SetDirection(node.availableDirections[index]);
            return;
        }

        foreach (Vector2 availableDirection in node.availableDirections)
        {
            Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
            float distance = (closestPacman.position - newPosition).sqrMagnitude;

            if (distance > maxDistance)
            {
                direction = availableDirection;
                maxDistance = distance;
            }
        }

        this.ghost.movement.SetDirection(direction);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled)
        {
            this.ghost.lastNode = node;
            if (this.ghost.controlledBy == null)
            {
                GhostAI(node);
            }
        }
    }
}
