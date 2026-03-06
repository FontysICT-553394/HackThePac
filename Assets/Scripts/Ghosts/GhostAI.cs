    using UnityEngine;

public class GhostAI : MonoBehaviour
{
    public enum GhostType { Blinky, Pinky, Inky, Clyde }

    [Header("Type")]
    [SerializeField] private GhostType type = GhostType.Blinky;

    [Header("Refs")]
    [SerializeField] private GhostMovement movement;
    [SerializeField] private PacMan pacman;
    [SerializeField] private GhostMovement blinkyForInky; // alleen voor Inky

    [Header("Tunables")]
    [SerializeField] private int pinkyAheadTiles = 4;
    [SerializeField] private int inkyAheadTiles = 2;
    [SerializeField] private float clydeChaseDistanceTiles = 8f;

    private void Reset()
    {
        movement = GetComponent<GhostMovement>();
    }

    private void Awake()
    {
        if (!movement) movement = GetComponent<GhostMovement>();
    }

    private void LateUpdate()
    {
        if (!movement || !pacman) return;

        // Alleen in Chase zetten we override
        if (movement.CurrentMode == GhostMovement.Mode.Chase && !movement.IsInHouse)
        {
            movement.UseChaseOverride = true;
            movement.ChaseTargetOverride = ComputeChaseTarget();
        }
        else
        {
            movement.UseChaseOverride = false;
        }
    }

    private Vector3 ComputeChaseTarget()
    {
        Vector3 pPos = pacman.transform.position;
        Vector2Int pDir = pacman.MoveDir;

        switch (type)
        {
            case GhostType.Blinky:
                // Target = Pac-Man tile
                return pPos;

            case GhostType.Pinky:
                // Target = 4 tiles ahead of Pac-Man
                // (Arcade had a known up-bug; dit is de "moderne correcte" variant)
                return pPos + new Vector3(pDir.x, pDir.y, 0f) * pinkyAheadTiles;

            case GhostType.Inky:
                {
                    // Target = vector trick with Blinky
                    if (!blinkyForInky) return pPos;

                    Vector3 p2 = pPos + new Vector3(pDir.x, pDir.y, 0f) * inkyAheadTiles;
                    Vector3 v = p2 - blinkyForInky.transform.position;
                    return p2 + v; // double vector
                }

            case GhostType.Clyde:
                {
                    float dist = Vector2.Distance(movement.transform.position, pPos);
                    if (dist > clydeChaseDistanceTiles)
                        return pPos; // chase
                    else
                        return (Vector3)movement.scatterCornerWorld; // scatter als dichtbij
                }

            default:
                return pPos;
        }
    }
}