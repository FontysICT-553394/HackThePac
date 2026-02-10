using UnityEngine;

public class GhostChase : GhostBehavior
{
    private Vector2 direction;
    private float minDistance;

    private void OnDisable()
    {
        this.ghost.scatter.Enable();
    }

    /// <summary>
    /// Sets up the AI of the Ghost if not being controlled by a player when the Ghost should be chasing down a Pac-Man. Uses Node to find the nearest available directions.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public void GhostAI(Node node)
    {
        if (this.ghost.target == null || !this.ghost.target.gameObject.activeSelf)
        {
            this.ghost.FindNewTarget();
        }
        
        if (this.ghost.target == null)
        {
            int index = Random.Range(0, node.availableDirections.Count);
            this.ghost.movement.SetDirection(node.availableDirections[index]);
            return;
        }
        
        direction = Vector2.zero;
        minDistance = float.MaxValue;

        foreach (Vector2 availableDirection in node.availableDirections)
        {
            Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
            float distance = (this.ghost.target.position - newPosition).sqrMagnitude;

            if (distance < minDistance)
            {
                direction = availableDirection;
                minDistance = distance;
            }
        }
        this.ghost.movement.SetDirection(direction);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled && !this.ghost.frightened.enabled)
        {
            this.ghost.lastNode = node;
            if (this.ghost.controlledBy == null)
            {
                GhostAI(node);
            }
        }
    }
    
}
