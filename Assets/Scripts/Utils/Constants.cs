using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace HeroesOfCrimson.Utils
{
  public static class Constants
  {
    public enum ShootingMouseDirs
    {
      DOWN = 0,
      HORIZONTAL = 2,
      UP = 4
    }

    public enum CollisionGroups
    {
      Player = 1,
      NPC = 2,
      Enemy = 3,
      Collision = 4,
      BulletCollision = 5,
    }
  }
}