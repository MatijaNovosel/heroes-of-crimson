using System.Collections;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class Lightning : MonoBehaviour
{
  public AnimationClip animationClip;
  public AudioClip audioClip;
  
  public void Setup(Vector3 position)
  {
    var timeToLive = animationClip.length;
    AudioSource.PlayClipAtPoint(audioClip, transform.position, 1.5f);
    GameManager.Singleton.SpawnProjectiles(position, timeToLive, Constants.ProjectilePattern.Circular);
    Destroy(gameObject, timeToLive);
  }
}
