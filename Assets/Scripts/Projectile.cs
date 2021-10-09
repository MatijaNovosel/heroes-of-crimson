using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

public class Projectile : MonoBehaviour
{
  private Vector3 direction;
  private float angle;
  private readonly float moveSpeed = 5f;
  private readonly float timeToLive = 1f; // 1 second?
  private float frequency = 20.0f;
  private float amplitude = 0.5f;

  // Start is called before the first frame update
  public void Setup(Vector3 direction)
  {
    this.direction = direction;
    angle = Utils.GetAngleFromShootDirection(direction);
    Destroy(gameObject, timeToLive);
  }

  // Update is called once per frame
  void Update()
  {
    transform.position += direction * moveSpeed * Time.deltaTime;
    /*

      Projectiles must be rotated according to the sprite
      
        DIR - Normal angle
        DIA - Minus 45 degrees
        RAN - Random

      For sine movement:

        Vector3 pos = transform.position;
        pos += direction * moveSpeed * Time.deltaTime;
        transform.position = pos + transform.right * Mathf.Sin(Time.time * frequency) * amplitude;

    */
    transform.eulerAngles = new Vector3(0, 0, angle - 45);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider != null)
    {
      if (collider.name != "BulletCollision" && collider.name != "Player" && collider.name != "Collision")
      {
        float dmg = Utils.RandInt(50, 200);
        GameManager.instance.ShowText(
          $"-{dmg}",
          65,
          Color.red,
          new Vector3(transform.position.x, transform.position.y + 0.4f, 0),
          Vector3.up,
          2.0f
        );
        collider.SendMessage("ReceiveDamage", dmg);
      }
      if (collider.name != "Player" && collider.name != "Collision")
      {
        Destroy(gameObject);
      }
    }
  }
}
