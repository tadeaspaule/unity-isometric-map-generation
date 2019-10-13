using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{    
    Vector3Int start;
    Vector3Int end;
    List<PQitem> queue;
    HashSet<GraphNode> finished;

    public PriorityQueue(Vector3Int start, Vector3Int end)
    {
        this.start = start;
        this.end = end;
        this.queue = new List<PQitem>();
        this.finished = new HashSet<GraphNode>();
    }

    public void InsertToPQ(PQitem origin, GraphNode node)
    {
        PQitem pqi = new PQitem(origin.node,node);
        pqi.totalDist = 1 + Vector3.Distance(node.pos,end) + origin.totalDist;
        for (var i = 0; i < queue.Count; i++) {
            if (pqi.totalDist < queue[i].totalDist) {
                queue.Insert(i, pqi);
                return;
            }
        }
        queue.Add(pqi);
    }

    public void AddConnections(GraphNode origin)
    {
        foreach (GraphNode connection in origin.connections) {
            if (!finished.Contains(connection)) InsertToPQ(new PQitem(origin,origin),connection);
        }
        finished.Add(origin);
    }

    public void AddConnections(PQitem origin)
    {
        foreach (GraphNode connection in origin.node.connections) {
            if (!finished.Contains(connection)) InsertToPQ(origin,connection);
        }
        finished.Add(origin.node);
    }

    public int Count()
    {
        return queue.Count;
    }

    public PQitem First()
    {
        return queue[0];
    }

    public PQitem RemoveFirst()
    {
        PQitem first = queue[0];
        queue.RemoveAt(0);
        return first;
    }
}