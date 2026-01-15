using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private Dictionary<Vector2Int, RoomNode> _grid = new();
    private List<RoomNode> mainPath = new();
    
    public GameObject startPrefab;
    public GameObject bossPrefab;
    public GameObject treasurePrefab;
    public GameObject[] normalPrefabs;
    private float roomSpacing = 16f;

    static readonly Dictionary<Constants.RoomType, Constants.Direction[]> RoomEntries = new()
    {
        { Constants.RoomType.Start, new[] { Constants.Direction.Up } },

        { Constants.RoomType.Normal1, new[] { Constants.Direction.Left, Constants.Direction.Right } },
        { Constants.RoomType.Normal2, new[] { Constants.Direction.Up, Constants.Direction.Down } },
        { Constants.RoomType.Normal3, new[] { Constants.Direction.Right } },
        { Constants.RoomType.Normal4, new[] { Constants.Direction.Left } },
        { Constants.RoomType.Normal5, new[] { Constants.Direction.Down, Constants.Direction.Right } },
        { Constants.RoomType.Normal6, new[] { Constants.Direction.Up, Constants.Direction.Left, Constants.Direction.Right } },
        { Constants.RoomType.Normal7, new[] { Constants.Direction.Down } },

        { Constants.RoomType.Treasure, new[] { Constants.Direction.Down } },
        { Constants.RoomType.Boss, new[] { Constants.Direction.Down } },
    };
    
    Vector2Int ToOffset(Constants.Direction d)
    {
        return d switch
        {
            Constants.Direction.Up => Vector2Int.up,
            Constants.Direction.Down => Vector2Int.down,
            Constants.Direction.Left => Vector2Int.left,
            Constants.Direction.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }
    
    Constants.Direction Opposite(Constants.Direction d)
    {
        return d switch
        {
            Constants.Direction.Up => Constants.Direction.Down,
            Constants.Direction.Down => Constants.Direction.Up,
            Constants.Direction.Left => Constants.Direction.Right,
            Constants.Direction.Right => Constants.Direction.Left,
            _ => d
        };
    }
    
    GameObject GetPrefab(Constants.RoomType type)
    {
        return type switch
        {
            Constants.RoomType.Start => startPrefab,
            Constants.RoomType.Boss => bossPrefab,
            Constants.RoomType.Treasure => treasurePrefab,
            Constants.RoomType.Normal1 => normalPrefabs[0],
            Constants.RoomType.Normal2 => normalPrefabs[1],
            Constants.RoomType.Normal3 => normalPrefabs[2],
            Constants.RoomType.Normal4 => normalPrefabs[3],
            Constants.RoomType.Normal5 => normalPrefabs[4],
            Constants.RoomType.Normal6 => normalPrefabs[5],
            Constants.RoomType.Normal7 => normalPrefabs[6],
            _ => startPrefab
        };
    }
    
    void SpawnDungeon()
    {
        Vector3 origin = transform.position;

        foreach (var pair in _grid)
        {
            RoomNode room = pair.Value;
            GameObject prefab = GetPrefab(room.Type);

            Vector3 worldPos =
                origin +
                new Vector3(
                    room.GridPos.x * roomSpacing,
                    room.GridPos.y * roomSpacing,
                    0
                );

            Instantiate(prefab, worldPos, Quaternion.identity);
        }
    }

    void Awake()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        _grid.Clear();
        mainPath.Clear();
        GenerateMainPath();
        GenerateBranches();
        SpawnDungeon();
    }

    void GenerateMainPath()
    {
        Vector2Int pos = Vector2Int.zero;

        var start = new RoomNode(Constants.RoomType.Start, pos, RoomEntries[Constants.RoomType.Start]);
        _grid[pos] = start;
        mainPath.Add(start);

        Constants.Direction prevDir = Constants.Direction.Up;

        int length = Random.Range(8, 12);

        for (int i = 0; i < length; i++)
        {
            var candidates = RoomEntries
                .Where(r => r.Key.ToString().StartsWith("Normal"))
                .Where(r => r.Value.Contains(Opposite(prevDir)))
                .ToList();

            var picked = candidates[Random.Range(0, candidates.Count)];
            Vector2Int newPos = pos + ToOffset(prevDir);

            if (_grid.ContainsKey(newPos))
            {
                i--;
                prevDir = RandomDirection();
                continue;
            }

            var room = new RoomNode(picked.Key, newPos, picked.Value);
            _grid[newPos] = room;
            mainPath.Add(room);

            pos = newPos;
            prevDir = PickExit(room, Opposite(prevDir));
        }

        Vector2Int bossPos = pos + ToOffset(prevDir);
        var boss = new RoomNode(Constants.RoomType.Boss, bossPos, RoomEntries[Constants.RoomType.Boss]);
        _grid[bossPos] = boss;
    }
    
    Constants.Direction PickExit(RoomNode room, Constants.Direction forbidden)
    {
        var exits = room.EntryDirections.Where(d => d != forbidden).ToList();
        if (exits.Count == 0) return forbidden;
        return exits[Random.Range(0, exits.Count)];
    }

    Constants.Direction RandomDirection()
    {
        return (Constants.Direction)Random.Range(0, 4);
    }

    void GenerateBranches()
    {
        foreach (var room in mainPath)
        {
            foreach (var dir in room.EntryDirections)
            {
                Vector2Int target = room.GridPos + ToOffset(dir);
                if (_grid.ContainsKey(target)) continue;

                if (Random.value < 0.4f)
                {
                    var branchType = Constants.RoomType.Treasure;
                    if (!RoomEntries[branchType].Contains(Opposite(dir))) continue;
                    _grid[target] = new RoomNode(branchType, target, RoomEntries[branchType]);
                }
            }
        }
    }
}
