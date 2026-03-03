using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    public enum Mode { Scatter, Chase, Frightened, Eaten }

    [Header("House")]
    [SerializeField] private Transform houseExitTarget; // empty object op de node NET buiten de deur
    [SerializeField] private float exitSnapDistance = 0.05f;

    public bool IsInHouse { get; private set; } = true;
    private bool leavingHouse;

    [Header("Refs")]
    [SerializeField] private NodeGraphBuilder graph;
    [SerializeField] private Transform pacman;

    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private Mode mode = Mode.Chase;

    [Header("Scatter Target (wereldpos)")]
    [SerializeField] private Vector2 scatterCornerWorld = new Vector2(12, 10);

    // node state
    private NodeData current;
    private NodeData previous;
    private NodeData next;

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
        if (!graph) { Debug.LogError("GhostMovement: graph ontbreekt."); enabled = false; return; }

        // Alleen snap naar graph als je NIET in house start (of als je house nodes hebt)
        current = graph.GetClosestNode(transform.position);

        // Als je wél house nodes hebt, is dit goed:
        transform.position = current.WorldPos;

        previous = null;

        // Als hij nog in house zit: laat hem binnen bewegen totdat hij released is
        next = ChooseNextNode(current, previous);
    }

    private void Update()
    {
        if (current == null || next == null) return;

        // move
        transform.position = Vector3.MoveTowards(transform.position, next.WorldPos, speed * Time.deltaTime);

        // arrived at next node
        if ((transform.position - next.WorldPos).sqrMagnitude < 0.0001f)
        {
            transform.position = next.WorldPos;
            previous = current;
            current = next;
            next = ChooseNextNode(current, previous);
        }

        // Als we leavingHouse zijn: check of we bij exit zijn
        if (leavingHouse && houseExitTarget != null)
        {
            if ((transform.position - houseExitTarget.position).sqrMagnitude <= exitSnapDistance * exitSnapDistance)
            {
                leavingHouse = false;
                // vanaf hier doet hij normaal scatter/chase keuzes
            }
        }
    }

    private NodeData ChooseNextNode(NodeData from, NodeData cameFrom)
    {
        var options = new List<NodeData>(from.Neighbors);

        if (options.Count == 0) return cameFrom; // edge case

        // verwijder reverse (niet omkeren), tenzij dead-end
        if (cameFrom != null && options.Count > 1)
        {
            options.Remove(cameFrom);
        }

        // frightened = random (maar nog steeds niet reverse tenzij dead-end)
        if (mode == Mode.Frightened)
        {
            return options[Random.Range(0, options.Count)];
        }

        Vector3 target = GetTargetWorld();

        // kies optie met kleinste afstand naar target; bij gelijk: tie-break via richting (Up,Left,Down,Right)
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
                // tie-break: kijk welke richting "eerder" komt in TieBreakOrder
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
        // Eerst naar de uitgang als we net released zijn
        if (leavingHouse && houseExitTarget != null)
            return houseExitTarget.position;

        switch (mode)
        {
            case Mode.Scatter:
                return scatterCornerWorld;
            case Mode.Chase:
            default:
                return pacman ? pacman.position : transform.position;
        }
    }

    private static Vector2Int WorldDir(Vector3 from, Vector3 to)
    {
        Vector2 delta = to - from;

        // nodes liggen grid-aligned, dus delta is ongeveer (±n,0) of (0,±n)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x >= 0 ? Vector2Int.right : Vector2Int.left;
        else
            return delta.y >= 0 ? Vector2Int.up : Vector2Int.down;
    }

    private static int CompareDirPriority(Vector2Int a, Vector2Int b)
    {
        int pa = PriorityIndex(a);
        int pb = PriorityIndex(b);
        return pa.CompareTo(pb);
    }

    private static int PriorityIndex(Vector2Int d)
    {
        for (int i = 0; i < TieBreakOrder.Length; i++)
            if (TieBreakOrder[i] == d) return i;
        return 999;
    }

    // Handig om later modes te switchen (en dan reverse te doen)
    public void SetMode(Mode newMode, bool reverseOnSwitch)
    {
        if (mode == newMode) return;

        mode = newMode;

        if (reverseOnSwitch && previous != null)
        {
            // direct omkeren: swap next/previous
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