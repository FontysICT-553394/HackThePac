using UnityEngine;

public class GhostPinkyTarget : GhostTargetStrategy{
    [SerializeField] private int tilesAhead =4;

    protected override Vector3 GetTargetPosition()
    {
        if (pacmanTransform == null)
            return transform.position;

        Vector2 direction = GetPacmanDirection();
        return pacmanTransform.position + (Vector3)(direction * tilesAhead);
    }
}
