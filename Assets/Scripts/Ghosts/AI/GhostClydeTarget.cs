using UnityEngine;

public class GhostClydeTarget : GhostTargetStrategy{
    [SerializeField] private float chaseDistance =8f;
    [SerializeField] private Vector2 scatterCorner = new Vector2(-100f, -100f);

    protected override Vector3 GetTargetPosition()
    {
        if (pacmanTransform == null)
            return transform.position;

        float distance = Vector2.Distance(transform.position, pacmanTransform.position);

        if (distance > chaseDistance)
            return pacmanTransform.position;

        return new Vector3(scatterCorner.x, scatterCorner.y, transform.position.z);
    }
}


