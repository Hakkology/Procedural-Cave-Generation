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

    //map generation method
    void GenerateMap() {
        map = new int[width, height];
        RandomFill();

        for (int i = 0; i < smoothingIteration; i++) {
            SmoothingMapEdges();
        }

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
                if (neighbourX >=0 && neighbourX < width && neighbourY >=0 && neighbourY < height) {
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
