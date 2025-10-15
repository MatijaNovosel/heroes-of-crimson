using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
  
  List<Constants.CollisionGroups> willDamage = new();
  List<Constants.CollisionGroups> willPenetrate = new();

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

    if (payload.WillDamage.Count != 0)
    {
      this.willDamage = payload.WillDamage;
    }
    
    if (payload.WillPenetrate.Count != 0)
    {
      this.willPenetrate = payload.WillPenetrate;
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
    
    var collidableComponent = collider.gameObject.GetComponent<Collidable>();

    if (!collidableComponent) return;
    
    if (collidableComponent.collisionGroups.Any(x => willDamage.Contains(x)))
    {
      collider.SendMessage("ReceiveDamage", new DamageModel(damage));
    }
    
    if (collidableComponent.collisionGroups.Any(x => willPenetrate.Contains(x)))
    {
      return;
    }
    
    Destroy(gameObject);
  }
}
