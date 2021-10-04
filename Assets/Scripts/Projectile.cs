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

    */
    transform.eulerAngles = new Vector3(0, 0, angle - 45);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    Target target = collider.GetComponent<Target>();
    if (target != null)
    {
      GameManager.instance.ShowText("Jebem ti mater.", 12, Color.yellow, new Vector3(transform.position.x, transform.position.y + 0.3f, 0), Vector3.up * 50, 2.0f);
      Destroy(gameObject);
    }
  }
}
