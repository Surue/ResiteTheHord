using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class MapGenerator: NetworkBehaviour {

    class Room {
        public MapTile[,] roomTiles;

        public Room[] childRooms = new Room[4];

        public bool divided;

        public bool isFinal = false;

        public bool isConnected = false;
    }

    #region Rules

    [Header("Generals Informations")]
    [SerializeField] Vector2Int mapSize;

    [Header("BSP rules")]
    [SerializeField] int BSP_minSize = 10;
    [SerializeField] int BSP_maxSize = 20;
    [SerializeField] [Range(0, 100)] float splitingFactor = 35;

    [Header("Room rules")]
    [SerializeField] int minWallSize = 1;
    [SerializeField] int maxWallSize = 3;

    [Header("Corridores rules")]
    [SerializeField] int corridorsSize = 3;

    [Header("Exterior wall rules")]
    [SerializeField] int exteriorWallSize = 10;

    [Header("Tiles")]
    [SerializeField] RuleTile solidTile;
    [SerializeField] RuleTile groundTile;
    [SerializeField] RuleTile motherChipTile;
    [SerializeField] RuleTile enemySpawnerTile;
    [SerializeField] RuleTile playerSpawnerTile;

    [Header("Gameobject to spawn")]
    [SerializeField] GameObject mainCore;
    [SerializeField] GameObject enemySpawner;
    [SerializeField] int nbPlayerSpawer = 4;
    [SerializeField] GameObject playerSpawner;
    #endregion

    List<Room> roomQuadTree;

    MapController mapController;

    #region Debug
    List<Color> colors;

    [Header("Debug")]
    [SerializeField] bool showBSP = true;
    [SerializeField] bool showWall = true;
    [SerializeField] bool showExteriorWall = true;
    #endregion

    public bool isGenerating = false;

    void Awake() {
        roomQuadTree = new List<Room>();
        mapController = GetComponent<MapController>();

        //colors = new List<Color>();

        //for(int i = 0;i < 1000;i++) {
        //    colors.Add(new Color(Random.value, Random.value, Random.value));
        //}
    }

    IEnumerator BinarySpacePartitioning() {
        //General map size and tiles
        MapTile [,] tiles = new MapTile[mapSize.x, mapSize.y];

        for(int x = 0;x < mapSize.x;x++) {
            for(int y = 0;y < mapSize.y;y++) {
                tiles[x, y] = new MapTile(new Vector2Int(x, y), true);
            }
        }

        //Main room
        Room r = new Room();
        r.roomTiles = tiles;
        r.divided = false;

        roomQuadTree.Add(r);

        //Separate Room
        List<Room> roomToAdd = new List<Room>();

        bool hasDivide = false;
        do {
            hasDivide = false;

            roomToAdd = new List<Room>();

            foreach(Room room in roomQuadTree) {
                if(!room.divided) {
                    //Check if can divide
                    if((room.roomTiles.GetLength(0) <= BSP_minSize * 2 || room.roomTiles.GetLength(1) <= BSP_minSize * 2) ||
                       ((room.roomTiles.GetLength(0) < BSP_maxSize && room.roomTiles.GetLength(1) < BSP_maxSize) && RandomSeed.GetValue() > splitingFactor / 100f)) {
                        room.divided = true;
                        room.isFinal = true;
                    } else {
                        //Division
                        Room[] newRooms = new Room[4];
                        newRooms[0] = new Room();
                        newRooms[1] = new Room();
                        newRooms[2] = new Room();
                        newRooms[3] = new Room();

                        //Horizontal division
                        int splitValueVertical = Mathf.Clamp((int)(RandomSeed.GetValue() * room.roomTiles.GetLength(0)), BSP_minSize,
                            room.roomTiles.GetLength(0) - BSP_minSize);

                        //Vertical division
                        int splitValueHorizontal = Mathf.Clamp((int)(RandomSeed.GetValue() * room.roomTiles.GetLength(1)), BSP_minSize,
                            room.roomTiles.GetLength(1) - BSP_minSize);

                        newRooms[0].roomTiles = new MapTile[splitValueVertical, splitValueHorizontal];
                        newRooms[1].roomTiles = new MapTile[room.roomTiles.GetLength(0) - splitValueVertical, splitValueHorizontal];
                        newRooms[2].roomTiles = new MapTile[splitValueVertical, room.roomTiles.GetLength(1) - splitValueHorizontal];
                        newRooms[3].roomTiles = new MapTile[room.roomTiles.GetLength(0) - splitValueVertical, room.roomTiles.GetLength(1) - splitValueHorizontal];

                        newRooms[0].divided = false;
                        newRooms[1].divided = false;
                        newRooms[2].divided = false;
                        newRooms[3].divided = false;

                        for(int i = 0;i < room.roomTiles.GetLength(0);i++) {
                            for(int j = 0;j < room.roomTiles.GetLength(1);j++) {

                                if(i < splitValueVertical) {
                                    if(j < splitValueHorizontal) {
                                        newRooms[0].roomTiles[i, j] = room.roomTiles[i, j];
                                    } else {
                                        newRooms[2].roomTiles[i, j - splitValueHorizontal] = room.roomTiles[i, j];
                                    }
                                } else {
                                    if(j < splitValueHorizontal) {
                                        newRooms[1].roomTiles[i - splitValueVertical, j] = room.roomTiles[i, j];
                                    } else {
                                        newRooms[3].roomTiles[i - splitValueVertical, j - splitValueHorizontal] = room.roomTiles[i, j];
                                    }
                                }
                            }
                        }
                        roomToAdd.Add(newRooms[0]);
                        roomToAdd.Add(newRooms[1]);
                        roomToAdd.Add(newRooms[2]);
                        roomToAdd.Add(newRooms[3]);

                        room.childRooms = newRooms;

                        hasDivide = true;
                        room.divided = true;
                    }
                }
            }

            roomQuadTree.AddRange(roomToAdd);
            yield return null;
        } while(hasDivide);

        //Generate Room
        StartCoroutine(GenerateRoom());
    }

    IEnumerator GenerateRoom() {
        foreach(Room room in roomQuadTree) {

            //build room on final room
            if(room.isFinal) {
                int wallLeft = Mathf.Clamp((int) (RandomSeed.GetValue() * maxWallSize), minWallSize, maxWallSize);
                int wallRight = Mathf.Clamp((int) (RandomSeed.GetValue() * maxWallSize), minWallSize, maxWallSize);
                int wallTop = Mathf.Clamp((int) (RandomSeed.GetValue() * maxWallSize), minWallSize, maxWallSize);
                int wallBottom = Mathf.Clamp((int) (RandomSeed.GetValue() * maxWallSize), minWallSize, maxWallSize);


                for(int i = 0;i < room.roomTiles.GetLength(0);i++) {
                    for(int j = 0;j < room.roomTiles.GetLength(1);j++) {
                        if(i > wallLeft && i < room.roomTiles.GetLength(0) - wallRight &&
                            j > wallBottom && j < room.roomTiles.GetLength(1) - wallTop) {
                            room.roomTiles[i, j].isSolid = false;
                        }
                    }
                }
            }

            yield return null;
        }

        StartCoroutine(AddCorridors());
    }

    IEnumerator AddCorridors() {
        //test all room until they're all connected
        foreach(Room room in roomQuadTree) {
            //if is a Final room do nothing
            if(room.isFinal) {
                room.isConnected = true;
                continue;
            }

            //if is connected do nothing
            if(room.isConnected) {
                continue;
            }

            bool allChildrenConnected = true;

            for(int i = 0;i < 4;i++) {
                if(!room.childRooms[i].isConnected) {
                    allChildrenConnected = false;
                }
            }

            //if has children and are not connected
            if(!allChildrenConnected) {

                //Connect children together
                Room room0 = room.childRooms[0];
                Room room1 = room.childRooms[1];
                Room room2 = room.childRooms[2];
                Room room3 = room.childRooms[3];

                //room 0 -> room 1
                int length = room0.roomTiles.GetLength(0) / 2 + room1.roomTiles.GetLength(0) / 2;
                int offX = room0.roomTiles.GetLength(0) % 2;

                for(int x = 0;x < length;x++) {
                    if(x < room0.roomTiles.GetLength(0) / 2 + offX) { //room 0
                        room0.roomTiles[x + room0.roomTiles.GetLength(0) / 2, room0.roomTiles.GetLength(1) / 2].isSolid = false;
                        room0.roomTiles[x + room0.roomTiles.GetLength(0) / 2, room0.roomTiles.GetLength(1) / 2 + 1].isSolid = false;
                        room0.roomTiles[x + room0.roomTiles.GetLength(0) / 2, room0.roomTiles.GetLength(1) / 2 - 1].isSolid = false;
                    } else { //room 1
                        room1.roomTiles[x - room0.roomTiles.GetLength(0) / 2 - offX, room0.roomTiles.GetLength(1) / 2].isSolid = false;
                        room1.roomTiles[x - room0.roomTiles.GetLength(0) / 2 - offX, room0.roomTiles.GetLength(1) / 2 + 1].isSolid = false;
                        room1.roomTiles[x - room0.roomTiles.GetLength(0) / 2 - offX, room0.roomTiles.GetLength(1) / 2 - 1].isSolid = false;
                    }
                }

                //room 1 -> room 3
                length = room1.roomTiles.GetLength(1) / 2 + room3.roomTiles.GetLength(1) / 2;
                int offY = room1.roomTiles.GetLength(1) % 2;

                for(int y = 0;y < length;y++) {
                    if(y < room1.roomTiles.GetLength(1) / 2 + offY) { //room 1
                        room1.roomTiles[room1.roomTiles.GetLength(0) / 2, y + room1.roomTiles.GetLength(1) / 2].isSolid = false;
                        room1.roomTiles[room1.roomTiles.GetLength(0) / 2 + 1, y + room1.roomTiles.GetLength(1) / 2].isSolid = false;
                        room1.roomTiles[room1.roomTiles.GetLength(0) / 2 - 1, y + room1.roomTiles.GetLength(1) / 2].isSolid = false;
                    } else { //room 3
                        room3.roomTiles[room1.roomTiles.GetLength(0) / 2, y - room1.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                        room3.roomTiles[room1.roomTiles.GetLength(0) / 2 + 1, y - room1.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                        room3.roomTiles[room1.roomTiles.GetLength(0) / 2 - 1, y - room1.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                    }
                }

                //room 3 -> room 2
                length = room3.roomTiles.GetLength(0) / 2 + room2.roomTiles.GetLength(0) / 2;
                
                offX = room2.roomTiles.GetLength(0) % 2;
                for(int x = 0;x < length;x++) {
                    if(x < room2.roomTiles.GetLength(0) / 2 + offX) { //room 2
                        room2.roomTiles[x + room2.roomTiles.GetLength(0) / 2, room2.roomTiles.GetLength(1) / 2].isSolid = false;
                        room2.roomTiles[x + room2.roomTiles.GetLength(0) / 2, room2.roomTiles.GetLength(1) / 2 + 1].isSolid = false;
                        room2.roomTiles[x + room2.roomTiles.GetLength(0) / 2, room2.roomTiles.GetLength(1) / 2 - 1].isSolid = false;
                    } else { //room 3
                        room3.roomTiles[x - room2.roomTiles.GetLength(0) / 2 - offX, room2.roomTiles.GetLength(1) / 2].isSolid = false;
                        room3.roomTiles[x - room2.roomTiles.GetLength(0) / 2 - offX, room2.roomTiles.GetLength(1) / 2 + 1].isSolid = false;
                        room3.roomTiles[x - room2.roomTiles.GetLength(0) / 2 - offX, room2.roomTiles.GetLength(1) / 2 - 1].isSolid = false;
                    }
                }
                //room 2 -> room 0
                length = room0.roomTiles.GetLength(1) / 2 + room2.roomTiles.GetLength(1) / 2;
                offY = room0.roomTiles.GetLength(1) % 2;

                for(int y = 0;y < length;y++) {
                    if(y < room0.roomTiles.GetLength(1) / 2 + offY) { //room 0
                        room0.roomTiles[room0.roomTiles.GetLength(0) / 2, y + room0.roomTiles.GetLength(1) / 2].isSolid = false;
                        room0.roomTiles[room0.roomTiles.GetLength(0) / 2 + 1, y + room0.roomTiles.GetLength(1) / 2].isSolid = false;
                        room0.roomTiles[room0.roomTiles.GetLength(0) / 2 - 1, y + room0.roomTiles.GetLength(1) / 2].isSolid = false;
                    } else { //room 2
                        room2.roomTiles[room0.roomTiles.GetLength(0) / 2, y - room0.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                        room2.roomTiles[room0.roomTiles.GetLength(0) / 2 + 1, y - room0.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                        room2.roomTiles[room0.roomTiles.GetLength(0) / 2 - 1, y - room0.roomTiles.GetLength(1) / 2 - offY].isSolid = false;
                    }
                }

                room.isConnected = true;

            }

            yield return null;
        }

        StartCoroutine(AddExteriorWall());
    }

    IEnumerator AddExteriorWall() {
        Room mainRoom = roomQuadTree[0];
        
        //new mapTile with added wall
        MapTile[,] newTiles = new MapTile[mainRoom.roomTiles.GetLength(0) + 2 * exteriorWallSize, mainRoom.roomTiles.GetLength(1) + 2 * exteriorWallSize];

        //fill mapTile with previous room
        for(int i = 0;i < mainRoom.roomTiles.GetLength(0);i++) {
            for(int j = 0;j < mainRoom.roomTiles.GetLength(1);j++) {
                newTiles[i + exteriorWallSize, j + exteriorWallSize] = mainRoom.roomTiles[i, j];
            }
        }

        yield return null;

        //if empty create new tile
        for (int i = 0; i < newTiles.GetLength(0); i++) {
            for (int j = 0; j < newTiles.GetLength(1); j++) {
                if (newTiles[i, j] == null) {
                    newTiles[i,j] = new MapTile(new Vector2Int(i - exteriorWallSize, j - exteriorWallSize), true);
                }
            }
        }

        mainRoom.roomTiles = newTiles;

        yield return null;

        StartCoroutine(AssociateTiles());
    }

    IEnumerator AssociateTiles() {
        foreach (MapTile roomTile in roomQuadTree[0].roomTiles) {
            if (roomTile.isSolid) {
                roomTile.solidTile = solidTile;
                roomTile.groundTile = null;
            }
            else {
                roomTile.solidTile = null;
                roomTile.groundTile = groundTile;
            }
        }

        yield return null;

        StartCoroutine(AddObjects());
    }

    IEnumerator AddObjects() {

        //Find biggest room

        int maxSize = 0;
        Room mainRoom = null;

        foreach(Room room in roomQuadTree) {
            if(room.isFinal && room.roomTiles.GetLength(0) * room.roomTiles.GetLength(1) > maxSize) {
                maxSize = room.roomTiles.GetLength(0) * room.roomTiles.GetLength(1);
                mainRoom = room;
            }
        }

        //Add motherChip tiles
        {
            int x = mainRoom.roomTiles.GetLength(0) / 2 - mainRoom.roomTiles.GetLength(0) % 2;
            int y = mainRoom.roomTiles.GetLength(1) / 2 - mainRoom.roomTiles.GetLength(1) % 2;
            mainRoom.roomTiles[x, y].groundTile = motherChipTile;
            mainRoom.roomTiles[x + 1, y].groundTile = motherChipTile;
            mainRoom.roomTiles[x, y + 1].groundTile = motherChipTile;
            mainRoom.roomTiles[x + 1, y + 1].groundTile = motherChipTile;
        }

        for (int i = 0; i < roomQuadTree[0].roomTiles.GetLength(0); i++) {
            for (int j = 0; j < roomQuadTree[0].roomTiles.GetLength(1); j++) {
                if (roomQuadTree[0].roomTiles[i, j].groundTile == motherChipTile) {
                    if (isServer) {
                        GameObject instance = Instantiate(mainCore);
                        instance.transform.position = new Vector2(i - 1f - maxWallSize, j - 1f - maxWallSize);

                        NetworkServer.Spawn(instance);
                    }

                    j = roomQuadTree[0].roomTiles.GetLength(1);
                    i = roomQuadTree[0].roomTiles.GetLength(0);
                }
            }
        }

        yield return null;
        //Add 4 Player Spawner in main room
        int playerSpawnerNb = 0;

        while (playerSpawnerNb < nbPlayerSpawer) {
            //Get RandomTile in main Room

            Vector2Int posTile = new Vector2Int(
                (int) (RandomSeed.GetValue() * mainRoom.roomTiles.GetLength(0)),
                (int) (RandomSeed.GetValue() * mainRoom.roomTiles.GetLength(1))
                );

            BoundsInt bounds = new BoundsInt(-1, -1, 0, 2, 2, 1);

            bool neighborsAllFree = true;
            foreach (Vector3Int b in bounds.allPositionsWithin) {
                if (posTile.x + b.x > 0 && posTile.x + b.x < mainRoom.roomTiles.GetLength(0) - 1 && 
                    posTile.y + b.y > 0 && posTile.y + b.y < mainRoom.roomTiles.GetLength(1) - 1) {
                    if (mainRoom.roomTiles[posTile.x + b.x, posTile.y + b.y].groundTile != groundTile || 
                        mainRoom.roomTiles[posTile.x + b.x, posTile.y + b.y].groundTile == motherChipTile ||
                        mainRoom.roomTiles[posTile.x + b.x, posTile.y + b.y].groundTile == playerSpawnerTile) {
                        neighborsAllFree = false;
                        continue;
                    }
                } else {
                    neighborsAllFree = false;
                    continue;
                }
            }

            if (neighborsAllFree) {
                foreach(Vector3Int b in bounds.allPositionsWithin) {
                    mainRoom.roomTiles[posTile.x + b.x, posTile.y + b.y].groundTile = playerSpawnerTile;
                }

                playerSpawnerNb++;
            }

            yield return null;
        }

        //Add one Spawner per Room
        foreach (Room room in roomQuadTree) {
            if (room.isFinal && room != mainRoom) {
                int x = room.roomTiles.GetLength(0) / 2 - room.roomTiles.GetLength(0) % 2;
                int y = room.roomTiles.GetLength(1) / 2 - room.roomTiles.GetLength(1) % 2;

                room.roomTiles[x, y].groundTile = enemySpawnerTile;
                room.roomTiles[x + 1, y].groundTile = enemySpawnerTile;
                room.roomTiles[x, y + 1].groundTile = enemySpawnerTile;
                room.roomTiles[x + 1, y + 1].groundTile = enemySpawnerTile;
            }
        }

        yield return null;

        for(int i = 0;i < roomQuadTree[0].roomTiles.GetLength(0);i++) {
            for(int j = 0;j < roomQuadTree[0].roomTiles.GetLength(1);j++) {
                if(roomQuadTree[0].roomTiles[i, j].groundTile == enemySpawnerTile) {
                    Vector2 pos = new Vector2(i - 1f - maxWallSize, j - 1f - maxWallSize);

                    if (!Physics2D.OverlapBox(pos, Vector2.one, 0, 1 << LayerMask.NameToLayer("EnemySpawner"))) {
                        GameObject instance = Instantiate(enemySpawner);
                        instance.transform.position = pos;
                    }
                }

                if(roomQuadTree[0].roomTiles[i, j].groundTile == playerSpawnerTile) {
                    Vector2 pos = new Vector2(i - 1f - maxWallSize, j - 1f - maxWallSize);

                    if(!Physics2D.OverlapBox(pos, Vector2.one, 0, 1 << LayerMask.NameToLayer("PlayerSpawner"))) {
                        GameObject instance = Instantiate(playerSpawner);
                        instance.transform.position = pos;
                    }
                }
            }
        }

        StartCoroutine(AddNetworkedCircuitEffect());

        isGenerating = false;
    }

    IEnumerator AddNetworkedCircuitEffect() {
        FindObjectOfType<NavigationAI>().GenerateNavigationGraphCross(roomQuadTree[0].roomTiles, new Vector2Int(exteriorWallSize/2, exteriorWallSize / 2));

        yield return null;

        mapController.SetTiles(roomQuadTree[0].roomTiles);

        isGenerating = false;
    }

    void OnDrawGizmos() {
        return;
        if(roomQuadTree == null) {
            return;
        }

        int i = 0;

        if(showBSP && !showWall) {
            foreach(Room room in roomQuadTree) {
                Gizmos.color = colors[i];
                i++;

                if(room.isFinal) {

                    foreach(MapTile roomRoomTile in room.roomTiles) {
                        Vector3 topLeft = new Vector2(0, 1);
                        Vector3 topRight = new Vector2(1, 1);
                        Vector3 bottomLeft = new Vector2(0, 0);
                        Vector3 bottomRight = new Vector2(1, 0);

                        Vector3 pos = new Vector3(roomRoomTile.position.x, roomRoomTile.position.y, 0);

                        Gizmos.DrawLine(pos + topLeft, pos + topRight);
                        Gizmos.DrawLine(pos + topRight, pos + bottomRight);
                        Gizmos.DrawLine(pos + bottomRight, pos + bottomLeft);
                        Gizmos.DrawLine(pos + bottomLeft, pos + topLeft);
                    }
                }
            }
        }

        if(showWall) {
            foreach(Room room in roomQuadTree) {
                Gizmos.color = colors[i];
                i++;

                if(room.isFinal) {
                    foreach(MapTile roomRoomTile in room.roomTiles) {
                        Vector3 topLeft = new Vector2(0, 1);
                        Vector3 topRight = new Vector2(1, 1);
                        Vector3 bottomLeft = new Vector2(0, 0);
                        Vector3 bottomRight = new Vector2(1, 0);

                        Vector3 pos = new Vector3(roomRoomTile.position.x, roomRoomTile.position.y, 0);

                        Gizmos.DrawLine(pos + topLeft, pos + topRight);
                        Gizmos.DrawLine(pos + topRight, pos + bottomRight);
                        Gizmos.DrawLine(pos + bottomRight, pos + bottomLeft);
                        Gizmos.DrawLine(pos + bottomLeft, pos + topLeft);

                        if(roomRoomTile.isSolid) {
                            Gizmos.DrawLine(pos + topLeft, pos + bottomRight);
                            Gizmos.DrawLine(pos + bottomLeft, pos + topRight);
                        }
                    }
                }
            }
        }

        if (showExteriorWall) {
            foreach(MapTile roomRoomTile in roomQuadTree[0].roomTiles) {
                Vector3 topLeft = new Vector2(0, 1);
                Vector3 topRight = new Vector2(1, 1);
                Vector3 bottomLeft = new Vector2(0, 0);
                Vector3 bottomRight = new Vector2(1, 0);

                Vector3 pos = new Vector3(roomRoomTile.position.x, roomRoomTile.position.y, 0);

                Gizmos.DrawLine(pos + topLeft, pos + topRight);
                Gizmos.DrawLine(pos + topRight, pos + bottomRight);
                Gizmos.DrawLine(pos + bottomRight, pos + bottomLeft);
                Gizmos.DrawLine(pos + bottomLeft, pos + topLeft);

                if(roomRoomTile.isSolid) {
                    Gizmos.DrawLine(pos + topLeft, pos + bottomRight);
                    Gizmos.DrawLine(pos + bottomLeft, pos + topRight);
                }
            }
        }
    }

    public void StartGeneration() {
        isGenerating = true;

        StartCoroutine(BinarySpacePartitioning());
    }
}
