using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Special Rooms")]
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject treasurePrefab;
    
    [Header("Normal Room Designs")]
    [SerializeField]
    private GameObject[] normalPrefabs;
    
    [Header("Room Size")]
    [SerializeField]
    private float roomWidth = 16f;

    [SerializeField]
    private float roomHeight = 15f;
    
    [Header("Enemies")]

    [SerializeField]
    private GameObject enemyPrefab;

    [Range(0f, 100f)]
    [SerializeField]
    private float enemySpawnChance = 35f;

    [SerializeField]
    private Vector3 enemySpawnOffset = Vector3.zero;

    [Header("Main Path")]
    
    [Min(3)]
    [SerializeField]
    private int minMainPathLength = 12;
    
    [Min(4)]
    [SerializeField]
    private int maxMainPathLength = 20;
    
    [Range(0f, 1f)]
    [SerializeField]
    private float mainPathTurnChance = 0.65f;

    [Min(1)]
    [SerializeField]
    private int maxStraightRooms = 3;
    

    [Header("Branches")]

    [Min(0)]
    [SerializeField]
    private int branchAttempts = 35;


    [Range(0f, 1f)]
    [SerializeField]
    private float branchChance = 0.75f;


    [Min(1)]
    [SerializeField]
    private int minBranchLength = 1;


    [Min(1)]
    [SerializeField]
    private int maxBranchLength = 5;


    [Range(0f, 1f)]
    [SerializeField]
    private float branchTurnChance = 0.60f;

    
    [Min(0)]
    [SerializeField]
    private int maxBranchRooms = 45;

    [Header("Generation")]

    [Min(1)]
    [SerializeField]
    private int generationAttempts = 250;

    [SerializeField]
    private bool generateOnAwake = true;
    
    [SerializeField]
    private bool useFixedSeed = false;


    [SerializeField]
    private int seed = 12345;
    private readonly Dictionary<Vector2Int, RoomNode> _grid = new Dictionary<Vector2Int, RoomNode>();
    private readonly List<RoomNode> _mainPath = new List<RoomNode>();
    private readonly List<GameObject> _spawnedRooms = new List<GameObject>();
    
    private RoomNode _startRoom;
    private RoomNode _bossRoom;
    private RoomNode _treasureRoom;

    private int _branchRoomCount;

    private static readonly Constants.Direction[] AllDirections =
    {
        Constants.Direction.Up,
        Constants.Direction.Down,
        Constants.Direction.Left,
        Constants.Direction.Right
    };
    
    private void Awake()
    {
        if (generateOnAwake) GenerateDungeon();
    }


    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        if (useFixedSeed) Random.InitState(seed);
        
        bool success = false;
        
        for (int attempt = 0; attempt < generationAttempts; attempt++)
        {
            ResetGeneration();
            if (!GenerateMainPath()) continue;
            GenerateBranches();
            if (!PlaceTreasure()) continue;
            success = true;
            break;
        }
        
        if (!success)
        {
            Debug.LogError($"Dungeon generation failed after {generationAttempts} attempts.");
            return;
        }
        
        SpawnDungeon();
        
        Debug.Log(
            $"Dungeon generated. Rooms: {_grid.Count} | Main Path: {_mainPath.Count} | Branch Rooms: {_branchRoomCount}"
        );
    }

    private bool GenerateMainPath()
    {
        _startRoom = AddRoom(Vector2Int.zero);
        _startRoom.IsStart = true;
        _mainPath.Add(_startRoom);
        
        RoomNode current = _startRoom;
        Constants.Direction previousDirection = Constants.Direction.Up;
        Vector2Int firstPosition = Vector2Int.up;
        RoomNode firstRoom = AddRoom(firstPosition);
        Connect(_startRoom, firstRoom, Constants.Direction.Up);
        _mainPath.Add(firstRoom);
        current = firstRoom;
        
        int targetLength = Random.Range(minMainPathLength, maxMainPathLength + 1);
        int straightCount = 0;
        

        for (int i = 1; i < targetLength; i++)
        {
            List<Constants.Direction> possibleDirections = GetFreeDirections(current, previousDirection);
            if (possibleDirections.Count == 0) return false;
            
            Constants.Direction direction = ChooseDirection(
                possibleDirections,
                previousDirection,
                straightCount,
                mainPathTurnChance
            );

            Vector2Int position = current.GridPos + DirectionOffset(direction);
            RoomNode next = AddRoom(position);
            Connect(current, next, direction);
            _mainPath.Add(next);

            if (direction == previousDirection) straightCount++;
            else straightCount = 0;
            
            previousDirection = direction;
            current = next;
        }

        List<Constants.Direction> bossDirections = GetFreeDirections(current);
        
        if (bossDirections.Count == 0) return false;
        
        Constants.Direction bossDirection = RandomItem(bossDirections);
        
        Vector2Int bossPosition = current.GridPos + DirectionOffset(bossDirection);
        
        _bossRoom = AddRoom(bossPosition);
        _bossRoom.IsBoss = true;

        Connect(current, _bossRoom, bossDirection);

        return true;
    }

    private void GenerateBranches()
    {
        for (int attempt = 0; attempt < branchAttempts; attempt++)
        {
            if (_branchRoomCount >= maxBranchRooms) return;
            if (Random.value > branchChance) continue;

            List<RoomNode> candidates =
                _grid.Values
                    .Where(room =>
                        room != _startRoom &&
                        room != _bossRoom &&
                        room != _treasureRoom &&
                        room.ConnectionCount < 4 &&
                        GetFreeDirections(room).Count > 0
                    )
                    .ToList();
            
            if (candidates.Count == 0) return;
            RoomNode parent = RandomItem(candidates);
            List<Constants.Direction> directions = GetFreeDirections(parent);

            if (directions.Count == 0) continue;
            
            GrowBranch(parent,RandomItem(directions));
        }
    }

    private void GrowBranch(RoomNode parent, Constants.Direction initialDirection)
    {
        RoomNode current = parent;
        Constants.Direction previousDirection = initialDirection;
        
        int length = Random.Range(minBranchLength, maxBranchLength + 1);
        int straightCount = 0;

        for (int i = 0; i < length; i++)
        {
            if (_branchRoomCount >= maxBranchRooms) return;

            Vector2Int position = current.GridPos + DirectionOffset(previousDirection);

            if (_grid.ContainsKey(position) || current.ConnectionCount >= 4) return;
            
            RoomNode next = AddRoom(position);
            Connect(current, next, previousDirection);
            
            _branchRoomCount++;

            if (i == length - 1) return;

            List<Constants.Direction> directions = GetFreeDirections(next, previousDirection);
            
            if (directions.Count == 0) return;

            Constants.Direction direction = ChooseDirection(
                directions,
                previousDirection,
                straightCount,
                branchTurnChance
            );
            
            if (direction == previousDirection) straightCount++;
            else straightCount = 0;
            
            current = next;
            previousDirection = direction;
        }
    }
    
    private bool PlaceTreasure()
    {
        List<RoomNode> deadEnds =
            _grid.Values
                .Where(room =>
                    room != _startRoom &&
                    room != _bossRoom &&
                    !_mainPath.Contains(room) &&
                    room.ConnectionCount == 1
                )
                .OrderByDescending(room =>
                    GridDistance(
                        _startRoom.GridPos,
                        room.GridPos
                    )
                )
                .ToList();

        if (deadEnds.Count > 0)
        {
            int pool = Mathf.Min(5, deadEnds.Count);
            _treasureRoom = deadEnds[Random.Range(0, pool)];
            _treasureRoom.IsTreasure = true;
            return true;
        }
        
        List<RoomNode> candidates =
            _grid.Values
                .Where(room =>
                    room != _startRoom &&
                    room != _bossRoom &&
                    room.ConnectionCount < 4 &&
                    GetFreeDirections(room).Count > 0
                )
                .OrderBy(_ => Random.value)
                .ToList();

        foreach (RoomNode parent in candidates)
        {
            List<Constants.Direction> directions = GetFreeDirections(parent);
            if (directions.Count == 0) continue;
            Constants.Direction direction = RandomItem(directions);
            Vector2Int position = parent.GridPos + DirectionOffset(direction);
            _treasureRoom = AddRoom(position);
            _treasureRoom.IsTreasure = true;
            Connect(parent, _treasureRoom, direction);
            return true;
        }
        
        return false;
    }

    private void SpawnDungeon()
    {
        foreach (RoomNode room in _grid.Values)
        {
            GameObject prefab = GetPrefabForRoom(room);
        
            if (prefab == null)
            {
                Debug.LogError($"Missing prefab for room at {room.GridPos}");
                continue;
            }
        
            Vector3 position = GridToWorld(room.GridPos);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
            instance.name = GetRoomName(room) + " [" + room.GridPos.x + ", " + room.GridPos.y + "]";
            DungeonRoom dungeonRoom =  instance.GetComponent<DungeonRoom>();

            if (dungeonRoom == null) Debug.LogError(instance.name + " is missing DungeonRoom component.");
            else dungeonRoom.SetConnections(room.Connections);

            TrySpawnEnemy(room, instance);
        
            _spawnedRooms.Add(instance);
        }
    }
    
    private void TrySpawnEnemy(RoomNode room, GameObject roomInstance)
    {
        if (enemyPrefab == null) return;

        if (room.IsStart || room.IsBoss || room.IsTreasure) return;

        float roll = Random.Range(0f, 100f);

        if (roll > enemySpawnChance) return;

        Vector3 spawnPosition = roomInstance.transform.position +enemySpawnOffset;

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity,
            roomInstance.transform
        );

        enemy.name = "Enemy";
    }

    private GameObject GetPrefabForRoom(RoomNode room)
    {
        if (room.IsStart) return startPrefab;
        if (room.IsBoss) return bossPrefab;
        if (room.IsTreasure) return treasurePrefab;
        if (normalPrefabs == null || normalPrefabs.Length == 0) return null;
        return normalPrefabs[Random.Range(0, normalPrefabs.Length)];
    }


    private string GetRoomName(RoomNode room)
    {
        if (room.IsStart) return "Start";
        if (room.IsBoss) return "Boss";
        if (room.IsTreasure) return "Treasure";
        return "Normal";
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return transform.position +
            new Vector3(
                gridPosition.x *
                roomWidth,
                gridPosition.y *
                roomHeight,
                0f
            );
    }
    

    private List<Constants.Direction> GetFreeDirections(
        RoomNode room,
        Constants.Direction? previousDirection = null)
    {
        List<Constants.Direction> result = new List<Constants.Direction>();
        if (room.ConnectionCount >= 4) return result;

        foreach (Constants.Direction direction in AllDirections)
        {
            if (room.Connections.Contains(direction)) continue;
            if (previousDirection.HasValue && direction == Opposite(previousDirection.Value)) continue;
            Vector2Int neighbour = room.GridPos + DirectionOffset(direction);
            if (_grid.ContainsKey(neighbour))continue;
            result.Add(direction);
        }

        return result;
    }

    private Constants.Direction ChooseDirection(
        List<Constants.Direction> options,
        Constants.Direction previousDirection,
        int straightCount,
        float turnProbability
    ) {
        if (options.Count == 1) return options[0];
        
        List<Constants.Direction> turns =
            options
                .Where(direction =>
                    direction != previousDirection
                )
                .ToList();
        
        if (straightCount >= maxStraightRooms && turns.Count > 0) return RandomItem(turns);
        if (turns.Count > 0 && Random.value < turnProbability) return RandomItem(turns);
        if (options.Contains(previousDirection)) return previousDirection;
        
        return RandomItem(options);
    }

    private RoomNode AddRoom(Vector2Int position)
    {
        RoomNode room = new RoomNode(position);
        _grid.Add(position, room);
        return room;
    }
    
    private void Connect(RoomNode from, RoomNode to, Constants.Direction direction)
    {
        from.Connections.Add(direction);
        to.Connections.Add(Opposite(direction));
    }

    private Vector2Int DirectionOffset(Constants.Direction direction)
    {
        switch (direction)
        {
            case Constants.Direction.Up: return Vector2Int.up;
            case Constants.Direction.Down: return Vector2Int.down;
            case Constants.Direction.Left: return Vector2Int.left;
            case Constants.Direction.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private Constants.Direction Opposite(Constants.Direction direction) {
        switch (direction)
        {
            case Constants.Direction.Up: return Constants.Direction.Down;
            case Constants.Direction.Down: return Constants.Direction.Up;
            case Constants.Direction.Left: return Constants.Direction.Right;
            case Constants.Direction.Right: return Constants.Direction.Left;
            default: return direction;
        }
    }
    
    private int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private T RandomItem<T>(IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
    
    private void ResetGeneration()
    {
        _grid.Clear();
        _mainPath.Clear();
        _startRoom = null;
        _bossRoom = null;
        _treasureRoom = null;
        _branchRoomCount = 0;
    }

    private void ClearDungeon()
    {
        for (int i = _spawnedRooms.Count - 1; i >= 0; i--)
        {
            if (_spawnedRooms[i] == null) continue;
            
            if (Application.isPlaying) Destroy(_spawnedRooms[i]);
            else DestroyImmediate(_spawnedRooms[i]);
        }
        _spawnedRooms.Clear();
    }
}