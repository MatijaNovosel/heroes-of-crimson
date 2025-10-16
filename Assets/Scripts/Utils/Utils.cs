
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace HeroesOfCrimson.Utils
{
  public static class Utils
  {
    public static Vector3 GetMousePosition()
    {
      var screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
      var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
      return new Vector3(worldPosition.x, worldPosition.y, 0);
    }
    
    public static Vector3 GetPlayerPosition()
    {
      var player = GameObject.Find("Player");
      return !player ? Vector3.zero : player.gameObject.transform.position;
    }
    public static bool IsPlayerDead()
    {
      var player = GameObject.Find("Player");
      return player is null;
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

    public static float CalculatePlayerMovementSpeed(float moveSpeed)
    {
      /* 

      0.02 - Minimum
      0.05 - Maximum
      [1, 75] SPD
      [0.02, 0.05] RANGE

    */
      return 0.02f + (moveSpeed * 4.054054054054054e-4f);
    }
  }
}