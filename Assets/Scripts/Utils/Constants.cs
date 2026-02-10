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
       Speedy = 1,
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
       Radiance = 16
    }

    public enum ConsumableItem
    {
      HpPot = 7000,
      ManaPot = 7001,
      LifePot = 7002,
    }

    public enum ItemTag
    {
      None = 1, 
      Weapon = 2, 
      Ability = 3, 
      Armor = 4, 
      Accessory = 5,
      Misc = 6,
      Consumable = 7
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
      // Eenemy sounds
      SkeletonHit = 2000,
      SkeletonDeath = 2001,
      // Player sounds
      MageHit = 3000,
      // Ability sounds
      FireSphere = 4000,
      Teleport = 4001
    }
    
    public enum InventorySlotEnum
    {
      Weapon = 0,
      Ability = 1,
      Armor = 2,
      Accessory = 3,
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
  }
}