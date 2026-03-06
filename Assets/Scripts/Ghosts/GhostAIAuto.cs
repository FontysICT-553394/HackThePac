using UnityEngine;

public class GhostAIAuto : MonoBehaviour
{
    public enum GhostType { Blinky, Pinky, Inky, Clyde }

    [Header("Type")]
    [SerializeField] private GhostType type;

    [Header("Refs (auto / semi-auto)")]
    [SerializeField] private PacMan pacman;                 // sleep PacMan (of vind hem)
    [SerializeField] private GhostAIIdentityRegistry registry; // optioneel (zie onder)
    [SerializeField] private int findEveryFrames = 10;

    [Header("Tunables")]
    [SerializeField] private int pinkyAheadTiles = 4;
    [SerializeField] private int inkyAheadTiles = 2;
    [SerializeField] private float clydeChaseDistanceTiles = 8f;

    private GhostMovement move;
    private GhostMovement blinkyMove; // alleen nodig voor Inky
    private int frameCounter;

    private void Awake()
    {
        if (!pacman) pacman = FindObjectOfType<PacMan>();
        TryBindMovement();
    }

    private void Update()
    {
        // Movement kan later toegevoegd/gewisseld worden ? probeer af en toe opnieuw te binden
        if (move == null || !move.isActiveAndEnabled)
        {
            frameCounter++;
            if (frameCounter % Mathf.Max(1, findEveryFrames) == 0)
                TryBindMovement();
            return;
        }

        // Alleen AI als deze ghost daadwerkelijk een AI-movement gebruikt
        // (Als speler de ghost bestuurt, wil je waarschijnlijk géén override target)
        if (move.CurrentMode == GhostMovement.Mode.Chase && !move.IsInHouse)
        {
            move.UseChaseOverride = true;
            move.ChaseTargetOverride = ComputeChaseTarget();
        }
        else
        {
            move.UseChaseOverride = false;
        }
    }

    private void TryBindMovement()
    {
        move = GetComponent<GhostMovement>();
        if (move == null) return;

        // Vind Blinky movement voor Inky (3 manieren; kies wat bij jullie past)
        if (type == GhostType.Inky)
        {
            // 1) via registry (meest netjes)
            if (registry != null)
                blinkyMove = registry.BlinkyMovement;

            // 2) fallback: zoek op tag/naam (minder netjes, maar werkt)
            if (blinkyMove == null)
            {
                var blinkyObj = GameObject.Find("blinky(Clone)") ?? GameObject.Find("blinky");
                if (blinkyObj) blinkyMove = blinkyObj.GetComponent<GhostMovement>();
            }
        }
    }

    private Vector3 ComputeChaseTarget()
    {
        Vector3 pPos = pacman ? pacman.transform.position : transform.position;
        Vector2Int pDir = pacman ? pacman.MoveDir : Vector2Int.right;

        switch (type)
        {
            case GhostType.Blinky:
                return pPos;

            case GhostType.Pinky:
                return pPos + new Vector3(pDir.x, pDir.y, 0f) * pinkyAheadTiles;

            case GhostType.Inky:
                {
                    if (blinkyMove == null) return pPos;
                    Vector3 p2 = pPos + new Vector3(pDir.x, pDir.y, 0f) * inkyAheadTiles;
                    Vector3 v = p2 - blinkyMove.transform.position;
                    return p2 + v;
                }

            case GhostType.Clyde:
                {
                    float dist = Vector2.Distance(transform.position, pPos);
                    return dist > clydeChaseDistanceTiles ? pPos : (Vector3)move.scatterCornerWorld;
                }

            default:
                return pPos;
        }
    }
}