using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Models;
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

    public static bool IsNegativeStatusEffect(Constants.StatusEffects statusEffect)
    {
      return new List<Constants.StatusEffects>()
      {
        Constants.StatusEffects.ArmorBroken,
        Constants.StatusEffects.Bleeding,
        Constants.StatusEffects.Slowed,
        Constants.StatusEffects.Silenced,
        Constants.StatusEffects.Paralyzed,
        Constants.StatusEffects.Poisoned,
        Constants.StatusEffects.Stunned,
        Constants.StatusEffects.Weak,
      }.Contains(statusEffect);
    }

    public static StatusEffectData GetStatusEffectData(Constants.StatusEffects statusEffect)
{
    var data = new StatusEffectData
    {
        IsNegative = IsNegativeStatusEffect(statusEffect)
    };

    switch (statusEffect)
    {
        case Constants.StatusEffects.Speedy:
            data.Name = "Speedy";
            data.Description = "Movement speed is increased.";
            break;

        case Constants.StatusEffects.Slowed:
            data.Name = "Slowed";
            data.Description = "Movement speed is reduced.";
            break;

        case Constants.StatusEffects.Silenced:
            data.Name = "Silenced";
            data.Description = "Cannot use abilities.";
            break;

        case Constants.StatusEffects.Damaging:
            data.Name = "Damaging";
            data.Description = "Deals increased damage.";
            break;

        case Constants.StatusEffects.ArmorBroken:
            data.Name = "Armor Broken";
            data.Description = "Defense is greatly reduced.";
            break;

        case Constants.StatusEffects.Healing:
            data.Name = "Healing";
            data.Description = "Gradually restores health over time.";
            break;

        case Constants.StatusEffects.Poisoned:
            data.Name = "Poisoned";
            data.Description = "Takes damage over time.";
            break;

        case Constants.StatusEffects.Bleeding:
            data.Name = "Bleeding";
            data.Description = "Loses health over time when moving.";
            break;

        case Constants.StatusEffects.Armored:
            data.Name = "Armored";
            data.Description = "Defense is increased.";
            break;

        case Constants.StatusEffects.Invincible:
            data.Name = "Invincible";
            data.Description = "Cannot take damage.";
            break;

        case Constants.StatusEffects.Weak:
            data.Name = "Weak";
            data.Description = "Deals reduced damage.";
            break;

        case Constants.StatusEffects.Stunned:
            data.Name = "Stunned";
            data.Description = "Cannot move or act.";
            break;

        case Constants.StatusEffects.Berserk:
            data.Name = "Berserk";
            data.Description = "Greatly increased attack speed and damage taken.";
            break;

        case Constants.StatusEffects.Paralyzed:
            data.Name = "Paralyzed";
            data.Description = "Cannot move but can still act.";
            break;

        default:
            data.Name = "Unknown";
            data.Description = "No description available.";
            break;
    }

    return data;
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