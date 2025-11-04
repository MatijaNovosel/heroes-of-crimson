using System.Collections.Generic;
using UnityEngine;

namespace GameManagement
{
    public class ResourceCacher : MonoBehaviour
    {
        public static ResourceCacher Singleton;
        public Sprite[] ArmorAndWeaponSprites;
        public Sprite[] ProjectileSprites;
        public Sprite[] ConsumableSprites;

        public List<AudioClip> ShootSounds;
        
        private void Awake()
        {
            Singleton = this;
            
            ArmorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
            ProjectileSprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
            ConsumableSprites = Resources.LoadAll<Sprite>("Sprites/Items/consumables");
            
            ShootSounds.Add(Resources.Load<AudioClip>("Sounds/General/arrowShoot"));
            ShootSounds.Add(Resources.Load<AudioClip>("Sounds/General/magicShoot"));
        }
    }
}