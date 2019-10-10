using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinder
{
    public static List<GraphNode> graph = new List<GraphNode>();
    
    private class PQitem
    {
        public float totalDist;
        public GraphNode goThrough;
        public GraphNode pos;

        public PQitem(GraphNode origin, GraphNode pos)
        {
            goThrough = origin;
            this.pos = pos;
        }
    }
    
    private class PQ
    {
        Vector3 start;
        Vector3 end;
        public List<PQitem> queue;
        Tilemap tilemap;
        HashSet<GraphNode> finished;

        public PQ(Vector3 start, Vector3 end, Tilemap tilemap)
        {
            this.start = start;
            this.end = end;
            this.tilemap = tilemap;
            this.queue = new List<PQitem>();
            this.finished = new HashSet<GraphNode>();
        }

        public void InsertToPQ(PQitem origin, GraphNode pos)
        {
            PQitem pqi = new PQitem(origin.pos,pos);
            pqi.totalDist = 1 + Vector3.Distance(pos.pos,end) + origin.totalDist;
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
            foreach (GraphNode connection in origin.pos.connections) {
                if (!finished.Contains(connection)) InsertToPQ(origin,connection);
            }
            finished.Add(origin.pos);
        }
    }
    
    public static List<Vector3> GetPath(Vector3 start, Vector3 end, Tilemap tilemap)
    {
        PQ pq = new PQ(start,end,tilemap);
        List<PQitem> doneCons = new List<PQitem>();
        GraphNode gStart = graph[0];
        GraphNode gEnd = graph[0];
        foreach (GraphNode node in graph) {
            if (Vector3.Distance(start,node.pos) < Vector3.Distance(gStart.pos,start)) gStart = node;
            if (Vector3.Distance(end,node.pos) < Vector3.Distance(gEnd.pos,start)) gEnd = node;
        }
        gStart = graph[0];
        gEnd = graph[graph.Count-1];
        pq.AddConnections(gStart);
        while (pq.queue.Count > 0 && !pq.queue[0].pos.Equals(gEnd)) {
            var first = pq.queue[0];
            pq.queue.RemoveAt(0);
            pq.AddConnections(first);
            doneCons.Add(first);
        }
        if (pq.queue.Count == 0) return null; // no path found
        var path = new List<PQitem>();
        path.Add(pq.queue[0]);
        while (!path[0].goThrough.Equals(gStart)) {
            var goThrough = path[0].goThrough;
            var prevStep = doneCons.Find(n => n.pos.Equals(goThrough));
            path.Insert(0,prevStep);
        }
        List<Vector3> vectPath = new List<Vector3>();
        vectPath.Add(gStart.pos);
        foreach (PQitem i in path) {
            // i.node
            vectPath.Add(i.pos.pos+new Vector3(0.5f,0.5f,0));
        }
        vectPath.Add(end);
        return vectPath;
    }
}