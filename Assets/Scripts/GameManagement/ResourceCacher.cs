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
        public Sprite[] MiscSprites;

        public Dictionary<Constants.Sounds, AudioClip> Sounds = new ();
        public Dictionary<Constants.InventorySlotEnum, Sprite> InventorySprites = new ();
        
        private void Awake()
        {
            Singleton = this;
            
            ArmorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
            ProjectileSprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
            MiscSprites = Resources.LoadAll<Sprite>("Sprites/Items/misc");
            ConsumableSprites = Resources.LoadAll<Sprite>("Sprites/Items/consumables");
            
            // General sounds
            Sounds.Add(Constants.Sounds.Error, Resources.Load<AudioClip>("Sounds/General/error"));
            Sounds.Add(Constants.Sounds.InventoryMove, Resources.Load<AudioClip>("Sounds/General/inventoryMoveItem"));
            Sounds.Add(Constants.Sounds.InventoryEquip, Resources.Load<AudioClip>("Sounds/General/equipItem"));
            Sounds.Add(Constants.Sounds.ArrowShoot, Resources.Load<AudioClip>("Sounds/General/arrowShoot"));
            Sounds.Add(Constants.Sounds.MagicShoot, Resources.Load<AudioClip>("Sounds/General/magicShoot"));
            Sounds.Add(Constants.Sounds.GenericHit, Resources.Load<AudioClip>("Sounds/General/hit"));
            Sounds.Add(Constants.Sounds.NoMana, Resources.Load<AudioClip>("Sounds/General/noMana"));
            Sounds.Add(Constants.Sounds.LootDrop, Resources.Load<AudioClip>("Sounds/General/lootDrop"));
            
            // Enemy sounds
            Sounds.Add(Constants.Sounds.SkeletonDeath, Resources.Load<AudioClip>("Sounds/Enemies/skeletonDeath"));
            Sounds.Add(Constants.Sounds.SkeletonHit, Resources.Load<AudioClip>("Sounds/Enemies/skeletonHit"));
            
            // Player sounds
            Sounds.Add(Constants.Sounds.MageHit, Resources.Load<AudioClip>("Sounds/Player/mageHit"));
            
            // Ability sounds
            Sounds.Add(Constants.Sounds.FireSphere, Resources.Load<AudioClip>("Sounds/Abilities/fireSphere"));
            Sounds.Add(Constants.Sounds.Teleport, Resources.Load<AudioClip>("Sounds/Abilities/teleport"));
            
            InventorySprites.Add(Constants.InventorySlotEnum.Weapon, Resources.Load<Sprite>("Sprites/UI/HotbarWeaponSlot"));
            InventorySprites.Add(Constants.InventorySlotEnum.Ability, Resources.Load<Sprite>("Sprites/UI/HotbarAbilitySlot"));
            InventorySprites.Add(Constants.InventorySlotEnum.Armor, Resources.Load<Sprite>("Sprites/UI/HotbarArmorSlot"));
            InventorySprites.Add(Constants.InventorySlotEnum.Accessory, Resources.Load<Sprite>("Sprites/UI/HotbarAccessorySlot"));
            InventorySprites.Add(Constants.InventorySlotEnum.Empty, Resources.Load<Sprite>("Sprites/UI/InventoryPanel"));
        }
    }
}