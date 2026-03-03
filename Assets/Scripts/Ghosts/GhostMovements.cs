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

    [Header("Movement")] [SerializeField] private float speed = 4f;
    [SerializeField] private float houseExitSpeed = 3f;
    [SerializeField] private Mode mode = Mode.Chase;

    [Header("Scatter Target (world pos)")] 
    [SerializeField] public Vector2 scatterCornerWorld = new(12, 10);

    // node state
    private NodeData current;
    private NodeData previous;
    private NodeData next;
    private NodeData firstNode;
    private bool movingToFirstNode;

    private float bobTimer;
    
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
            leavingHouse = true;          // reuse the leaving state so Update() calls HandleHouseExit()
            movingToFirstNode = true;     // skip Phase 1 & 2, go straight to Phase 3
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
        if (IsInHouse && !leavingHouse)
        {
            // Bob up and down inside the house
            bobTimer += Time.deltaTime * bobSpeed;
            Vector3 bobOffset = new Vector3(0f, Mathf.Sin(bobTimer) * bobAmplitude, 0f);
            transform.position = houseHomePos + bobOffset;
            return;
        }

        if (leavingHouse)
        {
            HandleHouseExit();
            return;
        }

        // Normal pathfinding movement
        if (current == null || next == null) return;

        transform.position = Vector3.MoveTowards(transform.position, next.WorldPos, speed * Time.deltaTime);

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
        if (houseExitTarget == null) return;

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

        // Phase 1: Align horizontally with exit
        Vector3 centerAlign = new Vector3(houseExitTarget.position.x, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - houseExitTarget.position.x) > exitSnapDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerAlign, houseExitSpeed * Time.deltaTime);
            return;
        }

        // Phase 2: Move up toward the exit point
        transform.position = Vector3.MoveTowards(transform.position, houseExitTarget.position, houseExitSpeed * Time.deltaTime);

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

        if (cameFrom != null && options.Count > 1)
        {
            options.Remove(cameFrom);
        }

        if (mode == Mode.Frightened)
        {
            return options[Random.Range(0, options.Count)];
        }

        Vector3 target = GetTargetWorld();

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
        if (mode == newMode) return;
        mode = newMode;

        if (reverseOnSwitch && previous != null && !IsInHouse && !leavingHouse)
        {
            var temp = previous;
            previous = next;
            next = temp;
        }
    }

    public void ReleaseFromHouse()
    {
        if (!IsInHouse) return;

        IsInHouse = false;
        leavingHouse = true;
    }
}