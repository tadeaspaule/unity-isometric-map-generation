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
    }
    public List<TilemapLevel> levels;

    public TileBase groundTile;

    
    // Start is called before the first frame update
    void Start()
    {
        frontwallsMap.ClearAllTiles();
        colliderMap.ClearAllTiles();
    }

    public void SetupMap(int[,] blocks, int blockSize, int blockGap, int mapSize)
    {
        Tilemap baseMap = levels[0].baseMap;
        baseMap.ClearAllTiles();
        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                int startX = (blockSize+blockGap)*x;
                int startY = (blockSize+blockGap)*y;
                for (int cellY = startY; cellY < startY + blockSize; cellY++) {
                    for (int cellX = startX; cellX < startX + blockSize; cellX++) {
                        baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
                    }
                }
                // connect block above
                if (y+1 < mapSize) {
                    if (blocks[y+1,x] == blocks[y,x]) {
                        // same room -> fill gap
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = startX; cellX < startX + blockSize; cellX++) {
                                baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
                            }
                        }
                    }
                    else {
                        // different room -> draw bridge
                        int bridgeX = startX + Random.Range(0,blockSize-2);
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = bridgeX; cellX < bridgeX + 2; cellX++) {
                                baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
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
                                baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
                            }
                        }
                    }
                    else {
                        // different room -> draw bridge
                        int bridgeY = startY + Random.Range(0,blockSize-2);
                        for (int cellY = bridgeY; cellY < bridgeY+2; cellY++) {
                            for (int cellX = startX+blockSize; cellX < startX+blockSize+blockGap; cellX++) {
                                baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
                            }
                        }
                    }
                }
                // fill empty gaps in the middle of chambers
                if (x+1 < mapSize && y+1 < mapSize) {
                    if (blocks[y,x+1] == blocks[y+1,x] && blocks[y,x+1] == blocks[y,x]) {
                        for (int cellY = startY+blockSize; cellY < startY+blockSize+blockGap; cellY++) {
                            for (int cellX = startX+blockSize; cellX < startX+blockSize+blockGap; cellX++) {
                                baseMap.SetTile(new Vector3Int(cellX,cellY,0),groundTile);
                            }
                        }
                    }
                }
            }
        }
    }
}
