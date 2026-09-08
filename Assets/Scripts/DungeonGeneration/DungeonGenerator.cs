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
    [SerializeField] private GameObject[] normalPrefabs;

    [Header("Enemies")]
    [SerializeField] private GameObject enemyPrefab;
    [Range(0f, 100f)]
    [SerializeField] private float enemySpawnChance = 35f;

    [Header("Boss")]
    [SerializeField] private GameObject bossEntityPrefab;
    [Min(0)]
    [SerializeField] private int bossClearance = 1;

    [Header("Room Placement")]
    [Min(0f)]
    [SerializeField] private float roomClearance = 2f;

    [Header("Main Path")]
    [Min(3)]
    [SerializeField] private int minMainPathLength = 12;
    [Min(4)]
    [SerializeField] private int maxMainPathLength = 20;
    [Range(0f, 1f)]
    [SerializeField] private float mainPathTurnChance = .65f;
    [Min(1)]
    [SerializeField] private int maxStraightRooms = 3;

    [Header("Branches")]
    [Min(0)]
    [SerializeField] private int branchAttempts = 35;
    [Range(0f, 1f)]
    [SerializeField] private float branchChance = .75f;
    [Min(1)]
    [SerializeField] private int minBranchLength = 1;
    [Min(1)]
    [SerializeField] private int maxBranchLength = 5;
    [Range(0f, 1f)]
    [SerializeField] private float branchTurnChance = .6f;
    [Min(0)]
    [SerializeField] private int maxBranchRooms = 45;

    [Header("Generation")]
    [Min(1)]
    [SerializeField] private int generationAttempts = 50;
    [SerializeField] private bool generateOnAwake = true;
    [SerializeField] private bool useFixedSeed;
    [SerializeField] private int seed = 12345;

    private readonly Dictionary<Vector2Int, RoomNode> _grid = new();
    private readonly List<RoomNode> _mainPath = new();
    private readonly List<GameObject> _spawnedRooms = new();
    private readonly Dictionary<RoomNode, RoomPlan> _plans = new();

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

    private class RoomPlan
    {
        public GameObject Prefab;
        public DungeonRoom Data;
        public Vector3 Position;
        public Rect Footprint;
        public Rect Clearance;

        public RoomPlan(GameObject prefab, DungeonRoom data, Vector3 position, Rect footprint, Rect clearance)
        {
            Prefab = prefab;
            Data = data;
            Position = position;
            Footprint = footprint;
            Clearance = clearance;
        }
    }

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
            if (!PlanDungeon()) continue;
            success = true;
            break;
        }

        if (!success)
        {
            Debug.LogError($"Dungeon generation failed after {generationAttempts} attempts.");
            return;
        }

        SpawnDungeon();
        SpawnEntities();
        Debug.Log($"Dungeon generated. Rooms: {_grid.Count} | Main Path: {_mainPath.Count} | Branch Rooms: {_branchRoomCount}");
    }

    private bool GenerateMainPath()
    {
        _startRoom = AddRoom(Vector2Int.zero);
        _startRoom.IsStart = true;
        _mainPath.Add(_startRoom);

        RoomNode current = _startRoom;
        Constants.Direction previous = Constants.Direction.Up;

        RoomNode first = AddRoom(Vector2Int.up);
        Connect(_startRoom, first, Constants.Direction.Up);
        _mainPath.Add(first);
        current = first;

        int length = Random.Range(minMainPathLength, maxMainPathLength + 1);
        int straight = 0;

        for (int i = 1; i < length; i++)
        {
            List<Constants.Direction> directions = GetFreeDirections(current, previous);
            if (directions.Count == 0) return false;

            Constants.Direction direction = ChooseDirection(directions, previous, straight, mainPathTurnChance);
            RoomNode next = AddRoom(current.GridPos + DirectionOffset(direction));

            Connect(current, next, direction);
            _mainPath.Add(next);

            straight = direction == previous ? straight + 1 : 0;
            previous = direction;
            current = next;
        }

        foreach (Constants.Direction direction in GetFreeDirections(current).OrderBy(_ => Random.value))
        {
            Vector2Int position = current.GridPos + DirectionOffset(direction);
            if (!HasBossClearance(position, current.GridPos)) continue;

            _bossRoom = AddRoom(position);
            _bossRoom.IsBoss = true;
            Connect(current, _bossRoom, direction);
            return true;
        }

        return false;
    }

    private bool HasBossClearance(Vector2Int bossPosition, Vector2Int entrancePosition)
    {
        for (int x = -bossClearance; x <= bossClearance; x++)
        {
            for (int y = -bossClearance; y <= bossClearance; y++)
            {
                Vector2Int position = bossPosition + new Vector2Int(x, y);
                if (position == entrancePosition) continue;
                if (_grid.ContainsKey(position)) return false;
            }
        }

        return true;
    }

    private void GenerateBranches()
    {
        for (int attempt = 0; attempt < branchAttempts; attempt++)
        {
            if (_branchRoomCount >= maxBranchRooms) return;
            if (Random.value > branchChance) continue;

            List<RoomNode> candidates = _grid.Values.Where(room => room != _startRoom && room != _bossRoom && room.ConnectionCount < 4 && GetFreeDirections(room).Count > 0).ToList();
            if (candidates.Count == 0) return;

            RoomNode parent = RandomItem(candidates);
            List<Constants.Direction> directions = GetFreeDirections(parent);
            if (directions.Count > 0) GrowBranch(parent, RandomItem(directions));
        }
    }

    private void GrowBranch(RoomNode parent, Constants.Direction initialDirection)
    {
        RoomNode current = parent;
        Constants.Direction previous = initialDirection;
        int length = Random.Range(minBranchLength, maxBranchLength + 1);
        int straight = 0;

        for (int i = 0; i < length; i++)
        {
            if (_branchRoomCount >= maxBranchRooms) return;

            Vector2Int position = current.GridPos + DirectionOffset(previous);
            if (_grid.ContainsKey(position) || current.ConnectionCount >= 4 || IsInsideBossClearance(position)) return;

            RoomNode next = AddRoom(position);
            Connect(current, next, previous);
            _branchRoomCount++;

            if (i == length - 1) return;

            List<Constants.Direction> directions = GetFreeDirections(next, previous);
            if (directions.Count == 0) return;

            Constants.Direction direction = ChooseDirection(directions, previous, straight, branchTurnChance);
            straight = direction == previous ? straight + 1 : 0;
            previous = direction;
            current = next;
        }
    }

    private bool IsInsideBossClearance(Vector2Int position)
    {
        if (_bossRoom == null) return false;
        return Mathf.Abs(position.x - _bossRoom.GridPos.x) <= bossClearance && Mathf.Abs(position.y - _bossRoom.GridPos.y) <= bossClearance;
    }

    private bool PlaceTreasure()
    {
        List<RoomNode> deadEnds = _grid.Values.Where(room => room != _startRoom && room != _bossRoom && !_mainPath.Contains(room) && room.ConnectionCount == 1).OrderByDescending(room => GridDistance(_startRoom.GridPos, room.GridPos)).ToList();

        if (deadEnds.Count > 0)
        {
            _treasureRoom = deadEnds[Random.Range(0, Mathf.Min(5, deadEnds.Count))];
            _treasureRoom.IsTreasure = true;
            return true;
        }

        foreach (RoomNode parent in _grid.Values.Where(room => room != _startRoom && room != _bossRoom && room.ConnectionCount < 4 && GetFreeDirections(room).Count > 0).OrderBy(_ => Random.value))
        {
            List<Constants.Direction> directions = GetFreeDirections(parent);
            if (directions.Count == 0) continue;

            Constants.Direction direction = RandomItem(directions);
            Vector2Int position = parent.GridPos + DirectionOffset(direction);

            if (IsInsideBossClearance(position)) continue;

            _treasureRoom = AddRoom(position);
            _treasureRoom.IsTreasure = true;
            Connect(parent, _treasureRoom, direction);
            return true;
        }

        return false;
    }

    private bool PlanDungeon()
    {
        _plans.Clear();

        DungeonRoom data = GetRoomData(startPrefab);
        if (!data) return false;

        Vector3 position = transform.position - ScalePosition(data.GetCenterLocalPosition(), startPrefab.transform.localScale);
        Rect footprint = data.GetFootprintRect(position);
        Rect clearance = data.GetFootprintRect(position, roomClearance * .5f);

        _plans[_startRoom] = new RoomPlan(startPrefab, data, position, footprint, clearance);

        Queue<RoomNode> queue = new();
        HashSet<RoomNode> visited = new();

        queue.Enqueue(_startRoom);
        visited.Add(_startRoom);

        while (queue.Count > 0)
        {
            RoomNode parent = queue.Dequeue();

            foreach (Constants.Direction direction in parent.Connections)
            {
                Vector2Int gridPosition = parent.GridPos + DirectionOffset(direction);
                if (!_grid.TryGetValue(gridPosition, out RoomNode child) || visited.Contains(child)) continue;
                if (!TryPlanRoom(parent, child, direction)) return false;

                visited.Add(child);
                queue.Enqueue(child);
            }
        }

        return visited.Count == _grid.Count;
    }

    private bool TryPlanRoom(RoomNode parent, RoomNode child, Constants.Direction direction)
    {
        RoomPlan parentPlan = _plans[parent];
        if (!parentPlan.Data.HasSocket(direction)) return false;

        Vector3 parentSocket = parentPlan.Position + ScalePosition(parentPlan.Data.GetSocketLocalPosition(direction), parentPlan.Prefab.transform.localScale);

        foreach (GameObject prefab in GetCandidatePrefabs(child))
        {
            if (!prefab) continue;

            DungeonRoom data = GetRoomData(prefab);
            if (!data || !data.HasSocket(Opposite(direction))) continue;

            Vector3 position = parentSocket - ScalePosition(data.GetSocketLocalPosition(Opposite(direction)), prefab.transform.localScale);
            Rect footprint = data.GetFootprintRect(position);
            Rect clearance = data.GetFootprintRect(position, roomClearance * .5f);

            if (OverlapsPlan(parent, footprint, clearance)) continue;

            _plans[child] = new RoomPlan(prefab, data, position, footprint, clearance);
            return true;
        }

        return false;
    }

    private bool OverlapsPlan(RoomNode parent, Rect footprint, Rect clearance)
    {
        foreach (KeyValuePair<RoomNode, RoomPlan> pair in _plans)
        {
            if (footprint.Overlaps(pair.Value.Footprint, true)) return true;
            if (pair.Key != parent && clearance.Overlaps(pair.Value.Clearance, true)) return true;
        }

        return false;
    }

    private List<GameObject> GetCandidatePrefabs(RoomNode room)
    {
        if (room.IsStart) return new() { startPrefab };
        if (room.IsBoss) return new() { bossPrefab };
        if (room.IsTreasure) return new() { treasurePrefab };
        return normalPrefabs == null ? new() : normalPrefabs.Where(prefab => prefab).OrderBy(_ => Random.value).ToList();
    }

    private DungeonRoom GetRoomData(GameObject prefab) => prefab ? prefab.GetComponent<DungeonRoom>() : null;

    private Vector3 ScalePosition(Vector3 position, Vector3 scale) => new(position.x * scale.x, position.y * scale.y, position.z * scale.z);

    private void SpawnDungeon()
    {
        foreach (KeyValuePair<RoomNode, RoomPlan> pair in _plans)
        {
            RoomNode room = pair.Key;
            RoomPlan plan = pair.Value;

            GameObject instance = Instantiate(plan.Prefab, plan.Position, Quaternion.identity, transform);
            instance.name = $"{GetRoomName(room)} [{room.GridPos.x}, {room.GridPos.y}]";

            DungeonRoom dungeonRoom = instance.GetComponent<DungeonRoom>();
            if (!dungeonRoom) continue;

            dungeonRoom.SetConnections(room.Connections);
            room.Instance = instance;
            room.RuntimeRoom = dungeonRoom;
            _spawnedRooms.Add(instance);
        }
    }

    private void SpawnEntities()
    {
        foreach (RoomNode room in _grid.Values)
        {
            if (!room.Instance || !room.RuntimeRoom) continue;

            if (room.IsBoss)
            {
                SpawnBoss(room);
                continue;
            }

            TrySpawnEnemy(room);
        }
    }

    private void TrySpawnEnemy(RoomNode room)
    {
        if (!enemyPrefab || room.IsStart || room.IsBoss || room.IsTreasure || Random.Range(0f, 100f) > enemySpawnChance) return;
        GameObject enemy = Instantiate(enemyPrefab, room.RuntimeRoom.EnemySpawnPosition, Quaternion.identity, room.Instance.transform);
        enemy.name = "Enemy";
    }

    private void SpawnBoss(RoomNode room)
    {
        if (!bossEntityPrefab) return;
        GameObject boss = Instantiate(bossEntityPrefab, room.RuntimeRoom.BossSpawnPosition, Quaternion.identity, room.Instance.transform);
        boss.name = "Boss";
    }

    private string GetRoomName(RoomNode room)
    {
        if (room.IsStart) return "Start";
        if (room.IsBoss) return "Boss";
        if (room.IsTreasure) return "Treasure";
        return "Normal";
    }

    private List<Constants.Direction> GetFreeDirections(RoomNode room, Constants.Direction? previous = null)
    {
        List<Constants.Direction> result = new();

        if (room.ConnectionCount >= 4) return result;

        foreach (Constants.Direction direction in AllDirections)
        {
            if (room.Connections.Contains(direction)) continue;
            if (previous.HasValue && direction == Opposite(previous.Value)) continue;

            Vector2Int position = room.GridPos + DirectionOffset(direction);
            if (_grid.ContainsKey(position)) continue;
            if (_bossRoom != null && IsInsideBossClearance(position)) continue;

            result.Add(direction);
        }

        return result;
    }

    private Constants.Direction ChooseDirection(List<Constants.Direction> options, Constants.Direction previous, int straightCount, float turnChance)
    {
        if (options.Count == 1) return options[0];

        List<Constants.Direction> turns = options.Where(direction => direction != previous).ToList();

        if (straightCount >= maxStraightRooms && turns.Count > 0) return RandomItem(turns);
        if (turns.Count > 0 && Random.value < turnChance) return RandomItem(turns);
        if (options.Contains(previous)) return previous;

        return RandomItem(options);
    }

    private RoomNode AddRoom(Vector2Int position)
    {
        RoomNode room = new(position);
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
        return direction switch
        {
            Constants.Direction.Up => Vector2Int.up,
            Constants.Direction.Down => Vector2Int.down,
            Constants.Direction.Left => Vector2Int.left,
            Constants.Direction.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }

    private Constants.Direction Opposite(Constants.Direction direction)
    {
        return direction switch
        {
            Constants.Direction.Up => Constants.Direction.Down,
            Constants.Direction.Down => Constants.Direction.Up,
            Constants.Direction.Left => Constants.Direction.Right,
            Constants.Direction.Right => Constants.Direction.Left,
            _ => direction
        };
    }

    private int GridDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private T RandomItem<T>(IList<T> list) => list[Random.Range(0, list.Count)];

    private void ResetGeneration()
    {
        _grid.Clear();
        _mainPath.Clear();
        _plans.Clear();
        _startRoom = null;
        _bossRoom = null;
        _treasureRoom = null;
        _branchRoomCount = 0;
    }

    private void ClearDungeon()
    {
        for (int i = _spawnedRooms.Count - 1; i >= 0; i--)
        {
            if (!_spawnedRooms[i]) continue;
            if (Application.isPlaying) Destroy(_spawnedRooms[i]);
            else DestroyImmediate(_spawnedRooms[i]);
        }

        _spawnedRooms.Clear();
    }
}