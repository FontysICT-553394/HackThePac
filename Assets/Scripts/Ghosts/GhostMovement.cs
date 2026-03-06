using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    public enum Mode
    {
        Scatter,
        Chase,
        Frightened,
        Eaten
    }

    [Header("House")]
    [SerializeField] public Transform houseExitTarget;
    [SerializeField] private float exitSnapDistance = 0.05f;

    [Header("House Bobbing")]
    [SerializeField] private float bobAmplitude = 0.35f;
    [SerializeField] private float bobSpeed = 8f;

    public bool IsInHouse { get; private set; } = true;
    public bool startsOutsideHouse = false;
    private bool leavingHouse;
    private Vector3 houseHomePos;

    [Header("Refs")]
    [SerializeField] public NodeGraphBuilder graph;
    [SerializeField] public Transform pacman;

    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float houseExitSpeed = 3f;
    [SerializeField] private Mode mode = Mode.Chase;

    [Header("Scatter Target (world pos)")]
    [SerializeField] public Vector2 scatterCornerWorld = new(12, 10);

    // --- Public read/AI hooks ---
    public Mode CurrentMode => mode;

    // Per-ghost AI can override chase target (Blinky/Pinky/Inky/Clyde)
    public bool UseChaseOverride { get; set; }
    public Vector3 ChaseTargetOverride { get; set; }

    // Last movement direction (cardinal)
    public Vector2Int CurrentMoveDir { get; private set; } = Vector2Int.left;

    // node state
    private NodeData current;
    private NodeData previous;
    private NodeData next;
    private NodeData firstNode;
    private bool movingToFirstNode;

    private float bobTimer;

    // Classic tie-break (arcade-ish): Up, Left, Down, Right
    private static readonly Vector2Int[] TieBreakOrder =
    {
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.right
    };

    private void Start()
    {
        if (!graph)
        {
            Debug.LogError("GhostMovement: graph ontbreekt.");
            enabled = false;
            return;
        }

        if (startsOutsideHouse)
        {
            // Blinky: smoothly move to nearest node (no snap)
            IsInHouse = false;

            // reuse leaving state so Update() calls HandleHouseExit()
            leavingHouse = true;

            // skip Phase 1 & 2, go straight to Phase 3
            movingToFirstNode = true;

            firstNode = graph.GetClosestNode(transform.position);
        }
        else
        {
            // Pinky/Inky/Clyde: stay in house, remember home position
            IsInHouse = true;
            leavingHouse = false;
            houseHomePos = transform.position;

            bobTimer = Random.Range(0f, Mathf.PI * 2f);

            current = null;
            previous = null;
            next = null;
        }
    }

    private void Update()
    {
        // 1) In-house idle bobbing 
        if (IsInHouse && !leavingHouse)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            Vector3 bobOffset = new Vector3(0f, Mathf.Sin(bobTimer) * bobAmplitude, 0f);
            transform.position = houseHomePos + bobOffset;
            return;
        }

        // 2) Leaving house (3-phase)
        if (leavingHouse)
        {
            HandleHouseExit();
            return;
        }

        // 3) Normal node-to-node movement
        if (current == null || next == null) return;

        Vector3 before = transform.position;
        // Normal pathfinding movement (axis-locked)
        Vector3 pos = transform.position;

        // welke richting is deze segment? (node->node is altijd horizontaal of verticaal)
        Vector3 seg = next.WorldPos - current.WorldPos;

        if (Mathf.Abs(seg.x) > Mathf.Abs(seg.y))
        {
            // horizontaal segment: Y vastzetten op current node lijn
            pos.y = current.WorldPos.y;
            float newX = Mathf.MoveTowards(pos.x, next.WorldPos.x, speed * Time.deltaTime);
            pos.x = newX;
        }
        else
        {
            // verticaal segment: X vastzetten op current node lijn
            pos.x = current.WorldPos.x;
            float newY = Mathf.MoveTowards(pos.y, next.WorldPos.y, speed * Time.deltaTime);
            pos.y = newY;
        }

        transform.position = pos;

        // track direction
        Vector3 delta = transform.position - before;
        if (delta.sqrMagnitude > 0.000001f)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                CurrentMoveDir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                CurrentMoveDir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        // arrived at node
        if ((transform.position - next.WorldPos).sqrMagnitude < 0.0001f)
        {
            transform.position = next.WorldPos;
            previous = current;
            current = next;
            next = ChooseNextNode(current, previous);
        }
    }

    private void HandleHouseExit()
    {
        // If no exit target, fail gracefully: snap to closest node and start normal movement
        if (houseExitTarget == null)
        {
            firstNode = graph.GetClosestNode(transform.position);
            transform.position = firstNode.WorldPos;
            leavingHouse = false;
            movingToFirstNode = false;
            current = firstNode;
            previous = null;
            next = ChooseNextNode(current, previous);
            return;
        }

        // Phase 3: Smoothly move to the first graph node (no snap)
        if (movingToFirstNode)
        {
            transform.position = Vector3.MoveTowards(transform.position, firstNode.WorldPos, houseExitSpeed * Time.deltaTime);

            if ((transform.position - firstNode.WorldPos).sqrMagnitude <= exitSnapDistance * exitSnapDistance)
            {
                transform.position = firstNode.WorldPos;
                leavingHouse = false;
                movingToFirstNode = false;

                current = firstNode;
                previous = null;
                next = ChooseNextNode(current, previous);
            }
            return;
        }

        // Phase 1: Align horizontally with exit (x)
        Vector3 centerAlign = new Vector3(houseExitTarget.position.x, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - houseExitTarget.position.x) > exitSnapDistance)
        {
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, centerAlign, houseExitSpeed * Time.deltaTime);

            // direction track while exiting
            Vector3 d = transform.position - before;
            if (d.sqrMagnitude > 0.000001f)
                CurrentMoveDir = d.x > 0 ? Vector2Int.right : Vector2Int.left;

            return;
        }

        // Phase 2: Move toward the exit point
        {
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, houseExitTarget.position, houseExitSpeed * Time.deltaTime);

            Vector3 d = transform.position - before;
            if (d.sqrMagnitude > 0.000001f)
            {
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                    CurrentMoveDir = d.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    CurrentMoveDir = d.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        if ((transform.position - houseExitTarget.position).sqrMagnitude <= exitSnapDistance * exitSnapDistance)
        {
            // Arrived at exit — start Phase 3: smoothly move to nearest node
            transform.position = houseExitTarget.position;
            firstNode = graph.GetClosestNode(transform.position);
            movingToFirstNode = true;
        }
    }

    private NodeData ChooseNextNode(NodeData from, NodeData cameFrom)
    {
        var options = new List<NodeData>(from.Neighbors);

        if (options.Count == 0) return cameFrom;

        // Don't allow reversing in normal movement, unless it's the only way.
        // While leavingHouse we allow reverse so it can always reach exit correctly.
        if (!leavingHouse && cameFrom != null && options.Count > 1)
        {
            options.Remove(cameFrom);
        }

        // Frightened = random choice (arcade-like)
        if (mode == Mode.Frightened)
        {
            if (options.Count == 0) return cameFrom;
            int idx = Random.Range(0, options.Count);
            return options[idx];
        }

        Vector3 target = GetTargetWorld();

        // Pick option with min distance to target; tie-break by classic direction priority
        NodeData best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var opt in options)
        {
            float d = (opt.WorldPos - target).sqrMagnitude;

            if (d < bestDist - 0.00001f)
            {
                bestDist = d;
                best = opt;
            }
            else if (Mathf.Abs(d - bestDist) < 0.00001f && best != null)
            {
                var dirA = WorldDir(from.WorldPos, best.WorldPos);
                var dirB = WorldDir(from.WorldPos, opt.WorldPos);

                if (CompareDirPriority(dirB, dirA) < 0)
                    best = opt;
            }
        }

        return best ?? options[0];
    }

    private Vector3 GetTargetWorld()
    {
        switch (mode)
        {
            case Mode.Scatter:
                return (Vector3)scatterCornerWorld;

            case Mode.Chase:
                if (UseChaseOverride) return ChaseTargetOverride;
                return pacman ? pacman.position : transform.position;

            case Mode.Eaten:
                // later: return-to-house target; for now just go to exit target if set
                return houseExitTarget ? houseExitTarget.position : transform.position;

            case Mode.Frightened:
            default:
                return pacman ? pacman.position : transform.position;
        }
    }

    private static Vector2Int WorldDir(Vector3 from, Vector3 to)
    {
        Vector2 delta = to - from;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x >= 0 ? Vector2Int.right : Vector2Int.left;
        else
            return delta.y >= 0 ? Vector2Int.up : Vector2Int.down;
    }

    private static int CompareDirPriority(Vector2Int a, Vector2Int b)
    {
        return PriorityIndex(a).CompareTo(PriorityIndex(b));
    }

    private static int PriorityIndex(Vector2Int d)
    {
        for (int i = 0; i < TieBreakOrder.Length; i++)
            if (TieBreakOrder[i] == d)
                return i;
        return 999;
    }

    public void SetMode(Mode newMode, bool reverseOnSwitch)
    {
        if (mode == newMode)
            return;

        mode = newMode;

        // If inside house or leaving, we don't manipulate path nodes physically
        if (IsInHouse || leavingHouse || current == null || next == null)
            return;

        // 1. Snap to the CURRENT corridor axis to fix floating-point drift
        // This is safe because we haven't changed segment yet.
        Vector3 seg = next.WorldPos - current.WorldPos;
        Vector3 p = transform.position;

        if (Mathf.Abs(seg.x) > Mathf.Abs(seg.y))
            p.y = current.WorldPos.y;
        else
            p.x = current.WorldPos.x;

        transform.position = p;

        // 2. Reverse direction by swapping current <-> next
        if (reverseOnSwitch)
        {
            // Instead of next = previous (which changes the axis to the PREVIOUS path segment 
            // and forces a teleport-snap), we simply swap current and next.
            // This reverses the ghost along the EXACT SAME segment it is currently on.
            NodeData temp = current;
            current = next;
            next = temp;

            // Note: 'previous' is not updated here, but that's okay.
            // When we arrive at 'next' (the old 'current'), 
            // the standard arrival logic will update 'previous = current' (old 'next').
        }
    }

    public void ReleaseFromHouse()
    {
        if (!IsInHouse) return;

        IsInHouse = false;
        leavingHouse = true;
        movingToFirstNode = false; // start phase 1/2/3 from inside
    }
}
