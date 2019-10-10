using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap colliderMap;     // this has collider stuff and it's used everywhere tiles are inaccessible
    public Tilemap frontwallsMap;   // the front bottom edges of all rooms, to give it a quasi 3D look

    [System.Serializable]
    public class TilemapLevel
    {
        public Tilemap baseMap;     // holds the bottom-most tilemap of that level, so mostly floor tiles
        public Tilemap detailMap;   // holds details tiles that sit directly on top of base tiles - decoration
        public Tilemap wallsMap;    // holds the top edge walls of that level
        public Tilemap doorsMap;    // if applicable, holds the doors to the next level. Otherwise null

        public void ClearAll()
        {
            baseMap.ClearAllTiles();
            if (detailMap != null) detailMap.ClearAllTiles();
            if (wallsMap != null) wallsMap.ClearAllTiles();
            if (doorsMap != null) doorsMap.ClearAllTiles();
        }
    }
    public List<TilemapLevel> levels;

    public TileBase groundTile;
    public TileBase frontwallsTile;
    public TileBase pillarTile;
    public TileBase highlightTile;

    public TileBase stairsUpLeftTile;
    public TileBase topRightCollider;
    public TileBase stairsUpRightTile;
    public TileBase topLeftCollider;
    public TileBase rightHalfCollider;
    public TileBase leftHalfCollider;
    public TileBase topHalfCollider;
    public TileBase stairsTopRightCollider;
    public TileBase stairsTopLeftCollider;

    const int blockSize = 9;
    const int mapSize = 5; // how many blocks is one side
    const int blockGap = 3;
    const int bridgeW = 3;

    int[,] blocks;
    
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

        public class Connection
        {
            public MapNode origin;
            public MapNode destination;
            public Vector2Int bridgeStart;
            public Vector2Int bridgeEnd;

            public Connection(MapNode origin, int endX, int endY, bool isSideConnection)
            {
                // coords to the middle of the block it's connecting to
                int middleX = endX*(blockSize+blockGap) + blockSize/2 + 1;
                int middleY = endY*(blockSize+blockGap) + blockSize/2 + 1;
                int edgeX = origin.endX*(blockSize+blockGap)+blockSize;
                int edgeY = origin.endY*(blockSize+blockGap)+blockSize;
                GraphNode node1 = Pathfinder.graph[0];
                GraphNode node2 = Pathfinder.graph[0];
                if (isSideConnection) {
                    bridgeStart = new Vector2Int(edgeX,middleY);
                    bridgeEnd = new Vector2Int(edgeX + blockGap + 1,middleY);
                    node1 = Pathfinder.graph[(origin.endX+endY*mapSize)*4+1];
                    node2 = Pathfinder.graph[(origin.endX+1+endY*mapSize)*4+3];
                }
                else {
                    bridgeStart = new Vector2Int(middleX,edgeY);
                    bridgeEnd = new Vector2Int(middleX,edgeY + blockGap + 1);
                    node1 = Pathfinder.graph[(endX+origin.endY*mapSize)*4+2];
                    node2 = Pathfinder.graph[(endX+(origin.endY+1)*mapSize)*4];
                }
                node1.connections.Add(node2);
                node2.connections.Add(node1);
            }
        }

        public MapNode(int x1, int y1, int x2, int y2)
        {
            startX = x1;
            startY = y1;
            endX = x2;
            endY = y2;
            blockConnections = new List<Vector2Int>();
            connections = new List<Connection>();
            if (x2 + 1 < mapSize) {
                for (int y = y1; y <= y2; y++) ConnectToBlock(x2+1,y,true);
            }
            if (y2 + 1 < mapSize) {
                for (int x = x1; x <= x2; x++) ConnectToBlock(x,y2+1,false);
            }
            centerX = (startX*(blockSize+blockGap) + endX*(blockSize+blockGap)+blockSize) / 2;
            centerY = (startY*(blockSize+blockGap) + endY*(blockSize+blockGap)+blockSize) / 2;
        }

        public void ConnectToBlock(int x, int y, bool isSideConnection)
        {
            blockConnections.Add(new Vector2Int(x,y));
            connections.Add(new Connection(this,x,y,isSideConnection));
        }
    }

    List<MapNode> layout;    // format is ROOM-X-Y-OPTIONS
                            // for example single-0-1, hallup-3-3-2 (2 is height)
        
    public void GenerateMapLayout()
    {
        Pathfinder.graph.Clear();
        for (int i = 0; i < mapSize*mapSize; i++) {
            int x = i % mapSize;
            int y = i / mapSize;
            float cx = x*(blockSize+blockGap) + blockSize/2;
            float cy = y*(blockSize+blockGap) + blockSize/2;
            float half = blockSize/2;
            // clockwise from top
            Pathfinder.graph.Add(new GraphNode(cx,cy-half));
            Pathfinder.graph.Add(new GraphNode(cx+half,cy));
            Pathfinder.graph.Add(new GraphNode(cx,cy+half));
            Pathfinder.graph.Add(new GraphNode(cx-half,cy));
        }
        blocks = new int[mapSize,mapSize];
        layout = new List<MapNode>();
        int counter = 1;
        int visitIndex = 0;
        while (visitIndex < mapSize*mapSize) {
            int block = visitIndex;
            visitIndex++;
            int x = block % mapSize;
            int y = block / mapSize;
            if (blocks[y,x] > 0) continue;
            List<string> possibleShapes = new List<string>();
            possibleShapes.Add("single");
            // possibleShapes.Add("single");
            bool canGoDown = y+1 < mapSize && blocks[y+1,x] == 0;
            bool canGoRight = x+1 < mapSize && blocks[y,x+1] == 0;
            if (canGoDown) possibleShapes.Add("hallwaydown");
            if (canGoDown) possibleShapes.Add("hallwaydown");
            if (canGoRight) possibleShapes.Add("hallwayright");
            if (canGoRight) possibleShapes.Add("hallwayright");
            if (canGoDown && canGoRight) {
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                // maybe make this more likely than hallways (add multiple times)
                // since this can have more variety?
            }
            string shape = possibleShapes[Random.Range(0,possibleShapes.Count)];
            switch (shape) {
                case "single":
                    blocks[y,x] = counter;
                    layout.Add(new MapNode(x,y,x,y));
                    break;
                case "hallwaydown":
                    int height = Random.Range(2, (int)(mapSize*0.8));
                    while (y+height >= mapSize) height--;
                    for (int h = 0; h <= height; h++) {
                        if (blocks[y+h,x] > 0) {
                            height = h-1;
                            break;
                        }
                        blocks[y+h,x] = counter;
                    }
                    if (height >= 1) layout.Add(new MapNode(x,y,x,y+height));
                    else layout.Add(new MapNode(x,y,x,y));
                    break;
                case "hallwayright":
                    int width = Random.Range(2, (int)(mapSize*0.8));
                    while (x+width >= mapSize) width--;
                    for (int w = 0; w <= width; w++) {
                        if (blocks[y,x+w] > 0) {
                            width = w-1;
                            break;
                        }
                        blocks[y,x+w] = counter;
                    }
                    if (width >= 1) layout.Add(new MapNode(x,y,x+width,y));
                    else layout.Add(new MapNode(x,y,x,y));
                    break;
                case "chamber":
                    int sizex = Random.Range(2, (int)(mapSize*0.8));
                    int sizey = Random.Range(2, (int)(mapSize*0.8));
                    // cutting down size so it fits within the map constraints
                    while (x + sizex > mapSize) sizex--;
                    while (y + sizey > mapSize) sizey--;
                    for (int s = 1; s < sizex; s++) {
                        if (blocks[y,x+s] > 0) {
                            sizex = s - 1;
                            break;
                        }
                    }
                    layout.Add(new MapNode(x,y,x+sizex-1,y+sizey-1));
                    for (int y2 = y; y2 < y+sizey; y2++) {
                        for (int x2 = x; x2 < x+sizex; x2++) {
                            blocks[y2,x2] = counter;
                        }
                    }
                    break;
            }
            counter++;
        }
        // within one room connections
        foreach (MapNode mn in layout) {
            List<GraphNode> nodesInRoom = new List<GraphNode>();
            for (int y = mn.startY; y <= mn.endY; y++) {
                // add left edge ones
                if (mn.startX > 0) nodesInRoom.Add(Pathfinder.graph[(mn.startX+y*mapSize)*4+3]);
                // add right edge ones
                if (mn.endX+1 < mapSize) nodesInRoom.Add(Pathfinder.graph[(mn.endX+y*mapSize)*4+1]);
            }
            for (int x = mn.startX; x <= mn.endX; x++) {
                // add top edge ones
                if (mn.startY > 0) nodesInRoom.Add(Pathfinder.graph[(x+mn.startY*mapSize)*4]);
                // add bottom edge ones
                if (mn.endY+1 < mapSize) nodesInRoom.Add(Pathfinder.graph[(x+mn.endY*mapSize)*4+2]);                
            }
            for (int i = 0; i < nodesInRoom.Count; i++) {
                for (int i2 = i+1; i2 < nodesInRoom.Count; i2++) {
                    nodesInRoom[i].connections.Add(nodesInRoom[i2]);
                    nodesInRoom[i2].connections.Add(nodesInRoom[i]);
                }
            }
        }
        Pathfinder.graph.RemoveAll(gn => gn.connections.Count == 0);
        for (int y = 0; y < mapSize; y++) {
            string row = "";
            for (int x = 0; x < mapSize; x++) {
                row += blocks[y,x] + " ";
            }
            Debug.Log(row);
        }

        PruneConnections();

        SetupMap();
        foreach (GraphNode g in Pathfinder.graph) {
            levels[0].wallsMap.SetTile(new Vector3Int((int)g.pos.x,(int)g.pos.y,0),highlightTile);
        }
    }

    void PruneConnections()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        // frontwallsMap.ClearAllTiles();
        // colliderMap.ClearAllTiles(); DONT DO THIS IT somehow turns off the collider component or smth
    }

    void SetupMap()
    {
        // This clears the tiles and adds collider everywhere
        // The collider is removed as you place tiles (with FillGroundTile), so you get edges around everything
        levels[0].ClearAll();
        for (int y = -1; y < (blockSize+blockGap)*mapSize+1; y++) {
            for (int x = -1; x < (blockSize+blockGap)*mapSize+1; x++) {
                colliderMap.SetTile(new Vector3Int(x,y,0),groundTile);
                frontwallsMap.SetTile(new Vector3Int(x,y,0),null);
            }
        }
        SetupRooms();
    }

    void SetupRooms()
    {
        foreach (MapNode node in layout) {
            // set the node's ground tiles first
            int startX = (blockSize+blockGap)*node.startX;
            int startY = (blockSize+blockGap)*node.startY;
            int endX = (blockSize+blockGap)*node.endX+blockSize;
            int endY = (blockSize+blockGap)*node.endY+blockSize;
            for (int y = startY; y < endY; y++) {
                for (int x = startX; x < endX; x++) {
                    FillGroundTile(new Vector3Int(x,y,0),x==startX||y==startY);
                }
            }

            if (node.startX < node.endX && node.startY < node.endY) {
                // chamber
                PopulateChamber(node.startX,node.startY,node.endX-node.startX+1,node.endY-node.startY+1);
            }
            else if (node.startX < node.endX) {
                // hallway side
                PopulateHallwaySide(node.startX,node.startY,node.endX-node.startX+1);
            }
            else if (node.startY < node.endY) {
                // hallway up
                PopulateHallwayUp(node.startX,node.startY,node.endY-node.startY+1);
            }
            else {
                // single
                PopulateSingleRoom(node.startX,node.endX);
            }

            // make the appropriate connections
            foreach (Vector2Int blockCon in node.blockConnections) {
                int x1 = node.endX;
                int y1 = node.endY;
                int x2 = blockCon.x;
                int y2 = blockCon.y;
                if (y2 == y1 + 1) {
                    // bridge going up
                    int sx = (blockSize+blockGap)*x2;
                    int sy = (blockSize+blockGap)*y1;
                    int bridgeX = sx + (blockSize-bridgeW) / 2;
                    for (int cellY = sy+blockSize; cellY < sy+blockSize+blockGap; cellY++) {
                        for (int cellX = bridgeX; cellX < bridgeX + bridgeW; cellX++) {
                            FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==sy+blockSize||cellX==bridgeX);
                        }
                    }
                }
                if (x2 == x1 + 1) {
                    // bridge going right
                    int sx = (blockSize+blockGap)*x1;
                    int sy = (blockSize+blockGap)*y2;
                    int bridgeY = sy + (blockSize-bridgeW) / 2;
                    for (int cellY = bridgeY; cellY < bridgeY+bridgeW; cellY++) {
                        for (int cellX = sx+blockSize; cellX < sx+blockSize+blockGap; cellX++) {
                            FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==bridgeY||cellX==sx+blockSize);
                        }
                    }
                }
            }
        }
    }

    void PopulateSingleRoom(int x, int y)
    {

    }

    void PopulateChamber(int x, int y, int xSide, int ySide)
    {
        return;
        int startX = (blockSize+blockGap)*x+3;
        int startY = (blockSize+blockGap)*y+3;
        MakeElevatedGround(startX,startY,startX+3,startY+7);
        MakeLeftUpStairs(startX,startY-1,4);
    }

    void MakeRightUpStairs(int x, int y, int length)
    {
        for (int i = 0; i < length; i++) {
            levels[0].baseMap.SetTile(new Vector3Int(x,y+i,0),stairsUpRightTile); // stair sprite
        }
        colliderMap.SetTile(new Vector3Int(x-1,y-2,0),topLeftCollider); // right bottom edge
        for (int i = 0; i < length; i++) {
            colliderMap.SetTile(new Vector3Int(x-1,y-1+i,0),null); // removing el. plat. bottom edge
            colliderMap.SetTile(new Vector3Int(x,y+i,0),null); // removing el.plat top edge
        }        
        colliderMap.SetTile(new Vector3Int(x,y+length-1,0),stairsTopLeftCollider); // left stair edge
        colliderMap.SetTile(new Vector3Int(x,y-1,0),stairsTopLeftCollider); // right stair edge
    }

    void MakeLeftUpStairs(int x, int y, int length)
    {
        for (int i = 0; i < length; i++) {
            levels[0].baseMap.SetTile(new Vector3Int(x+i,y,0),stairsUpLeftTile); // stair sprite
        }
        colliderMap.SetTile(new Vector3Int(x-2,y-1,0),topRightCollider); // right bottom edge
        for (int i = 0; i < length; i++) {
            colliderMap.SetTile(new Vector3Int(x-1+i,y-1,0),null); // removing el. plat. bottom edge
            colliderMap.SetTile(new Vector3Int(x+i,y,0),null); // removing el.plat top edge
        }        
        colliderMap.SetTile(new Vector3Int(x+length-1,y,0),stairsTopRightCollider); // left stair edge
        colliderMap.SetTile(new Vector3Int(x-1,y,0),stairsTopRightCollider); // right stair edge
    }

    void MakeElevatedGround(int x1, int y1, int x2, int y2)
    {
        for (int x = x1; x <= x2; x++) {
            // bottom right collider
            colliderMap.SetTile(new Vector3Int(x-1,y1-2,0),topLeftCollider);
            colliderMap.SetTile(new Vector3Int(x,y1-1,0),topLeftCollider);
            colliderMap.SetTile(new Vector3Int(x,y2,0),topLeftCollider);
            if (x == x2) colliderMap.SetTile(new Vector3Int(x,y1-1,0),leftHalfCollider);
            for (int y = y1; y <= y2; y++) {
                levels[0].baseMap.SetTile(new Vector3Int(x,y,0),pillarTile);
                if (x == x1) {
                    // bottom left
                    colliderMap.SetTile(new Vector3Int(x-2,y-1,0),topRightCollider);
                    colliderMap.SetTile(new Vector3Int(x-1,y,0),topRightCollider);
                    if (y == y2) {
                        colliderMap.SetTile(new Vector3Int(x-1,y,0),rightHalfCollider);
                    }
                }
                else if (x == x2) {
                    // top right collider
                    colliderMap.SetTile(new Vector3Int(x,y,0),topRightCollider);
                }
            }
        }
        colliderMap.SetTile(new Vector3Int(x2,y2,0),topHalfCollider);
    }

    void PopulateHallwayUp(int x, int y, int length)
    {
        return;
        // populates a hallway going "up" (north west)
        int startX = (blockSize+blockGap)*x;
        int startY = (blockSize+blockGap)*y;

        for (int y2 = startY; y2 < startY+blockSize*length+blockGap*(length-1); y2+=3) {
            // bottom left edge
            Vector3Int pos = new Vector3Int(startX+1,y2+1,0);
            levels[0].wallsMap.SetTile(pos,pillarTile);
            colliderMap.SetTile(pos,groundTile);
            // top right edge
            pos = new Vector3Int(startX+blockSize-2,y2+1,0);
            levels[0].wallsMap.SetTile(pos,pillarTile);
            colliderMap.SetTile(pos,groundTile);
        }
    }

    void PopulateHallwaySide(int x, int y, int length)
    {
        return;
        // populates a hallway going "right" (north east)
        int startX = (blockSize+blockGap)*x;
        int startY = (blockSize+blockGap)*y;

        for (int x2 = startX; x2 < startX+blockSize*length+blockGap*(length-1); x2+=3) {
            // bottom right edge
            Vector3Int pos = new Vector3Int(x2+1,startY+1,0);
            levels[0].wallsMap.SetTile(pos,pillarTile);
            colliderMap.SetTile(pos,groundTile);
            // top left edge
            pos = new Vector3Int(x2+1,startY+blockSize-2,0);
            levels[0].wallsMap.SetTile(pos,pillarTile);
            colliderMap.SetTile(pos,groundTile);
        }
    }

    void FillGroundTile(Vector3Int pos, bool onEdge)
    {
        levels[0].baseMap.SetTile(pos,groundTile);
        colliderMap.SetTile(pos,null);
        if (onEdge) frontwallsMap.SetTile(new Vector3Int(pos.x-1,pos.y-1,0),frontwallsTile);
    }
}
