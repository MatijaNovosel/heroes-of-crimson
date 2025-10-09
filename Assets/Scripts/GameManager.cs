using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
  public FloatingTextManager floatingTextManager;
  public DatabaseManager databaseManager;
  public static GameManager instance;

  private void Awake()
  {
    instance = this;
  }
  
  private IEnumerator SpawnProjectilesCoroutine(Vector3 position, float time)
  {
    yield return new WaitForSeconds(time);
    var projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
    Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/genericProjectiles");

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
          projectilePrefab,
          new Vector3(position.x, position.y, 0),
          Quaternion.identity
      );

      var projComponent = projectile.GetComponent<Projectile>();
      projComponent.Setup(direction, sprites[0], 0, speed);
    }
  }

  public void SpawnProjectiles(Vector3 position, float delay)
  {
    StartCoroutine(SpawnProjectilesCoroutine(position, delay));
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
    var ft = floatingTextManager.Show(
      msg,
      fontSize,
      color,
      position,
      motion,
      duration
    );

    return ft;
  }
}
