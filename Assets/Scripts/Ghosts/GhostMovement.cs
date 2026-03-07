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
    [SerializeField] public Transform houseRespawnPoint;
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

    public Mode CurrentMode => mode;
    public bool CanBeEaten => mode == Mode.Frightened && !IsInHouse && !leavingHouse;

    public bool UseChaseOverride { get; set; }
    public Vector3 ChaseTargetOverride { get; set; }

    public Vector2Int CurrentMoveDir { get; private set; } = Vector2Int.left;

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
            IsInHouse = false;
            leavingHouse = true;
            movingToFirstNode = true;
            firstNode = graph.GetClosestNode(transform.position);
        }
        else
        {
            IsInHouse = true;
            leavingHouse = false;
            houseHomePos = houseRespawnPoint ? houseRespawnPoint.position : transform.position;

            // zet hem meteen netjes op home pos als er een respawn point is
            transform.position = houseHomePos;

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

        if (current == null || next == null) return;

        Vector3 before = transform.position;
        Vector3 pos = transform.position;

        Vector3 seg = next.WorldPos - current.WorldPos;

        if (Mathf.Abs(seg.x) > Mathf.Abs(seg.y))
        {
            pos.y = current.WorldPos.y;
            float newX = Mathf.MoveTowards(pos.x, next.WorldPos.x, speed * Time.deltaTime);
            pos.x = newX;
        }
        else
        {
            pos.x = current.WorldPos.x;
            float newY = Mathf.MoveTowards(pos.y, next.WorldPos.y, speed * Time.deltaTime);
            pos.y = newY;
        }

        transform.position = pos;

        Vector3 delta = transform.position - before;
        if (delta.sqrMagnitude > 0.000001f)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                CurrentMoveDir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                CurrentMoveDir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

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

        Vector3 centerAlign = new Vector3(houseExitTarget.position.x, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - houseExitTarget.position.x) > exitSnapDistance)
        {
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, centerAlign, houseExitSpeed * Time.deltaTime);

            Vector3 d = transform.position - before;
            if (d.sqrMagnitude > 0.000001f)
                CurrentMoveDir = d.x > 0 ? Vector2Int.right : Vector2Int.left;

            return;
        }

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
            transform.position = houseExitTarget.position;
            firstNode = graph.GetClosestNode(transform.position);
            movingToFirstNode = true;
        }
    }

    private NodeData ChooseNextNode(NodeData from, NodeData cameFrom)
    {
        var options = new List<NodeData>(from.Neighbors);

        if (options.Count == 0) return cameFrom;

        if (!leavingHouse && cameFrom != null && options.Count > 1)
        {
            options.Remove(cameFrom);
        }

        if (mode == Mode.Frightened)
        {
            if (options.Count == 0) return cameFrom;
            int idx = Random.Range(0, options.Count);
            return options[idx];
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
                if (UseChaseOverride) return ChaseTargetOverride;
                return pacman ? pacman.position : transform.position;

            case Mode.Eaten:
                return houseRespawnPoint ? houseRespawnPoint.position :
                       houseExitTarget ? houseExitTarget.position :
                       transform.position;

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

        if (IsInHouse || leavingHouse || current == null || next == null)
            return;

        Vector3 seg = next.WorldPos - current.WorldPos;
        Vector3 p = transform.position;

        if (Mathf.Abs(seg.x) > Mathf.Abs(seg.y))
            p.y = current.WorldPos.y;
        else
            p.x = current.WorldPos.x;

        transform.position = p;

        if (reverseOnSwitch)
        {
            NodeData temp = current;
            current = next;
            next = temp;
        }
    }

    public void ReleaseFromHouse()
    {
        if (!IsInHouse) return;

        IsInHouse = false;
        leavingHouse = true;
        movingToFirstNode = false;
    }

    public void OnEatenByPacman()
    {
        if (!CanBeEaten) return;

        mode = Mode.Eaten;
        UseChaseOverride = false;

        Vector3 respawnPos =
            houseRespawnPoint ? houseRespawnPoint.position :
            houseExitTarget ? houseExitTarget.position :
            transform.position;

        transform.position = respawnPos;
        houseHomePos = respawnPos;

        IsInHouse = true;
        leavingHouse = false;
        movingToFirstNode = false;

        current = null;
        previous = null;
        next = null;
        firstNode = null;

        bobTimer = Random.Range(0f, Mathf.PI * 2f);

        // zet hem weer op scatter zodat hij later normaal kan releasen
        mode = Mode.Scatter;
    }
}