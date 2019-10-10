using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathManager : MonoBehaviour
{
    public List<GraphNode> graph = new List<GraphNode>();
    public List<MapNode> rooms = new List<MapNode>();
    public Tilemap tilemap;
    
    private class PQitem
    {
        public float totalDist;
        public GraphNode goThrough;
        public GraphNode node;

        public PQitem(GraphNode origin, GraphNode node)
        {
            goThrough = origin;
            this.node = node;
        }
    }
    
    private class PQ
    {
        Vector3Int start;
        Vector3Int end;
        public List<PQitem> queue;
        HashSet<GraphNode> finished;

        public PQ(Vector3Int start, Vector3Int end)
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
    }
    
    public List<Vector3Int> GetPath(Vector3Int start, Vector3Int end)
    {
        PQ pq = new PQ(start,end);
        List<PQitem> doneCons = new List<PQitem>();
        MapNode startRoom = rooms.Find(r => r.InThisRoom(start));
        MapNode endRoom = rooms.Find(r => r.InThisRoom(end));
        GraphNode gStart = new GraphNode(start.x,start.y);
        foreach (GraphNode node in startRoom.nodes) {
            gStart.connections.Add(node);
        }
        GraphNode gEnd = graph[0];
        foreach (GraphNode node in endRoom.nodes) {
            // if (Vector3.Distance(start,node.pos) < Vector3.Distance(gStart.pos,start)) gStart = node;
            if (Vector3.Distance(end,node.pos) < Vector3.Distance(gEnd.pos,start)) gEnd = node;
        }
        pq.AddConnections(gStart);
        while (pq.queue.Count > 0 && !pq.queue[0].node.Equals(gEnd)) {
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
            var prevStep = doneCons.Find(n => n.node.Equals(goThrough));
            path.Insert(0,prevStep);
        }
        List<Vector3Int> vectPath = new List<Vector3Int>();
        vectPath.Add(gStart.pos);
        foreach (PQitem i in path) {
            // i.node
            vectPath.Add(i.node.pos);
            if (endRoom.InThisRoom(i.node.pos)) break;
        }
        vectPath.Add(end);
        return vectPath;
    }

    public List<Vector3> GetPathWorld(Vector3 start, Vector3 end)
    {
        Vector3Int tileStart = tilemap.WorldToCell(start);
        Vector3Int tileEnd = tilemap.WorldToCell(end);
        List<Vector3Int> tilePath = GetPath(tileStart,tileEnd);
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector3Int tilePart in tilePath) {
            worldPath.Add(tilemap.CellToWorld(tilePart));
        }
        return worldPath;
    }
}
