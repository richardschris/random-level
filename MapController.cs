using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{

    public Transform wall;
    public Transform floor;
    public Transform player;
    public Transform monsterGeneric;
    public Transform[,] map;
    public static MapController Instance;

    private struct RoomCenter
    {
        public int x;
        public int y;
    }

    private struct MapData
    {
        public List<Room> roomList;
        public Transform[,] map;
    }

    private class Room
    {
        public int xSize;
        public int ySize;
        public int xCorner;
        public int yCorner;
        public RoomCenter center;

        bool overlapStatus;
        Transform basicFloor;
        public Transform basicWall;

        public Room()
        {
            basicFloor = MapController.Instance.floor;
            basicWall = MapController.Instance.wall;
        }

        public Transform InsertPlayer(Transform player)
        {
            Transform p = (Transform)Instantiate(player, new Vector3(center.x, 0, center.y), Quaternion.identity);
            return p;
        }

        public Transform[,] MakeRoom(Transform[,] emptyMap)
        {
            for (int i = xCorner; i < xCorner + xSize; i++)
            {
                for (int j = yCorner; j < yCorner + ySize; j++)
                {
                    if (CheckOverlap(emptyMap[i, j])) { return emptyMap; }
                }
            }

            for (int i = xCorner; i < xCorner + xSize; i++)
            {
                for (int j = yCorner; j < yCorner + ySize; j++)
                {
                    emptyMap[i, j] = basicFloor;
                }
            }
            center = FindCenter();
            return emptyMap;
        }

        bool CheckOverlap(Transform mapSquare)
        {
            bool overlap = true;

            if (mapSquare == basicWall) { overlap = false; }
            else if (mapSquare == basicFloor) { overlap = true; }

            return overlap;
        }

        public RoomCenter FindCenter()
        {
            RoomCenter roomCenter;

            roomCenter.x = xCorner + (xSize / 2);
            roomCenter.y = yCorner + (ySize / 2);

            return roomCenter;
        }

        public Transform[,] FillRoom(Transform[,] emptyMap)
        {
            for (int i = xCorner; i < xCorner + xSize; i++)
            {
                for (int j = yCorner; j < yCorner + ySize; j++)
                {
                    emptyMap[i, j] = basicWall;
                }
            }
            return emptyMap;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        // Placeholder map

        int xSize = 100;
        int ySize = 100;
        int roomCount = 100;
        int maxSize = 20;

        Transform[,] emptyMap = MakeEmptyMap(xSize, ySize);
        MapData mapData = RoomListGenerator(xSize, ySize, maxSize, roomCount, emptyMap);

        map = mapGenerator(mapData.roomList, mapData.map);
        Instantiate(floor, new Vector3(0, 0, 0), Quaternion.identity);

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                if (map[i, j] == wall) { Instantiate(wall, new Vector3(i, 1, j), Quaternion.identity); }
            }
        }

        Instantiate(player, new Vector3(mapData.roomList[0].center.x, 1, mapData.roomList[0].center.y), Quaternion.identity);
        for (int i = 1; i < mapData.roomList.Count - 1; i++)
        {
            Instantiate(monsterGeneric, new Vector3(mapData.roomList[i].center.x, 1, mapData.roomList[i].center.y), Quaternion.identity);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void TurnOrder(List<GameObject> turns)
    {

    }

    bool CompareMaps(Transform[,] firstMap, Transform[,] secondMap)
    {
        for (int j = 0; j < firstMap.GetLength(0); j++)
        {
            for (int k = 0; k < firstMap.GetLength(1); k++)
            {
                if (firstMap[j, k].tag != secondMap[j, k].tag) { return true; }
            }
        }
        return false;
    }

    MapData RoomListGenerator(int xSize, int ySize, int maxSize, int roomCount, Transform[,] emptyMap)
    {
        List<Room> roomList = new List<Room>();

        for (int i = 0; i < roomCount; i++)
        {
            Room room = new Room();

            room.xSize = (int)Mathf.Floor(Random.Range(2, maxSize));
            room.ySize = (int)Mathf.Floor(Random.Range(2, maxSize));
            room.xCorner = (int)Mathf.Floor(Random.Range(2, xSize - maxSize));
            room.yCorner = (int)Mathf.Floor(Random.Range(2, ySize - maxSize));

            Transform[,] emptyMapCopy = new Transform[emptyMap.GetLength(0), emptyMap.GetLength(1)];
            System.Array.Copy(emptyMap, emptyMapCopy, emptyMap.Length);
            emptyMap = room.MakeRoom(emptyMap);

            if (CompareMaps(emptyMap, emptyMapCopy)) { roomList.Add(room); }
        }

        MapData mapData = new MapData();

        mapData.roomList = roomList;
        mapData.map = emptyMap;

        return mapData;
    }

    Transform[,] MakeEmptyMap(int xSize, int ySize)
    {
        Transform[,] emptyMap = new Transform[xSize, ySize];

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                emptyMap[i, j] = wall;
            }
        }

        return emptyMap;
    }

    Transform[,] mapGenerator(List<Room> roomList, Transform[,] emptyMap)
    {
        for (int k = 0; k < roomList.Count - 1; k++)
        {
            RoomCenter firstRoom = roomList[k].FindCenter();
            RoomCenter secondRoom = roomList[k + 1].FindCenter();
            int xIndex = firstRoom.x;

            if (firstRoom.x > secondRoom.x)
            {
                int xHallLength = firstRoom.x - secondRoom.x;
                for (int i = 0; i < xHallLength; i++)
                {
                    emptyMap[firstRoom.x - i, firstRoom.y] = floor;
                    emptyMap[firstRoom.x - i, firstRoom.y + 1] = floor;

                }

                xIndex = firstRoom.x - xHallLength;
            }

            else if (firstRoom.x < secondRoom.x)
            {
                int xHallLength = secondRoom.x - firstRoom.x;
                for (int i = 0; i < xHallLength; i++)
                {
                    emptyMap[firstRoom.x + i, firstRoom.y] = floor;
                    emptyMap[firstRoom.x + i, firstRoom.y + 1] = floor;

                }
                xIndex = firstRoom.x + xHallLength;
            }

            if (firstRoom.y > secondRoom.y)
            {
                int yHallLength = firstRoom.y - secondRoom.y;
                for (int i = 0; i < yHallLength; i++)
                {
                    emptyMap[xIndex, firstRoom.y - i] = floor;
                    emptyMap[xIndex + 1, firstRoom.y - i] = floor;

                }
            }

            else if (firstRoom.y < secondRoom.y)
            {
                int yHallLength = secondRoom.y - firstRoom.y;
                for (int i = 0; i < yHallLength; i++)
                {
                    emptyMap[xIndex, firstRoom.y + i] = floor;
                    emptyMap[xIndex + 1, firstRoom.y + i] = floor;

                }
            }
        }
        return emptyMap;
    }
}

