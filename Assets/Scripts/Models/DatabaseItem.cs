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
        public int projectileDegree;
        public List<int> stats;
        public float range;

        // Projectile info
        public string projectilePath;
        public List<string> projectilePaths;
        public int minDamage;
        public int maxDamage;
        public int projectileCount;
        public int shootSound;
        public float projectileScale;
        public string impactColor;
        public float spinSpeed;
    }

    [System.Serializable]
    public class DatabaseItemList
    {
        public List<DatabaseItem> items;
    }
}