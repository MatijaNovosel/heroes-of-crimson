using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

/// <summary>
/// Spider that moves and fights like the Brawler, but fires a fan of
/// projectiles that inflict Bleeding. The middle projectile flies straight at
/// the player; the others are spread evenly on either side of it.
/// </summary>
public class BiterAI : BrawlerAI
{
    [Header("Biter Spread")]
    [SerializeField] private int projectileCount = 3;
    [Tooltip("Angle between neighbouring projectiles, in degrees.")]
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private Constants.StatusEffects inflictedEffect = Constants.StatusEffects.Bleeding;

    protected override void FireAtPlayer(Vector3 shootDirection)
    {
        var statusEffects = new List<Constants.StatusEffects> { inflictedEffect };

        // For 3 projectiles at 30°: -30°, 0° (at the player), +30°
        float startAngle = -spreadAngle * (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            var direction = Quaternion.Euler(0f, 0f, startAngle + i * spreadAngle) * shootDirection;
            SpawnProjectile(direction, projectileDamage, statusEffects);
        }
    }
}
