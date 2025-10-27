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
    
    public enum SlotTag { None, Weapon, Ability, Armor, Accessory }
  }
}