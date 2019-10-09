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

    const int blockSize = 10;
    const int mapSize = 5; // how many blocks is one side
    const int blockGap = 2;

    int[,] blocks;

    public void GenerateMapLayout()
    {
        blocks = new int[mapSize,mapSize];
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
            if (canGoDown) possibleShapes.Add("corridordown");
            if (canGoDown) possibleShapes.Add("corridordown");
            if (canGoRight) possibleShapes.Add("corridorright");
            if (canGoRight) possibleShapes.Add("corridorright");
            if (canGoDown && canGoRight) {
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                // maybe make this more likely than corridors (add multiple times)
                // since this can have more variety?
            }
            string shape = possibleShapes[Random.Range(0,possibleShapes.Count)];
            switch (shape) {
                case "single":
                    blocks[y,x] = counter;
                    break;
                case "corridordown":
                    int height = Random.Range(2, (int)(mapSize*0.8));
                    for (int y2 = y; y2+1 < mapSize && y2 < y + height; y2++) {
                        if (blocks[y2,x] > 0) break;
                        blocks[y2,x] = counter;
                    }
                    break;
                case "corridorright":
                    int width = Random.Range(2, (int)(mapSize*0.8));
                    for (int x2 = x; x2+1 < mapSize && x2 < x + width; x2++) {
                        if (blocks[y,x2] > 0) break;
                        blocks[y,x2] = counter;
                    }
                    break;
                case "chamber":
                    int sizex = Random.Range(2, (int)(mapSize*0.8));
                    int sizey = Random.Range(2, (int)(mapSize*0.8));
                    Debug.Log($"Making chamber, original size {sizex} {sizey}");
                    // cutting down size so it fits within the map constraints
                    while (x + sizex > mapSize) sizex--;
                    while (y + sizey > mapSize) sizey--;
                    for (int s = 1; s < sizex; s++) {
                        if (blocks[y,x+s] > 0) {
                            sizex = s - 1;
                            break;
                        }
                    }
                    Debug.Log($"Making chamber, pruned size {sizex} {sizey}");
                    for (int y2 = y; y2 < y+sizey; y2++) {
                        for (int x2 = x; x2 < x+sizex; x2++) {
                            blocks[y2,x2] = counter;
                        }
                    }
                    break;
            }
            counter++;
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
                        int bridgeX = startX + Random.Range(0,blockSize-2);
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = bridgeX; cellX < bridgeX + 2; cellX++) {
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
                        int bridgeY = startY + Random.Range(0,blockSize-2);
                        for (int cellY = bridgeY; cellY < bridgeY+2; cellY++) {
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
    }

    void FillGroundTile(Vector3Int pos, bool onEdge)
    {
        levels[0].baseMap.SetTile(pos,groundTile);
        colliderMap.SetTile(pos,null);
        if (onEdge) frontwallsMap.SetTile(new Vector3Int(pos.x-1,pos.y-1,0),frontwallsTile);
    }
}
