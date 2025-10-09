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
    if (deathSound != null)
    {
      AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    }
    Destroy(gameObject);
  }

  protected virtual void ReceiveDamage(float dmg)
  {
    GameManager.instance.ShowText(
      $"-{dmg}",
      170,
      Color.red,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      Vector3.up,
      2.0f
    );
    
    if (invincible) return;
    
    if (!(Time.time - lastImmune > immuneTime)) return;
    hp -= dmg;

    if (!(hp <= 0)) return;
    hp = 0;
    Die();
  }
}
