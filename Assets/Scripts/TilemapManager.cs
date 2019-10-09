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

    int[,] blocks;
    List<string> layout;    // format is ROOM-X-Y-OPTIONS
                            // for example single-0-1, hallup-3-3-2 (2 is height)

    public void GenerateMapLayout()
    {
        blocks = new int[mapSize,mapSize];
        layout = new List<string>();
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
            possibleShapes.Add("single");
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
                    layout.Add($"single-{x}-{y}");
                    break;
                case "hallwaydown":
                    int height = Random.Range(2, (int)(mapSize*0.8));
                    while (y+height >= mapSize) height--;
                    for (int h = 0; y+h <= mapSize && h <= height; h++) {
                        if (blocks[y+h,x] > 0) {
                            height = h-1;
                            break;
                        }
                        blocks[y+h,x] = counter;
                    }
                    if (height >= 1) layout.Add($"hallup-{x}-{y}-{height+1}");
                    else layout.Add($"single-{x}-{y}");
                    break;
                case "hallwayright":
                    int width = Random.Range(2, (int)(mapSize*0.8));
                    while (x+width >= mapSize) width--;
                    for (int w = 0; x+w <= mapSize && w <= width; w++) {
                        if (blocks[y,x+w] > 0) {
                            width = w-1;
                            break;
                        }
                        blocks[y,x+w] = counter;
                    }
                    if (width >= 1) layout.Add($"hallside-{x}-{y}-{width+1}");
                    else layout.Add($"single-{x}-{y}");
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
                    layout.Add($"chamber-{x}-{y}-{sizex}-{sizey}");
                    for (int y2 = y; y2 < y+sizey; y2++) {
                        for (int x2 = x; x2 < x+sizex; x2++) {
                            blocks[y2,x2] = counter;
                        }
                    }
                    break;
            }
            counter++;
        }
        for (int y = 0; y < mapSize; y++) {
            string row = "";
            for (int x = 0; x < mapSize; x++) {
                row += blocks[y,x] + " ";
            }
            Debug.Log(row);
        }
        SetupMap();
    }

    // Start is called before the first frame update
    void Start()
    {
        // frontwallsMap.ClearAllTiles();
        // colliderMap.ClearAllTiles(); DONT DO THIS IT somehow turns off the collider component or smth
    }

    public void SetupMap()
    {
        levels[0].ClearAll();
        for (int y = -1; y < (blockSize+blockGap)*mapSize+1; y++) {
            for (int x = -1; x < (blockSize+blockGap)*mapSize+1; x++) {
                colliderMap.SetTile(new Vector3Int(x,y,0),groundTile);
                frontwallsMap.SetTile(new Vector3Int(x,y,0),null);
            }
        }
        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                int startX = (blockSize+blockGap)*x;
                int startY = (blockSize+blockGap)*y;
                for (int cellY = startY; cellY < startY + blockSize; cellY++) {
                    for (int cellX = startX; cellX < startX + blockSize; cellX++) {
                        FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==startY||cellX==startX);
                    }
                }
                // connect block above
                if (y+1 < mapSize) {
                    if (blocks[y+1,x] == blocks[y,x]) {
                        // same room -> fill gap
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = startX; cellX < startX + blockSize; cellX++) {
                                FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==startY||cellX==startX);
                            }
                        }
                    }
                    else {
                        // different room -> draw bridge
                        int bridgeX = startX + 3;
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = bridgeX; cellX < bridgeX + 3; cellX++) {
                                FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==startY+blockSize||cellX==bridgeX);
                            }
                        }
                    }
                }
                // connect block to the right
                if (x+1 < mapSize) {
                    if (blocks[y,x+1] == blocks[y,x]) {
                        // same room -> fill gap
                        for (int cellY = startY; cellY < startY+blockSize; cellY++) {
                            for (int cellX = startX+blockSize; cellX < startX + blockSize+blockGap; cellX++) {
                                FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==startY||cellX==startX);
                            }
                        }
                    }
                    else {
                        // different room -> draw bridge
                        int bridgeY = startY + 3;
                        for (int cellY = bridgeY; cellY < bridgeY+3; cellY++) {
                            for (int cellX = startX+blockSize; cellX < startX+blockSize+blockGap; cellX++) {
                                FillGroundTile(new Vector3Int(cellX,cellY,0),cellY==bridgeY||cellX==startX+blockSize);
                            }
                        }
                    }
                }
                // fill empty gaps in the middle of chambers
                if (x+1 < mapSize && y+1 < mapSize) {
                    if (blocks[y,x+1] == blocks[y+1,x] && blocks[y,x+1] == blocks[y,x]) {
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = startX+blockSize; cellX < startX+blockSize+blockGap; cellX++) {
                                FillGroundTile(new Vector3Int(cellX,cellY,0),false);
                            }
                        }
                    }
                }
            }
        }
        SetupRooms();
    }

    void SetupRooms()
    {
        foreach (string roomShape in layout) {
            Debug.Log(roomShape);
            string[] parts = roomShape.Split('-');
            int x = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);
            switch (parts[0]) {
                case "single":
                    PopulateSingleRoom(x,y);
                    break;
                case "hallup":
                    PopulateHallwayUp(x,y,int.Parse(parts[3]));
                    break;
                case "hallside":
                    PopulateHallwaySide(x,y,int.Parse(parts[3]));
                    break;
                case "chamber":
                    PopulateChamber(x,y,int.Parse(parts[3]),int.Parse(parts[4]));
                    break;
            }
        }
    }

    void PopulateSingleRoom(int x, int y)
    {

    }

    void PopulateChamber(int x, int y, int xSide, int ySide)
    {
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
