using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseNPCBehaviour : MonoBehaviour
{
  // Public stats
  public float hp = 100;
  public float maxHp = 100;
  public bool invincible = false;
  public List<Constants.StatusEffects>  statusEffects;

  // Immunity
  private const float immuneTime = 1.0f;
  protected float lastImmune;

  // Other
  public AudioClip deathSound;
  public AudioClip hitSound;

  private void Die()
  {
    if (deathSound)
    {
      AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    }

    Destroy(gameObject);
  }

  private void DisplayStatusEffects()
  {
    if (statusEffects.Count == 0) return;
    
  }

  private void ReceiveDamage(DamageModel payload)
  {
    if (hitSound)
    {
      AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.5f);
    }
    
    var angle = Random.Range(-40f, 40f);
    var radians = angle * Mathf.Deg2Rad;
    var randomDirection = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);

    var randomRotation = Random.Range(-15f, 15f);
    var randomScale = Random.Range(0.9f, 1.2f);

    var textObj = GameManager.instance.ShowText(
      $"-{payload.Value}",
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
    
    if (!(Time.time - lastImmune > immuneTime) || invincible) return;

    hp -= payload.Value;

    if (hp <= 0)
    {
      hp = 0;
      Die();
    }
  }
}