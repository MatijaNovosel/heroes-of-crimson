using HeroesOfCrimson.Utils;
using UnityEngine;

namespace UI.Inventory
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
    {
        // Meta data
        public int id;
        public Constants.SlotTag tag;
        public Sprite sprite;
        public string name;
        public string description;
    
        // Projectile info
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
    }
}
