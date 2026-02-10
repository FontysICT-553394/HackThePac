using UnityEngine;

public class GhostScatter : GhostBehavior
{
    private void OnDisable()
    {
        this.ghost.chase.Enable();
    }

    /// <summary>
    /// Sets up the AI of the Ghost if not being controlled by a player when all the Ghosts scatter at the beginning of the game. Uses Node to find the nearest available directions.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public void GhostAI(Node node)
    {
        int index = Random.Range(0, node.availableDirections.Count);
        if (node.availableDirections.Count > 1 && node.availableDirections[index] == -ghost.movement.direction)
        {
            index++;

            if (index >= node.availableDirections.Count)
            {
                index = 0;
            }
        }

        this.ghost.movement.SetDirection(node.availableDirections[index]);
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
