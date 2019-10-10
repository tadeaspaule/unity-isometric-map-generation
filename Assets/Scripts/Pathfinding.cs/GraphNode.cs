using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public Vector3 pos;
    public List<GraphNode> connections;

    public GraphNode(float x, float y)
    {
        this.pos = new Vector3(x,y,0f);
        connections = new List<GraphNode>();
    }

    public override string ToString()
    {
        return $"({pos.x},{pos.y})";
    }
}

