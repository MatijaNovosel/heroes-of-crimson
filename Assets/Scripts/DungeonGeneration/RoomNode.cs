using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class RoomNode
{
    public Constants.RoomType Type;
    public Vector2Int GridPos;
    public Constants.Direction[] EntryDirections;

    public RoomNode(Constants.RoomType type, Vector2Int pos, Constants.Direction[] entries)
    {
        Type = type;
        GridPos = pos;
        EntryDirections = entries;
    }
}