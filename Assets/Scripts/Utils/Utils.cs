
using UnityEngine;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Random = System.Random;

namespace HeroesOfCrimson.Utils
{
  public static class Utils
  {
    public static Vector3 GetMousePosition()
    {
      var screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

      if (Camera.main is null) return Vector3.zero;
      
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
    
    public static Color FromHex(string hex)
    {
      if (hex.Length<6)
      {
        throw new System.FormatException("Needs a string with a length of at least 6");
      }

      var r = hex.Substring(0, 2);
      var g = hex.Substring(2, 2);
      var b = hex.Substring(4, 2);
      string alpha;
      if (hex.Length >= 8)
        alpha = hex.Substring(6, 2);
      else
        alpha = "FF";

      return new Color((int.Parse(r, NumberStyles.HexNumber) / 255f),
        (int.Parse(g, NumberStyles.HexNumber) / 255f),
        (int.Parse(b, NumberStyles.HexNumber) / 255f),
        (int.Parse(alpha, NumberStyles.HexNumber) / 255f));
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

    public static float CalculatePlayerMovementSpeed(float SPD)
    {
      var spd = Mathf.Clamp(SPD, 1f, 100f);
      const float minSpeed = 0.05f;
      const float maxSpeed = 0.25f;
      return Mathf.Lerp(minSpeed, maxSpeed, (spd - 1f) / 99f);
    }
    
    public static float GetDistanceToPlayer(Vector3 from)
    {
      var player = GameObject.Find("Player");
      if (!player) return float.MaxValue;
      return ((Vector2)player.transform.position - (Vector2)from).sqrMagnitude;
    }
    
    public static double RandFloat(float minimum, float maximum)
    { 
      Random random = new Random();
      return Math.Floor(random.NextDouble() * (maximum - minimum) + minimum);
    }
    
    public static LineRenderer CreateCircle(
      Transform transform,
      string name,
      float radius,
      Color color,
      int circleSegments = 48
    )
    {
      var go = new GameObject(name);
      go.transform.SetParent(transform);
      go.transform.localPosition = Vector3.zero;

      var lr = go.AddComponent<LineRenderer>();
      lr.useWorldSpace = false;
      lr.loop = true;
      lr.positionCount = circleSegments;
      lr.startWidth = 0.1f;
      lr.endWidth = 0.1f;
      lr.material = new Material(Shader.Find("Sprites/Default"));
      lr.startColor = color;
      lr.endColor = color;
      lr.sortingLayerName = "Collision";

      float angleStep = 2 * Mathf.PI / circleSegments;

      for (int i = 0; i < circleSegments; i++)
      {
        float angle = i * angleStep;
        lr.SetPosition(
          i,
          new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            0
          )
        );
      }

      return lr;
    }
  }
}