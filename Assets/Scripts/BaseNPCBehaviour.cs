using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNPCBehaviour : MonoBehaviour
{
  // Public stats
  public float hp = 100;
  public float maxHp = 100;
  public bool invincible = false;

  // Immunity
  private const float immuneTime = 1.0f;
  protected float lastImmune;

  // Other
  public AudioClip deathSound;

  void Start()
  {

  }

  void Update()
  {

  }

  protected virtual void Die()
  {
    if (deathSound)
    {
      AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    }

    Destroy(gameObject);
  }

  protected virtual void ReceiveDamage(float dmg)
  {
    var angle = Random.Range(-40f, 40f);
    var radians = angle * Mathf.Deg2Rad;
    var randomDirection = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);

    var randomRotation = Random.Range(-15f, 15f);
    var randomScale = Random.Range(0.9f, 1.2f);

    var textObj = GameManager.instance.ShowText(
      $"-{dmg}",
      170,
      Color.red,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      randomDirection,
      2.0f
    );

    if (textObj != null)
    {
      textObj.obj.transform.rotation = Quaternion.Euler(0, 0, randomRotation);
      textObj.obj.transform.localScale *= randomScale;
    }

    if (invincible) return;
    if (!(Time.time - lastImmune > immuneTime)) return;

    hp -= dmg;

    if (hp <= 0)
    {
      hp = 0;
      Die();
    }
  }
}