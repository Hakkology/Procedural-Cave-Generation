using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleIdentity = new Dictionary<int, List<Triangle>>();

    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize) {

        triangleIdentity.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid (map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                Triangulation(squareGrid.squares[x,y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }

    void CreateWallMesh() {

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count-1; i++) {

                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); //left
                wallVertices.Add(vertices[outline[i + 1]]); //right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); //bottomleft
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); //bottomright

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();

        walls.mesh = wallMesh;
    }


    void Triangulation (Square square) {
        switch (square.Configuration) {
            case 0:
                break;

            //1 point
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            //2 points
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            //3 points
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            //4 points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangles(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangles(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangles(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangles(points[0], points[4], points[5]);
    }

    void AssignVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].vertexIndex == -1) {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangles(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {

        if (triangleIdentity.ContainsKey(vertexIndexKey)) {
            triangleIdentity[vertexIndexKey].Add (triangle);
        }
        else {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleIdentity.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines() {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = GetConnectedOurlineVertex(vertexIndex);
                if (newOutlineVertex != -1) {
                    checkedVertices.Add(vertexIndex);

                    List <int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutlines(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count-1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutlines (int vertexIndex, int outlineIndex) {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add (vertexIndex);
        int nextVertexIndex = GetConnectedOurlineVertex (vertexIndex);

        if (nextVertexIndex != -1) {
            FollowOutlines(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOurlineVertex(int vertexIndex) {

        List<Triangle> triangleContainingVertex = triangleIdentity[vertexIndex];

        for (int i = 0; i < triangleContainingVertex.Count; i++) {
            Triangle triangle = triangleContainingVertex[i];

            for (int j = 0; j < 3; j++) {

                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
                    if (IsOutlineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB) {

        List<Triangle> triangleContainingVertexA = triangleIdentity[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < triangleContainingVertexA.Count; i++) {
            if (triangleContainingVertexA[i].Contains(vertexB)) {
                sharedTriangleCount++;
                if (sharedTriangleCount>1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c) {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i] {
            get {
                return vertices[i];
            }
        }

        public bool Contains (int vertexIndex) {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }


    //private void OnDrawGizmos() {
    //    if (squareGrid != null) {
    //        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
    //            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
    //                Gizmos.color = (squareGrid.squares[x,y].topLeft.Active)? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * .4f);

    //                Gizmos.color = (squareGrid.squares[x, y].topRight.Active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * .4f);

    //                Gizmos.color = (squareGrid.squares[x, y].bottomRight.Active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * .4f);

    //                Gizmos.color = (squareGrid.squares[x, y].bottomLeft.Active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * .4f);

    //                Gizmos.color = Color.grey;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * .15f);
    //            }
    //        }
    //    }
    //}


    public class SquareGrid {

        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength (0);
            int nodeCountY = map.GetLength (1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX -1, nodeCountY -1 ];
            for (int x = 0; x < nodeCountX -1; x++) {
                for (int y = 0; y < nodeCountY -1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }


    public class Square {

        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node centreTop, centreBottom, centreLeft, centreRight;
        public int Configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
            
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.Active) {
                Configuration += 8;
            }
            if (topRight.Active) {
                Configuration += 4;
            }
            if (bottomRight.Active) {
                Configuration += 2;
            }
            if (bottomLeft.Active) {
                Configuration += 1;
            }
        }
    }


    public class Node {

        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos) {
            position = _pos;
        }
    }

    public class ControlNode : Node {

        public bool Active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
            Active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }


}
