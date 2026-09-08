using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class RoomNode
{
    public Vector2Int GridPos;
    public readonly HashSet<Constants.Direction> Connections = new();
    public bool IsStart;
    public bool IsBoss;
    public bool IsTreasure;
    public GameObject Instance;
    public DungeonRoom RuntimeRoom;

    public RoomNode(Vector2Int gridPos)
    {
        GridPos = gridPos;
    }

    public int ConnectionCount => Connections.Count;
}