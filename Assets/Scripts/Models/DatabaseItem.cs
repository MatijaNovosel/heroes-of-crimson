using System.Collections.Generic;

namespace Models
{
    [System.Serializable]
    public class DatabaseItem
    {
        // Meta data
        public int id;
        public string name;
        public string description;
        public string spritePath;
        public int tag;
        public int rarity;
        public List<int> stats;

        // Projectile info
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
    }
    
    [System.Serializable]
    public class DatabaseItemList
    {
        public List<DatabaseItem> items;
    }
}
