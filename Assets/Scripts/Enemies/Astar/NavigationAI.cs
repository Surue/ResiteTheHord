using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Linq;

public class NavigationAI:MonoBehaviour {

    public class Node {
        public static float COST_NODE_SOLID = 1000;

        public Vector2 position;
        public Vector2Int positionInt;

        public List<Node> neighbors = null;

        //A* variables
        public float tileCost;
        public float cost;
        public float totalCost;
        public Node parent = null;
        public bool visited = false;
        public bool isSolid = false;

        public void SetTotalCost(float f) {
            totalCost = f;
        }

        public void SetParent(Node p) {
            parent = p;
        }

        public void SetCost(float f) {
            cost += f;
        }

        public void Reset() {
            visited = false;
            cost = tileCost;
            totalCost = 0;
            parent = null;
        }
    }

    public bool DebugMode = true;

    public bool isGenerated = false;

    public bool canGoThroughSolid = false;

    public Node[,] graph;

    float cellSize = 1;

    Vector2Int offsetTileMap = Vector2Int.zero;

    public void GenerateNavigationGraph(MapTile[,] mapTiles, Vector2Int offset) {
        offsetTileMap = offset;
        isGenerated = false;

        //Get width and height of tilemap
        int width = mapTiles.GetLength(0);
        int height = mapTiles.GetLength(1);

        //If graph does not existe -> instanciate it
        graph = new Node[width, height];
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                graph[x, y] = null;
            }
        }

        //Go through tilemap to find free tile
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {

                if(!mapTiles[x, y].isSolid && !mapTiles[x, y].isOccuped) {
                    graph[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y)
                    };
                } else if(mapTiles[x, y].isOccuped) {
                    graph[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                } else {
                    graph[x, y] = new Node {
                        tileCost = mapTiles[x, y].cost,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                }
            }
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                if((mapTiles[x, y].isSolid || mapTiles[x, y].isOccuped) && !canGoThroughSolid) continue;
                if(graph[x, y] != null) {

                    foreach(Vector3Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue;
                        if(x + b.x < 0 || x + b.x >= width || y + b.y < 0 || y + b.y >= height) continue;
                        if((mapTiles[x + b.x, y + b.y].isSolid || mapTiles[x + b.x, y + b.y].isOccuped) && !canGoThroughSolid) continue;

                        if(graph[x + b.x, y + b.y] != null) {
                            if(b.x == 0 || b.y == 0) { //On ajoute automatiquement la croix
                                graph[x, y].neighbors.Add(graph[x + b.x, y + b.y]);
                            } else { //Si on est en diagonale
                                     //Entre bloc solid il est impossible de voyager de manière diagonale
                                if(graph[x + b.x, y].isSolid || graph[x, y + b.y].isSolid) continue;

                                graph[x, y].neighbors.Add(graph[x + b.x, y + b.y]);
                            }
                        }
                    }
                }
            }
        }

        isGenerated = true;
    }

    public List<Node> GetGraphOnlyFreeTile() {
        List<Node> freeNode = new List<Node>();

        foreach(Node node in graph) {
            if(!node.isSolid)
                freeNode.Add(node);
        }

        return freeNode;
    }

    public Node GetRandomPatrolsPoint() {

        while(true) {
            int width = graph.GetLength(0);
            int height = graph.GetLength(1);
            Node n = graph[Random.Range(0, width), Random.Range(0, height)];

            if(!n.isSolid) return n;
        }
    }

    public Node GetClosestNode(Vector2 pos) {

        int x = (int)pos.x + offsetTileMap.x;
        int y = (int)pos.y + offsetTileMap.y;

        Node n = graph[x, y];

        if(!n.isSolid) return n;

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        foreach(Vector3Int b in bounds.allPositionsWithin) {
            if(pos.x + b.x >= 0 && pos.x + b.x < graph.GetLength(0) && pos.y + b.y >= 0 && pos.y + b.y < graph.GetLength(1)) {
                if(!graph[x + b.x, y + b.y].isSolid) {
                    n = graph[x + b.x, y + b.y];
                }
            }
        }

        return n;
    }

    private void OnDrawGizmos() {
        if(DebugMode) {
            if(graph != null) {
                foreach(Node node in graph) {
                    if(node != null) {
                        Gizmos.color = node.tileCost == Node.COST_NODE_SOLID ? Color.white : Color.red;

                        Gizmos.DrawWireSphere(new Vector3(node.position.x, node.position.y, 0), 0.1f);

                        foreach(Node neighbor in node.neighbors) {
                            Gizmos.DrawLine(node.position, neighbor.position);
                        }
                    }
                }
            }
        }
    }
}
