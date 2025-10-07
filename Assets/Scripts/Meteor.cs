using System.Collections;
using UnityEngine;

public class Meteor : MonoBehaviour
{
  public GameObject Projectile;
  public AnimationClip animationClip;

  void Start()
  {
    //
  }

  void Update()
  {
    //
  }

  public void Setup(Vector3 position)
  {
    var timeToLive = animationClip.length;
    Destroy(gameObject, timeToLive);
  }
}
