using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Linq;
using TMPro;

public class NavigationAI:MonoBehaviour {

    public class Node {
        public const int COST_NODE_SOLID = 1000;
        public const int COST_NODE_WALLED = 10;

        public Vector2 position;
        public Vector2Int positionInt;

        public List<Node> neighbors = null;

        //A* variables
        public int tileCost;
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

    [Header("Debug")]
    public bool DebugModeFullNodes = true;
    public bool DebugModeCrossNodes = true;

    public bool isGenerated = false;

    public bool canGoThroughSolid = false;

    public Node[,] graphFull;
    public Node[,] graphCross;

    float cellSize = 1;

    Vector2Int offsetTileMap = Vector2Int.zero;

    public void GenerateNavigationGraphCross(MapTile[,] mapTiles, Vector2Int offset) {
        offsetTileMap = offset;
        isGenerated = false;

        //Get width and height of tilemap
        int width = mapTiles.GetLength(0);
        int height = mapTiles.GetLength(1);

        //If graph does not existe -> instanciate it
        graphCross = new Node[width, height];
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                graphCross[x, y] = null;
            }
        }

        //Go through tilemap to find free tile
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {

                if(mapTiles[x, y].isSolid && !mapTiles[x, y].isOccuped) {
                    graphCross[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                } else if(mapTiles[x, y].isOccuped) {
                    graphCross[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                } else {
                    graphCross[x, y] = new Node {
                        tileCost = mapTiles[x, y].cost,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = false
                    };
                }
            }
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                if (!graphCross[x, y].isSolid) {
                    foreach(Vector3Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue; //Not self node
                        if(x + b.x < 0 || x + b.x >= width || y + b.y < 0 || y + b.y >= height) continue; //Inside bounderies

                        if(graphCross[x + b.x, y + b.y].isSolid && !canGoThroughSolid) continue;
                        
                        if(b.x == 0 || b.y == 0) { //On ajoute uniquement la croix
                            graphCross[x, y].neighbors.Add(graphCross[x + b.x, y + b.y]);
                        }

                    }
                }
            }
        }

        //Add cost if against wall
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (!mapTiles[x, y].isSolid) {

                    foreach(Vector3Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue;
                        if(x + b.x < 0 || x + b.x >= width || y + b.y < 0 || y + b.y >= height) continue;

                        if(graphCross[x + b.x, y + b.y].isSolid) {
                            graphCross[x, y].tileCost = Node.COST_NODE_WALLED;
                        }
                    }
                }
            }
        }

        isGenerated = true;
    }

    public void GenerateNavigationGraphFull(MapTile[,] mapTiles, Vector2Int offset) {
        offsetTileMap = offset;
        isGenerated = false;

        //Get width and height of tilemap
        int width = mapTiles.GetLength(0);
        int height = mapTiles.GetLength(1);

        //If graph does not existe -> instanciate it
        graphFull = new Node[width, height];
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                graphFull[x, y] = null;
            }
        }

        //Go through tilemap to find free tile
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {

                if(mapTiles[x, y].isSolid && !mapTiles[x, y].isOccuped) {
                    graphFull[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                } else if(mapTiles[x, y].isOccuped) {
                    graphFull[x, y] = new Node {
                        tileCost = Node.COST_NODE_SOLID,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = true
                    };
                } else {
                    graphFull[x, y] = new Node {
                        tileCost = mapTiles[x, y].cost,
                        neighbors = new List<Node>(),
                        position = new Vector2(mapTiles[x, y].position.x + cellSize / 2.0f, mapTiles[x, y].position.y + cellSize / 2.0f),
                        positionInt = new Vector2Int(mapTiles[x, y].position.x, mapTiles[x, y].position.y),
                        isSolid = false
                    };
                }
            }
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                if((mapTiles[x, y].isSolid || mapTiles[x, y].isOccuped) && !canGoThroughSolid) continue;
                if(graphFull[x, y] != null) {

                    foreach(Vector3Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue;
                        if(x + b.x < 0 || x + b.x >= width || y + b.y < 0 || y + b.y >= height) continue;
                        if((mapTiles[x + b.x, y + b.y].isSolid || mapTiles[x + b.x, y + b.y].isOccuped) && !canGoThroughSolid) continue;

                        if(graphFull[x + b.x, y + b.y] != null) {
                            if(b.x == 0 || b.y == 0) { //On ajoute automatiquement la croix
                                graphFull[x, y].neighbors.Add(graphFull[x + b.x, y + b.y]);
                            } else { //Si on est en diagonale
                                     //Entre bloc solid il est impossible de voyager de manière diagonale
                                if(graphFull[x + b.x, y].isSolid || graphFull[x, y + b.y].isSolid) continue;

                                graphFull[x, y].neighbors.Add(graphFull[x + b.x, y + b.y]);
                            }
                        }
                    }
                }
            }
        }

        //Add cost if against wall
        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                if(!mapTiles[x, y].isSolid) {

                    foreach(Vector3Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue;
                        if(x + b.x < 0 || x + b.x >= width || y + b.y < 0 || y + b.y >= height) continue;

                        if(graphFull[x + b.x, y + b.y].isSolid) {
                            graphFull[x, y].tileCost = Node.COST_NODE_WALLED;
                        }
                    }
                }
            }
        }

        isGenerated = true;
    }

    public List<Node> GetGraphOnlyFreeTile() {
        List<Node> freeNode = new List<Node>();

        foreach(Node node in graphFull) {
            if(!node.isSolid)
                freeNode.Add(node);
        }

        return freeNode;
    }

    public Node GetRandomPatrolsPoint() {

        while(true) {
            int width = graphFull.GetLength(0);
            int height = graphFull.GetLength(1);
            Node n = graphFull[Random.Range(0, width), Random.Range(0, height)];

            if(!n.isSolid) return n;
        }
    }

    public Node GetClosestNode(Vector2 pos) {

        int x = (int)pos.x + offsetTileMap.x;
        int y = (int)pos.y + offsetTileMap.y;

        Node n = graphFull[x, y];

        if(!n.isSolid) return n;

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        foreach(Vector3Int b in bounds.allPositionsWithin) {
            if(pos.x + b.x >= 0 && pos.x + b.x < graphFull.GetLength(0) && pos.y + b.y >= 0 && pos.y + b.y < graphFull.GetLength(1)) {
                if(!graphFull[x + b.x, y + b.y].isSolid) {
                    n = graphFull[x + b.x, y + b.y];
                }
            }
        }

        return n;
    }

    private void OnDrawGizmos() {
        if (DebugModeFullNodes) {
            if (graphFull != null) {
                foreach (Node node in graphFull) {
                    if(node != null) {
                        switch((int)node.tileCost) {
                            case 1:
                                Gizmos.color = Color.white;
                                break;

                            case Node.COST_NODE_WALLED:
                                Gizmos.color = Color.grey;
                                break;

                            case Node.COST_NODE_SOLID:
                                Gizmos.color = Color.red;
                                break;
                        }

                        Gizmos.DrawWireSphere(new Vector3(node.position.x, node.position.y, 0), 0.1f);

                        foreach(Node neighbor in node.neighbors) {
                            switch((int)neighbor.tileCost) {
                                case 1:
                                    Gizmos.color = Color.white;
                                    if(node.tileCost == 1) {
                                        Gizmos.DrawLine(node.position, neighbor.position);
                                    }

                                    break;

                                case Node.COST_NODE_WALLED:
                                    Gizmos.color = Color.grey;
                                    Gizmos.DrawLine(node.position, neighbor.position);
                                    break;

                                case Node.COST_NODE_SOLID:
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawLine(node.position, neighbor.position);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        if(DebugModeCrossNodes) { 
            if(graphCross != null) {
                foreach(Node node in graphCross) {
                    if(node != null) {
                        switch ((int)node.tileCost) {
                            case 1:
                                Gizmos.color = Color.white;
                                break;

                            case Node.COST_NODE_WALLED:
                                Gizmos.color = Color.grey;
                                break;

                            case Node.COST_NODE_SOLID:
                                Gizmos.color = Color.red;
                                break;
                        }

                        Gizmos.DrawWireSphere(new Vector3(node.position.x, node.position.y, 0), 0.1f);

                        foreach(Node neighbor in node.neighbors) {
                            switch ((int) neighbor.tileCost) {
                                case 1:
                                    Gizmos.color = Color.white;
                                    if (node.tileCost == 1) {
                                        Gizmos.DrawLine(node.position, neighbor.position);
                                    }

                                    break;

                                case Node.COST_NODE_WALLED:
                                    Gizmos.color = Color.grey;
                                    Gizmos.DrawLine(node.position, neighbor.position);
                                    break;

                                case Node.COST_NODE_SOLID:
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawLine(node.position, neighbor.position);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
