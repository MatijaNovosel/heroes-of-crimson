using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
  private Vector3 direction;
  private float angle;
  private float rotation = 45;
  private float moveSpeed = 10f;
  private readonly float timeToLive = 2f; // 1 second?
  private float frequency = 20.0f;
  private float amplitude = 0.5f;

  // Start is called before the first frame update
  public void Setup(Vector3 direction, Sprite sprite = null, float? rotation = null, float? speed = null)
  {
    this.direction = direction;
    var spriteRenderer = GetComponent<SpriteRenderer>();

    if (speed != null)
    {
      this.moveSpeed = (float)speed;
    }

    if (sprite != null)
    {
      spriteRenderer.sprite = sprite;
    }

    if (rotation != null)
    {
      this.rotation = (float)rotation;
    }

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
    transform.eulerAngles = new Vector3(0, 0, this.angle - this.rotation);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    var collidersToAvoid = new List<string>() { "BulletCollision", "Player", "Collision", "Projectile(Clone)" };

    if (collider != null)
    {
      if (!collidersToAvoid.Contains(collider.name))
      {
        float dmg = Utils.RandInt(50, 200);
        GameManager.instance.ShowText(
          $"-{dmg}",
          170,
          Color.red,
          new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
          Vector3.up,
          2.0f
        );
        collider.SendMessage("ReceiveDamage", dmg);
      }
      if (collider.name != "Player" && collider.name != "Collision" && collider.name != "Projectile(Clone)")
      {
        Destroy(gameObject);
      }
    }
  }
}
