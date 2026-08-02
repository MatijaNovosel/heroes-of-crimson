using System.Collections.Generic;
using UnityEngine;

namespace Models.Player
{
    public class CachedWeaponFireData
    {
        public Sprite projectileSprite;
        public int projectileDegree;
        public float projectileScale;
        public float range;
        public float spinSpeed;
        public Color impactColor;
        public AudioClip shootSound;
        public int minDamage;
        public int maxDamage;
        public List<Sprite> projectileFrames;
    }
}