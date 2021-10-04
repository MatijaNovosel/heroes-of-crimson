using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;
using System;

public class InputDetector : MonoBehaviour
{
  public GameObject Player;
  public GameObject Projectile;
  private float lastShown;
  private readonly float delay = 0.2f; // 200 ms

  void Start()
  {
    //
  }

  bool CanFire()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - lastShown > delay;
  }

  void Fire()
  {
    Vector3 shootDirection = (Utils.GetMousePosition() - Player.transform.position).normalized;
    GameObject proj = Instantiate(Projectile, new Vector3(Player.transform.position.x, Player.transform.position.y, 0), Quaternion.identity);
    proj.GetComponent<Projectile>().Setup(shootDirection);
    lastShown = Time.time;
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
