using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private Transform center;

    [Header("Footprint")]
    [SerializeField] private Vector2 footprintSize = new(12f, 12f);
    [SerializeField] private Vector2 footprintOffset;

    [Header("Spawn Points")]
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Door Sockets")]
    [SerializeField] private Transform upSocket;
    [SerializeField] private Transform downSocket;
    [SerializeField] private Transform leftSocket;
    [SerializeField] private Transform rightSocket;

    [Header("Up")]
    [SerializeField] private GameObject upWall;
    [SerializeField] private GameObject upPassage;

    [Header("Down")]
    [SerializeField] private GameObject downWall;
    [SerializeField] private GameObject downPassage;

    [Header("Left")]
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject leftPassage;

    [Header("Right")]
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject rightPassage;

    public Vector3 CenterPosition => center ? center.position : transform.position;
    public Vector3 EnemySpawnPosition => enemySpawnPoint ? enemySpawnPoint.position : CenterPosition;
    public Vector3 BossSpawnPosition => bossSpawnPoint ? bossSpawnPoint.position : CenterPosition;

    public Transform GetSocket(Constants.Direction direction)
    {
        return direction switch
        {
            Constants.Direction.Up => upSocket,
            Constants.Direction.Down => downSocket,
            Constants.Direction.Left => leftSocket,
            Constants.Direction.Right => rightSocket,
            _ => null
        };
    }

    public bool HasSocket(Constants.Direction direction) => GetSocket(direction);

    public Vector3 GetSocketLocalPosition(Constants.Direction direction)
    {
        Transform socket = GetSocket(direction);
        return socket ? transform.InverseTransformPoint(socket.position) : Vector3.zero;
    }

    public Vector3 GetCenterLocalPosition() => center ? transform.InverseTransformPoint(center.position) : Vector3.zero;

    public Rect GetFootprintRect(Vector3 rootPosition, float margin = 0f)
    {
        Vector3 scale = transform.localScale;
        Vector2 size = new(footprintSize.x * Mathf.Abs(scale.x), footprintSize.y * Mathf.Abs(scale.y));
        Vector2 offset = new(footprintOffset.x * scale.x, footprintOffset.y * scale.y);
        size += Vector2.one * margin * 2f;
        return new Rect(rootPosition.x + offset.x - size.x * .5f, rootPosition.y + offset.y - size.y * .5f, size.x, size.y);
    }

    public void SetConnections(HashSet<Constants.Direction> connections)
    {
        SetSide(upWall, upPassage, connections.Contains(Constants.Direction.Up));
        SetSide(downWall, downPassage, connections.Contains(Constants.Direction.Down));
        SetSide(leftWall, leftPassage, connections.Contains(Constants.Direction.Left));
        SetSide(rightWall, rightPassage, connections.Contains(Constants.Direction.Right));
    }

    private void SetSide(GameObject wall, GameObject passage, bool open)
    {
        if (wall) wall.SetActive(!open);
        if (passage) passage.SetActive(open);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(footprintOffset.x, footprintOffset.y), new Vector3(footprintSize.x, footprintSize.y));
    }
}