using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;
using System;

public class InputDetector : MonoBehaviour
{
  public GameObject Player;
  public GameObject Projectile;
  private DateTime? LastFired = null;
  private readonly int DelayMs = 350;

  void Start()
  {
    //
  }

  bool CanFire()
  {
    bool shouldFire = false;

    if (LastFired == null)
    {
      shouldFire = true;
    }
    else
    {
      TimeSpan diff = DateTime.Now - (DateTime)LastFired;
      int ms = (int)diff.TotalMilliseconds;
      if (ms > DelayMs)
      {
        shouldFire = true;
      }
    }

    return shouldFire;
  }

  void Fire()
  {
    Vector3 shootDirection = (Utils.GetMousePosition() - Player.transform.position).normalized;
    GameObject proj = Instantiate(Projectile, new Vector3(Player.transform.position.x, Player.transform.position.y, 0), Quaternion.identity);
    proj.GetComponent<Projectile>().Setup(shootDirection);
    LastFired = DateTime.Now;
  }

  private void HandleShooting()
  {
    if (Input.GetMouseButton(0))
    {
      if (CanFire())
      {
        Fire();
      }
    }
  }

  void Update()
  {
    HandleShooting();
  }
}
