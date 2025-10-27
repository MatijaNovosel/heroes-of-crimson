using System.Collections;
using UnityEngine;

public class Meteor : MonoBehaviour
{
  public AnimationClip animationClip;
  public AudioClip audioClip;
  
  public void Setup(Vector3 position)
  {
    var timeToLive = animationClip.length;
    AudioSource.PlayClipAtPoint(audioClip, transform.position, 1.5f);
    GameManager.instance.SpawnProjectiles(position, timeToLive);
    Destroy(gameObject, timeToLive);
  }
}
