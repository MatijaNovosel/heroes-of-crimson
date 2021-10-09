using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNPCBehaviour : MonoBehaviour
{
  // Public stats
  public float hp = 100;
  public float maxHp = 100;

  // Immunity
  protected float immuneTime = 1.0f;
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
    AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    Destroy(gameObject);
  }

  protected virtual void ReceiveDamage(float dmg)
  {
    if (Time.time - lastImmune > immuneTime)
    {
      hp -= dmg;
      if (hp <= 0)
      {
        hp = 0;
        Die();
      }
    }
  }
}
