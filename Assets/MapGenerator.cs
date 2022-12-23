using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public int smoothingIteration;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int FillPercentageCoefficient;
    int[,] map;

    private void Start() {
        GenerateMap();
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            GenerateMap();
        }
    }

    //map generation method
    void GenerateMap() {
        map = new int[width, height];
        RandomFill();

        for (int i = 0; i < smoothingIteration; i++) {
            SmoothingMapEdges();
        }

        ProcessMap();

        int borderSize = 10;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++) {
            for (int y = 0; y < borderedMap.GetLength(1); y++) {
                if (x>= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

    void ProcessMap() {
        List<List<Coordinates>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coordinates> wallRegion in wallRegions) {
            if (wallRegion.Count < wallThresholdSize) {
                foreach (Coordinates tile in wallRegion) {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coordinates>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;

        foreach (List<Coordinates> roomRegion in roomRegions) {
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coordinates tile in roomRegion) {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }

    List<List<Coordinates>> GetRegions(int tileType) {
        List<List<Coordinates>> regions = new List<List<Coordinates>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
                    List<Coordinates> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coordinates tile in newRegion) {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coordinates> GetRegionTiles(int startX, int startY) {
        List<Coordinates> tiles = new List<Coordinates>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX,startY];

        Queue<Coordinates> queue = new Queue<Coordinates>();
        queue.Enqueue(new Coordinates(startX, startY));
        mapFlags [startX,startY] = 1;

        while (queue.Count > 0) {
            Coordinates tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX -1; x <= tile.tileX; x++) {
                for (int y = tile.tileY - 1; y <= tile.tileY; y++) {
                    if (IsInMapRange (x,y) && (y == tile.tileY || x == tile.tileX)) {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coordinates(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //random fill map generation method
    void RandomFill() {
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }

        //Random number generator
        System.Random numberGenerator = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                if (x==0 || x== width-1 || y==0 || y == height -1) {
                    map[x, y] = 1;
                }
                else {
                    //check if random is above or below fill.
                    map[x, y] = numberGenerator.Next(0, 100) < FillPercentageCoefficient ? 1 : 0;
                }
            }
        }
    }

    void SmoothingMapEdges() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4) {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4) {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX -1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (IsInMapRange (neighbourX, neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    struct Coordinates {
        public int tileX;
        public int tileY;

        public Coordinates(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    //private void OnDrawGizmos() {
    //    if (map!=null) {
    //        for (int x = 0; x < width; x++) {
    //            for (int y = 0; y < height; y++) {
    //                if (map[x, y] == 1) {
    //                    Gizmos.color = Color.black;
    //                }
    //                else {
    //                    Gizmos.color = Color.white;
    //                }

    //                // Draw a line on 0.5, 1.5, 2.5 etc, procedurally increasing, 
    //                Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                Gizmos.DrawCube(pos, Vector3.one);
    //            }
    //        }
    //    }
    //}
}
