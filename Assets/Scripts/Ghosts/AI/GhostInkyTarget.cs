using UnityEngine;

public class GhostInkyTarget : GhostTargetStrategy{
    [SerializeField] private int tilesAhead =2;

    private Ghost blinkyGhost;

    protected override void Start()
    {
        base.Start();
        FindBlinky();
    }

    protected override Vector3 GetTargetPosition()
    {
        if (pacmanTransform == null)
            return transform.position;

        if (blinkyGhost == null)
            FindBlinky();

        Vector3 pacmanPoint = pacmanTransform.position + (Vector3)(GetPacmanDirection() * tilesAhead);

        if (blinkyGhost == null)
            return pacmanPoint;

        Vector3 fromBlinkyToPoint = pacmanPoint - blinkyGhost.transform.position;
        return pacmanPoint + fromBlinkyToPoint;
    }

    private void FindBlinky()
    {
        Ghost[] ghosts = FindObjectsOfType<Ghost>();

        foreach (Ghost foundGhost in ghosts)
        {
            if (foundGhost.GetComponent<GhostBlinkyTarget>() != null)
            {
                blinkyGhost = foundGhost;
                return;
            }
        }
    }
}
