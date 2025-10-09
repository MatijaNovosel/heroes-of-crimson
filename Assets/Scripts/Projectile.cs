using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
  private Vector3 direction;
  private float angle;
  private bool piercingWall;
  private bool piercing;
  private float damage = 50;
  private float rotation = 45;
  private float moveSpeed = 10f;
  private readonly float timeToLive = 2f; // 1 second?
  private float frequency = 20.0f;
  private float amplitude = 0.5f;
  
  List<string> collidersToDamage = new() { "Enemy", "NPC" };
  List<string> destructiveColliders = new() { "BulletCollision", "Enemy", "NPC" };

  public void Setup(Vector3 direction, Sprite sprite = null, float? rotation = null, float? speed = null)
  {
    this.direction = direction;
    var spriteRenderer = GetComponent<SpriteRenderer>();

    if (speed != null)
    {
      this.moveSpeed = (float)speed;
    }

    if (sprite is not null)
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

  void Update()
  {
    transform.position += direction * (moveSpeed * Time.deltaTime);
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
    if (!collider) return;
    
    if (collidersToDamage.Contains(collider.tag))
    {
      collider.SendMessage("ReceiveDamage", damage);
    }
    
    if (destructiveColliders.Contains(collider.tag))
    {
      Destroy(gameObject);
    }
  }
}
