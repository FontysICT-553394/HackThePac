using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeGraphBuilder : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap nodeTilemap;   // jouw "Node"
    [SerializeField] private Tilemap wallsTilemap;  // jouw "walls"

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    public IReadOnlyDictionary<Vector3Int, NodeData> Nodes => nodes;

    private readonly Dictionary<Vector3Int, NodeData> nodes = new();

    private static readonly Vector3Int[] Dir4 =
    {
        new Vector3Int( 1, 0, 0), // Right
        new Vector3Int(-1, 0, 0), // Left
        new Vector3Int( 0, 1, 0), // Up
        new Vector3Int( 0,-1, 0), // Down
    };

    private void Awake()
    {
        Build();
    }

    [ContextMenu("Build Graph")]
    public void Build()
    {
        nodes.Clear();

        if (!nodeTilemap || !wallsTilemap)
        {
            Debug.LogError("NodeGraphBuilder: nodeTilemap of wallsTilemap ontbreekt.");
            return;
        }

        nodeTilemap.CompressBounds();
        var b = nodeTilemap.cellBounds;

        // 1) alle node-cellen verzamelen
        for (int x = b.xMin; x < b.xMax; x++)
            for (int y = b.yMin; y < b.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (nodeTilemap.HasTile(cell))
                {
                    nodes[cell] = new NodeData(cell, nodeTilemap.GetCellCenterWorld(cell));
                }
            }

        // 2) connecties leggen: scan in 4 richtingen tot wall of volgende node
        foreach (var kvp in nodes)
        {
            var fromCell = kvp.Key;
            var fromNode = kvp.Value;

            foreach (var dir in Dir4)
            {
                var hit = ScanToNextNode(fromCell, dir);
                if (hit.HasValue)
                {
                    var toCell = hit.Value;
                    if (nodes.TryGetValue(toCell, out var toNode))
                    {
                        fromNode.Neighbors.Add(toNode);
                    }
                }
            }
        }

        Debug.Log($"NodeGraphBuilder: Built {nodes.Count} nodes.");
    }

    private Vector3Int? ScanToNextNode(Vector3Int start, Vector3Int dir)
    {
        var cell = start + dir;

        // loop door corridor
        while (true)
        {
            // Wall? stop
            if (wallsTilemap.HasTile(cell))
                return null;

            // Node gevonden? connect
            if (nodes.ContainsKey(cell))
                return cell;

            // anders verder
            cell += dir;

            // veiligheid (als bounds groot/rare maps)
            if (Mathf.Abs(cell.x - start.x) + Mathf.Abs(cell.y - start.y) > 500)
                return null;
        }
    }

    public NodeData GetClosestNode(Vector3 worldPos)
    {
        // world -> cell
        var cell = nodeTilemap.WorldToCell(worldPos);

        // als exact op node:
        if (nodes.TryGetValue(cell, out var exact))
            return exact;

        // anders: brute force dichtstbij (prima voor Pac-Man schaal)
        NodeData best = null;
        float bestD = float.PositiveInfinity;

        foreach (var n in nodes.Values)
        {
            float d = (n.WorldPos - worldPos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = n;
            }
        }

        return best;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || nodes.Count == 0) return;

        Gizmos.color = Color.cyan;
        foreach (var n in nodes.Values)
        {
            Gizmos.DrawSphere(n.WorldPos, 0.08f);

            Gizmos.color = Color.cyan;
            foreach (var nb in n.Neighbors)
            {
                Gizmos.DrawLine(n.WorldPos, nb.WorldPos);
            }
        }
    }
}

public class NodeData
{
    public Vector3Int Cell { get; }
    public Vector3 WorldPos { get; }
    public List<NodeData> Neighbors { get; } = new();

    public NodeData(Vector3Int cell, Vector3 worldPos)
    {
        Cell = cell;
        WorldPos = worldPos;
    }
}