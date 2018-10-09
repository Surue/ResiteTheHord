using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator:MonoBehaviour {

    class Room {
        public MapTile[,] roomTiles;

        public Room[] childRooms = new Room[4];

        public bool divided;

        public bool isFinal = false;

        public bool isConnected = false;

        public Vector2Int GetCenter() {
            return new Vector2Int((int)(roomTiles.GetLength(0) / 2f), (int)(roomTiles.GetLength(1) / 2f));
        }
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
    public RuleTile solidTile;
    public RuleTile groundTile;
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

    void Start() {
        roomQuadTree = new List<Room>();
        mapController = GetComponent<MapController>();

        StartCoroutine(BinarySpacePartitioning());

        colors = new List<Color>();

        for(int i = 0;i < 1000;i++) {
            colors.Add(new Color(Random.value, Random.value, Random.value));
        }
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

        mapController.SetTiles(roomQuadTree[0].roomTiles);
    }

    void OnDrawGizmos() {
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
}
