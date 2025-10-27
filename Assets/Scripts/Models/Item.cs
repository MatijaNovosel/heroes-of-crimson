using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public class Item : ScriptableObject
    {
        // Meta data
        public int id;
        public string name;
        public string description;
        public Constants.SlotTag tag;
        public Sprite sprite;
    
        // Projectile info
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
    }
}
