using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public Vector3Int pos;
    public List<GraphNode> connections;

    public GraphNode(int x, int y)
    {
        this.pos = new Vector3Int(x,y,0);
        connections = new List<GraphNode>();
    }

    public override string ToString()
    {
        return $"({pos.x},{pos.y})";
    }
}

