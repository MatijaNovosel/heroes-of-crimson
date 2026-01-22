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
      ATT = 1,
      DEF = 2,
      WIS = 3,
      VIT = 4,
      DEX = 5,
      SPD = 6
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
      // Eenemy sounds
      SkeletonHit = 2000,
      SkeletonDeath = 2001,
      // Player sounds
      MageHit = 3000,
      // Ability sounds
      FireSphere = 4000
    }
    
    public enum InventorySlotSpritesEnum
    {
      Weapon = 1,
      Ability = 2,
      Armor = 3,
      Accessory = 4,
      Empty = 5,
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
  }
}