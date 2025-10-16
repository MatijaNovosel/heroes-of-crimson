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

    public enum CollisionGroups
    {
      Player = 1,
      NPC = 2,
      Enemy = 3,
      Collision = 4,
      BulletCollision = 5,
    }
    
    public enum StatusEffects
    {
       Speedy = 1,
       Slowed = 2,
       Armored = 3,
       Invincible = 4,
       Silenced = 5,
       Poisoned = 6,
       Bleeding = 7,
       Regenerating = 8,
    }
  }
}