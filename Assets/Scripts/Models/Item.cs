using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public class Item : ScriptableObject
    {
        // Meta data
        public int id;
        public Constants.ItemRarity rarity;
        public new string name;
        public string description;
        public Color impactColor;
        public Constants.ItemTag tag;
        public Constants.Sounds shootSound;
        public Sprite sprite;
        public List<int> stats;
        public float range;

        // Projectile info
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
        public int projectileDegree;
        public Sprite projectileSprite;
        public List<Sprite> projectileFrames;
        public float projectileScale;
        public float spinSpeed;

        // Projectile pattern
        // Degrees between neighbouring projectiles, fanned out around the aim direction
        public float spreadAngle;
        // Side-to-side distance (world units) for weaving projectiles; 0 = straight
        public float waveAmplitude;
        // Distance (world units) a weaving projectile travels per full wave
        public float waveLength;
    }
}