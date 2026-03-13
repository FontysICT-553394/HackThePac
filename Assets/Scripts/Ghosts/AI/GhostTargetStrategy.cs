using UnityEngine;

[RequireComponent(typeof(Ghost))]
public abstract class GhostTargetStrategy : MonoBehaviour{
    protected Ghost ghost;
    protected Transform pacmanTransform;
    protected Movement pacmanMovement;

    private Transform runtimeTarget;

    protected virtual void Start()
    {
        ghost = GetComponent<Ghost>();
        FindPacman();
        CreateRuntimeTarget();
    }

    protected virtual void LateUpdate()
    {
        if (ghost == null)
            ghost = GetComponent<Ghost>();

        if (pacmanTransform == null || pacmanMovement == null)
            FindPacman();

        if (runtimeTarget == null)
            CreateRuntimeTarget();

        if (runtimeTarget != null)
            runtimeTarget.position = GetTargetPosition();
    }

    protected abstract Vector3 GetTargetPosition();

    protected Vector2 GetPacmanDirection()
    {
        if (pacmanMovement == null)
            return Vector2.zero;

        return pacmanMovement.direction;
    }

    private void FindPacman()
    {
        PacMan pacman = FindObjectOfType<PacMan>();

        if (pacman != null)
        {
            pacmanTransform = pacman.transform;
            pacmanMovement = pacman.GetComponent<Movement>();
        }
    }

    private void CreateRuntimeTarget()
    {
        GameObject targetObject = new GameObject(GetType().Name + "_Target");
        targetObject.transform.SetParent(transform);
        targetObject.transform.position = transform.position;

        runtimeTarget = targetObject.transform;

        if (ghost != null)
            ghost.target = runtimeTarget;
    }
}
