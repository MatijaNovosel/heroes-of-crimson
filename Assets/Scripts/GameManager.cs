using System.Collections;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public FloatingTextManager floatingTextManager;
  public static GameManager Instance;
  private Sprite[] _projectileSprites;
  private GameObject _projectilePrefab;

  private void Awake()
  {
    _projectileSprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
    _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
    Instance = this;
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
        _projectileSprites[1],
        new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
        new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player }
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
