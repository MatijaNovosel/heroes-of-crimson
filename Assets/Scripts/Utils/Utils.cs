
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace HeroesOfCrimson.Utils
{
  public static class Utils
  {
    public static Vector3 GetMousePosition()
    {
      Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
      Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
      return new Vector3(worldPosition.x, worldPosition.y, 0);
    }

    public static float GetAngleFromShootDirection(Vector3 direction)
    {
      return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public static int RandInt(int min, int max)
    {
      var seed = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
      return new System.Random(seed).Next(min, max);
    }
  }
}