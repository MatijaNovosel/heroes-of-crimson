using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using Models;

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

  private GameObject source;

  public void Setup(ProjectileSetupModel payload)
  {
    var spriteRenderer = GetComponent<SpriteRenderer>();
    
    this.direction = payload.Direction;

    if (payload.Speed != null)
    {
      this.moveSpeed = (float)payload.Speed;
    }

    if (payload.Sprite)
    {
      spriteRenderer.sprite = payload.Sprite;
    }

    if (payload.Rotation != null)
    {
      this.rotation = (float)payload.Rotation;
    }

    if (payload.Source)
    {
      this.source = payload.Source;
    }

    angle = Utils.GetAngleFromShootDirection(payload.Direction);

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
      collider.SendMessage("ReceiveDamage", new DamageModel(source, damage, collider.name));
    }

    if (!destructiveColliders.Contains(collider.tag)) return;
    
    if (source)
    {
      if (source.name == collider.name || (source.name == "KrakenTentacle" && collider.name == "Kraken")) return;
    }
    
    Destroy(gameObject);
  }
}
