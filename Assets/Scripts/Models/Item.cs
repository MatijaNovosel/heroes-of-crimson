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
        public string name;
        public string description;
        public Constants.SlotTag tag;
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
