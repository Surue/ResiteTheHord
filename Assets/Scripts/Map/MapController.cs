using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController:MonoBehaviour {
    
    [HideInInspector]
    public MapTile[,] tiles;


    [SerializeField]
    Tilemap solidTilemap;
    [SerializeField]
    Tilemap groundTilemap;

    // Use this for initialization
    void Start() {
        SetTiles();
    }

    public void SetTiles(MapTile[,] mapTiles) {
        int width = mapTiles.GetLength(0);
        int height = mapTiles.GetLength(1);

        tiles = new MapTile[width, height];

        for(int x = 0;x < width;x++) {
            for(int y = 0;y < height;y++) {
                tiles[x, y] = mapTiles[x, y];
            }
        }
    }

    public void SetTiles() {
        int width = solidTilemap.size.x;
        int height = solidTilemap.size.y;

        tiles = new MapTile[width, height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Vector3Int pos = new Vector3Int(i + solidTilemap.cellBounds.xMin, j + solidTilemap.cellBounds.yMin, 0);
                if (solidTilemap.HasTile(pos)) {
                    tiles[i, j] = new MapTile(new Vector2Int(pos.x, pos.y), true);
                }else {
                    tiles[i, j] = new MapTile(new Vector2Int(pos.x, pos.y), false);
                }
            }
        }

        FindObjectOfType<NavigationAI>().GenerateNavigationGraph(tiles, new Vector2Int(Mathf.Abs(solidTilemap.cellBounds.x), Mathf.Abs(solidTilemap.cellBounds.y + 1)));
    }

    public MapTile GetTile(Vector2Int pos) {
        if(pos.x >= 0 && pos.x < tiles.GetLength(0) && pos.y >= 0 && pos.y < tiles.GetLength(1)) {
            return tiles[pos.x, pos.y];
        }

        return null;
    }

    public void AttackTile(MapTile t, float damage) {
        if(t.Attack(damage)) {
            UpdateTile(t);
        }
    }

    void UpdateTile(MapTile t) {
        FindObjectOfType<NavigationAI>().GenerateNavigationGraph(tiles, new Vector2Int(Mathf.Abs(solidTilemap.cellBounds.x), Mathf.Abs(solidTilemap.cellBounds.y)));
    }
    
    Vector2Int Transform2TilePos(Transform pos) {
        int x = 0;
        int y = 0;

        x = Mathf.FloorToInt(pos.position.x);
        y = Mathf.FloorToInt(pos.position.y);

        return new Vector2Int(x, y);
    }

    //WRONG
    public static Vector2Int Vector2TilePos(Vector2 pos) {
        int x = 0;
        int y = 0;

        x = Mathf.FloorToInt(pos.x);
        y = Mathf.FloorToInt(pos.y);

        return new Vector2Int(x, y);
    }
}

//Represents a tile of a map, does not take in count the layer of tilemap
public class MapTile {

    [System.Serializable]
    public enum TileType {
        FREE,
        SOLID,
        INVULNERABLE,
        ANY //Used to draw them
    }

    public TileType type;

    public Vector2Int position;

    public float cost = 1;

    public Tile tile;
    public Tile groundTile;
    public Tile decalTile;

    public int score = 5;

    public bool isSolid;
    public bool isInvulnerable;
    public bool isOccuped = false;

    float lifePoint;

    public MapTile(Vector2Int pos, bool solid) {
        position = pos;
        isSolid = solid;

        if(isSolid) {
            lifePoint = 1000;
        }
    }

    public MapTile(MapTile t) {
        position = t.position;
        isSolid = t.isSolid;
        cost = t.cost;
        tile = t.tile;
        score = t.score;
        type = t.type;

        if(isSolid) {
            lifePoint = 1000;
        }
    }

    public void SetType(TileType t) {
        type = t;

        switch(type) {
            case TileType.FREE:
                isInvulnerable = false;
                isSolid = false;
                break;

            case TileType.SOLID:
                isInvulnerable = false;
                isSolid = true;
                break;

            case TileType.INVULNERABLE:
                isInvulnerable = true;
                isSolid = true;
                cost = Mathf.Infinity;
                break;
        }
    }

    public bool Attack(float d) {
        if(!isSolid || isInvulnerable) {
            return false;
        }

        lifePoint -= d;
        if(lifePoint <= 0) {
            SetType(TileType.FREE);

            decalTile = null;

            tile = null;

            return true;
        } else {
            return false;
        }
    }
}