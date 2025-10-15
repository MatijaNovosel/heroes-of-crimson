using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Models
{
    public readonly struct ProjectileSetupModel
    {
        public readonly Vector3 Direction;
        [CanBeNull] public readonly Sprite Sprite;
        public readonly float? Rotation;
        public readonly float? Speed;
        public readonly List<Constants.CollisionGroups> WillDamage;
        public readonly List<Constants.CollisionGroups> WillPenetrate;
        
        public ProjectileSetupModel(
            Vector3 direction,
            float? rotation,
            float? speed,
            [CanBeNull] Sprite sprite,
            List<Constants.CollisionGroups> willDamage,
            List<Constants.CollisionGroups> willPenetrate
        )
        {
            this.Direction = direction;
            this.Sprite = sprite;
            this.Speed = speed;
            this.Rotation = rotation;
            this.WillDamage = willDamage;
            this.WillPenetrate = willPenetrate;
        }
    }
}