using System.Collections.Generic;
using Models;

namespace HeroesOfCrimson.Utils
{
  public static class Constants
  {
    public enum ShootingMouseDirs
    {
      DOWN = 0,
      HORIZONTAL = 2,
      UP = 4
    }

    public enum ShootingDirections
    {
      DOWN = 1,
      UP = 2,
      LEFT = 3,
      RIGHT = 4,
    }

    public enum CollisionGroups
    {
      Player = 1,
      NPC = 2,
      Enemy = 3,
      Collision = 4,
      BulletCollision = 5,
    }

    public enum Stats
    {
      MGT = 0,
      ARM = 1,
      WIS = 2,
      STR = 3,
      AGI = 4,
      SWF = 5
    }

    public enum Screens
    {
      MainMenu = 0,
      NewGame = 1,
      Continue = 2,
      Game = 3
    }

    public enum StatusEffects
    {
      Energized = 1,
      Slowed = 2,
      Silenced = 3,
      Damaging = 4,
      ArmorBroken = 5,
      Healing = 6,
      Poisoned = 7,
      Bleeding = 8,
      Armored = 9,
      Invincible = 10,
      Weak = 11,
      Stunned = 12,
      Berserk = 13,
      Paralyzed = 14,
      Burning = 15,
      Radiance = 16,
      Sick = 17,
      Frostbite = 18
    }

    public enum ItemTag
    {
      None = 1,
      Weapon = 2,
      Armor = 3,
      Accessory = 4,
      Misc = 5,
      Consumable = 6
    }

    public enum ItemRarity
    {
      Common = 1,
      Uncommon = 2,
      Rare = 3,
      Epic = 4,
      Legendary = 5
    }

    public enum ProjectilePattern
    {
      Circular = 1,
      Star = 2,
      Wave = 3
    }

    public enum AnimationIdleState
    {
      Horizonal = 0,
      Up = 1,
      Down = 2
    }

    public enum Sounds
    {
      // General sounds
      Error = 1000,
      InventoryMove = 1001,
      InventoryEquip = 1002,
      ArrowShoot = 1003,
      MagicShoot = 1004,
      GenericHit = 1005,
      NoMana = 1006,
      LootDrop = 1007,
      BladeSwing = 1008,
      UsePotion = 1009,
      HitWall = 1010,

      // Eenemy sounds
      SkeletonHit = 2000,
      SkeletonDeath = 2001,
      SpiderShoot = 2002,

      // Player sounds
      MageHit = 3000,

      // Ability sounds
      FireSphere = 4000,
      Teleport = 4001
    }

    public enum InventorySlotEnum
    {
      Weapon = 0,
      Armor = 1,
      Accessory1 = 2,
      Accessory2 = 3,
      Empty = 4,
    }

    public enum RoomType
    {
      Start,
      Normal1,
      Normal2,
      Normal3,
      Normal4,
      Normal5,
      Normal6,
      Normal7,
      Treasure,
      Boss
    }

    public enum Direction
    {
      Up,
      Right,
      Down,
      Left
    }

    public enum Character
    {
      Knight = 1,
      Mage = 2,
      Ranger = 3
    }

    public enum AbilityType
    {
      Meteor = 1,
      FireSphere = 2,
      Teleport = 3
    }

    public enum TeleportMarkers
    {
      Basement = 1,
      Attic = 2,
      Cabin = 3
    }

    public enum Talents
    {
      // Mage
      ArcaneSupremacyOne = 1,
      ArcaneSupremacyTwo = 2,
      ArcaneSupremacyThree = 3,
      FireSchoolOne = 4,
      FireSchoolTwo = 5,
      FireSchoolThree = 6,
      FireSchoolFour = 7,
      IceSchoolOne = 8,
      IceSchoolTwo = 9,
      IceSchoolThree = 10,
    }

    public enum CentralWindowTabsEnum
    {
      Talents = 1,
      Quests = 2
    }
    
    public static class DialogueTriggers
    {
      public const string GiveItem = "giveItem";
      public const string SetFlag = "setFlag";
      public const string RemoveFlag = "removeFlag";
      public const string GiveXp = "giveXp";
      public const string ApplyStatusEffect = "applyStatusEffect";
      public const string TeleportToMarker = "teleportToMarker";
      public const string StartQuest = "startQuest";
    }
    
    public static class NPCMessages
    {
      public const string ReceiveDamage = "ReceiveDamage";
    }
    
    public static class DialogueConditions
    {
      public const string Flag = "flag";
      public const string Stat = "stat";
      public const string Level = "level";
      public const string HasItem = "hasItem";
    }

    public enum LootTableEnum
    {
      Basic = 1
    }

    public static readonly Dictionary<LootTableEnum, LootTableModel> LootTables = new()
    {
      {
        LootTableEnum.Basic,
        new LootTableModel(
          id: LootTableEnum.Basic,
          new()
        )
        {
          Items =
          {
            new ItemDropModel((int)ConsumableItemEnum.HealthPotion, 20, true),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfMight, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfMight, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfMight, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfMight, 20),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfAgility, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfAgility, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfAgility, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfAgility, 20),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfSwiftness, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfSwiftness, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfSwiftness, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfSwiftness, 20),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfProtection, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfProtection, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfProtection, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfProtection, 20),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfWisdom, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfWisdom, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfWisdom, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfWisdom, 20),
            
            new ItemDropModel((int)AccessoryItemEnum.IronRingOfStrength, 20),
            new ItemDropModel((int)AccessoryItemEnum.SapphireRingOfStrength, 20),
            new ItemDropModel((int)AccessoryItemEnum.SilverRingOfStrength, 20),
            new ItemDropModel((int)AccessoryItemEnum.GoldenRingOfStrength, 20),
          }
        }
      }
    };
  }
}