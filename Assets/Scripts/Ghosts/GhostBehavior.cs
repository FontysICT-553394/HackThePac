using UnityEngine;

[RequireComponent(typeof(Ghost))]
public abstract class GhostBehavior : MonoBehaviour{
    public Ghost ghost { get; private set; }
    public float duration;

    private void Awake()
    {
        ghost = GetComponent<Ghost>();
    }

    public void Enable()
    {
        Enable(duration);
    }

    public virtual void Enable(float duration)
    {
        enabled = true;

        CancelInvoke();
        Invoke(nameof(Disable), duration);
    }

    public virtual void Disable()
    {
        enabled = false;

        CancelInvoke();
    }

    protected bool IsReverseDirection(Vector2 direction, Node node)
    {
        if (ghost == null || ghost.movement == null || node == null)
            return false;

        return node.availableDirections.Count >1 &&
               ghost.movement.direction != Vector2.zero &&
               direction == -ghost.movement.direction;
    }
}

