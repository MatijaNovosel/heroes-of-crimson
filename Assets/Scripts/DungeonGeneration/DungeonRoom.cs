using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
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

    public void SetConnections(HashSet<Constants.Direction> connections)
    {
        SetSide(
            upWall,
            upPassage,
            connections.Contains(Constants.Direction.Up)
        );

        SetSide(
            downWall,
            downPassage,
            connections.Contains(Constants.Direction.Down)
        );

        SetSide(
            leftWall,
            leftPassage,
            connections.Contains(Constants.Direction.Left)
        );

        SetSide(
            rightWall,
            rightPassage,
            connections.Contains(Constants.Direction.Right)
        );
    }


    private void SetSide(GameObject wall, GameObject passage, bool open)
    {
        if (wall != null) wall.SetActive(!open);
        if (passage != null) passage.SetActive(open);
    }
}