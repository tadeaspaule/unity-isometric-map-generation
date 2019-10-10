using UnityEngine;
using System.Collections.Generic;

public class MapNode
{
    public int startX;
    public int endX;
    public int startY;
    public int endY;
    public float centerX;
    public float centerY;
    public List<Connection> connections;
    public List<Vector2Int> blockConnections;
    public List<GraphNode> nodes;

    public static int blockSize;
    public static int blockGap;
    public static int mapSize;

    public class Connection
    {
        public MapNode origin;
        public MapNode destination;
        public Vector2Int bridgeStart;
        public Vector2Int bridgeEnd;

        public Connection(MapNode origin, int endX, int endY, bool isSideConnection, PathManager pathManager)
        {
            // coords to the middle of the block it's connecting to
            int middleX = endX*(blockSize+blockGap) + blockSize/2 + 1;
            int middleY = endY*(blockSize+blockGap) + blockSize/2 + 1;
            int edgeX = origin.endX*(blockSize+blockGap)+blockSize;
            int edgeY = origin.endY*(blockSize+blockGap)+blockSize;
            GraphNode node1 = pathManager.graph[0];
            GraphNode node2 = pathManager.graph[0];
            if (isSideConnection) {
                bridgeStart = new Vector2Int(edgeX,middleY);
                bridgeEnd = new Vector2Int(edgeX + blockGap + 1,middleY);
                node1 = pathManager.graph[(origin.endX+endY*mapSize)*4+1];
                node2 = pathManager.graph[(origin.endX+1+endY*mapSize)*4+3];
            }
            else {
                bridgeStart = new Vector2Int(middleX,edgeY);
                bridgeEnd = new Vector2Int(middleX,edgeY + blockGap + 1);
                node1 = pathManager.graph[(endX+origin.endY*mapSize)*4+2];
                node2 = pathManager.graph[(endX+(origin.endY+1)*mapSize)*4];
            }
            node1.connections.Add(node2);
            node2.connections.Add(node1);
        }
    }

    public MapNode(int x1, int y1, int x2, int y2, PathManager pathManager)
    {
        startX = x1;
        startY = y1;
        endX = x2;
        endY = y2;
        blockConnections = new List<Vector2Int>();
        connections = new List<Connection>();
        if (x2 + 1 < mapSize) {
            for (int y = y1; y <= y2; y++) ConnectToBlock(x2+1,y,true,pathManager);
        }
        if (y2 + 1 < mapSize) {
            for (int x = x1; x <= x2; x++) ConnectToBlock(x,y2+1,false,pathManager);
        }
        centerX = (startX*(blockSize+blockGap) + endX*(blockSize+blockGap)+blockSize) / 2;
        centerY = (startY*(blockSize+blockGap) + endY*(blockSize+blockGap)+blockSize) / 2;
    }

    public bool InThisRoom(Vector3Int tile)
    {
        int x1 = (blockSize+blockGap)*startX;
        int y1 = (blockSize+blockGap)*startY;
        int x2 = (blockSize+blockGap)*endX+blockSize;
        int y2 = (blockSize+blockGap)*endY+blockSize;
        return (tile.x >= x1 && tile.x < x2 && tile.y >= y1 && tile.y < y2);
    }

    public void ConnectToBlock(int x, int y, bool isSideConnection, PathManager pathManager)
    {
        blockConnections.Add(new Vector2Int(x,y));
        connections.Add(new Connection(this,x,y,isSideConnection,pathManager));
    }
}
