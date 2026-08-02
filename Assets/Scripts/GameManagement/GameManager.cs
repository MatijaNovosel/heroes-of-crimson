using System.Collections;
using System.Collections.Generic;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public FloatingTextManager floatingTextManager;
  public static GameManager Singleton;
  private GameObject _projectilePrefab;
  
  public Dictionary<Constants.TeleportMarkers, TeleportMarker> teleportMarkersDict = new();
  public List<TeleportMarker> teleportMarkers = new ();

  public static int selectedCharacter = (int)Constants.Character.Knight;

  public void SetCharacter(int character)
  {
    selectedCharacter = character;
  }

  public int GetSelectedCharacter() => selectedCharacter;

  private void Awake()
  {
    Singleton = this;
    _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
    teleportMarkers.ForEach(tm =>
    {
      teleportMarkersDict.Add(tm.id, tm);
    });
  }
  
  private IEnumerator SpawnProjectilesCoroutine(Vector3 position, float time, Constants.ProjectilePattern pattern)
  {
    yield return new WaitForSeconds(time);

    var speed = 12f;
    var projectileCount = 25;
    var angleStep = 360f / projectileCount;
    var initialAngle = 90f;

    for (var i = 0; i < projectileCount; i++)
    {
      var angle = initialAngle + i * angleStep;
      var rad = angle * Mathf.Deg2Rad;
      var direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

      var projectile = Instantiate(
        _projectilePrefab,
          new Vector3(position.x, position.y, 0),
          Quaternion.identity
      );
      
      projectile.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
        direction,
        45,
        speed,
        null,
        50,
        ResourceCacher.Singleton.ProjectileSprites[1],
        new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
        new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
        Color.orange,
        new List<Constants.StatusEffects>(),
        5.0f,
        null,
        null
      ));
    }
  }

  public void SpawnProjectiles(Vector3 position, float delay, Constants.ProjectilePattern pattern)
  {
    StartCoroutine(SpawnProjectilesCoroutine(position, delay, pattern));
  }

  public FloatingText ShowText(
    string msg,
    int fontSize,
    Color color,
    Vector3 position,
    Vector3 motion,
    float duration
  )
  {
    return floatingTextManager.Show(
      msg,
      fontSize,
      color,
      position,
      motion,
      duration
    );
  }
}
