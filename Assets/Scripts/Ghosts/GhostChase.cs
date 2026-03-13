using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GhostChase : GhostBehavior{
    private void OnDisable()
    {
        if (ghost.scatter != null)
            ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            Vector2 direction = ghost.movement.direction;
            float minDistance = float.MaxValue;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                if (IsReverseDirection(availableDirection, node))
                    continue;

                Vector3 newPosition = transform.position + (Vector3)availableDirection;
                float distance = (ghost.target.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }

            ghost.movement.SetDirection(direction);
        }
    }
}

