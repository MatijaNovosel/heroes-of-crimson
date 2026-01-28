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
    
        // Projectile info
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
        public int projectileDegree;
        public Sprite projectileSprite;
    }
}
