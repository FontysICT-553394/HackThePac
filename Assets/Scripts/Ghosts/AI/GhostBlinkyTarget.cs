using UnityEngine;

public class GhostBlinkyTarget : GhostTargetStrategy{
    protected override Vector3 GetTargetPosition()
    {
        if (pacmanTransform == null)
            return transform.position;

        return pacmanTransform.position;
    }
}


