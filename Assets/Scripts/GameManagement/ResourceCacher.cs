using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

namespace GameManagement
{
    public class ResourceCacher : MonoBehaviour
    {
        public static ResourceCacher Singleton;
        public Sprite[] ArmorAndWeaponSprites;
        public Sprite[] ProjectileSprites;
        public Sprite[] ConsumableSprites;

        public Dictionary<Constants.Sounds, AudioClip> Sounds = new ();
        public Dictionary<Constants.InventorySprites, Sprite> InventorySprites = new ();
        
        private void Awake()
        {
            Singleton = this;
            
            ArmorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
            ProjectileSprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
            ConsumableSprites = Resources.LoadAll<Sprite>("Sprites/Items/consumables");
            
            Sounds.Add(Constants.Sounds.Error, Resources.Load<AudioClip>("Sounds/General/error"));
            Sounds.Add(Constants.Sounds.InventoryMove, Resources.Load<AudioClip>("Sounds/General/inventoryMoveItem"));
            Sounds.Add(Constants.Sounds.InventoryEquip, Resources.Load<AudioClip>("Sounds/General/equipItem"));
            Sounds.Add(Constants.Sounds.ArrowShoot, Resources.Load<AudioClip>("Sounds/General/arrowShoot"));
            Sounds.Add(Constants.Sounds.MagicShoot, Resources.Load<AudioClip>("Sounds/General/magicShoot"));
            Sounds.Add(Constants.Sounds.GenericHit, Resources.Load<AudioClip>("Sounds/General/hit"));
            
            InventorySprites.Add(Constants.InventorySprites.Weapon, Resources.Load<Sprite>("Sprites/UI/HotbarWeaponSlot"));
            InventorySprites.Add(Constants.InventorySprites.Ability, Resources.Load<Sprite>("Sprites/UI/HotbarAbilitySlot"));
            InventorySprites.Add(Constants.InventorySprites.Armor, Resources.Load<Sprite>("Sprites/UI/HotbarArmorSlot"));
            InventorySprites.Add(Constants.InventorySprites.Accessory, Resources.Load<Sprite>("Sprites/UI/HotbarAccessorySlot"));
            InventorySprites.Add(Constants.InventorySprites.Empty, Resources.Load<Sprite>("Sprites/UI/InventoryPanel"));
        }
    }
}