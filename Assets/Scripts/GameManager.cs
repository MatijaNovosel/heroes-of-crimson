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

  void Start()
  {
    //
  }

  void Update()
  {
    //
  }

  public IEnumerator SpawnProjectilesCoroutine(Vector3 position, float time)
  {
    yield return new WaitForSeconds(time);
    var projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
    Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/genericProjectiles");

    float speed = 12f;
    int projectileCount = 25;
    float angleStep = 360f / projectileCount;
    float initialAngle = 90f;

    for (int i = 0; i < projectileCount; i++)
    {
      float angle = initialAngle + i * angleStep;
      float rad = angle * Mathf.Deg2Rad;
      Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

      GameObject projectile = Instantiate(
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

  public void ShowText(
    string msg,
    int fontSize,
    Color color,
    Vector3 position,
    Vector3 motion,
    float duration
  )
  {
    floatingTextManager.Show(
      msg,
      fontSize,
      color,
      position,
      motion,
      duration
    );
  }
}
