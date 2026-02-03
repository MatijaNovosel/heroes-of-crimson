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
        public readonly float? Scale;
        public readonly float? Damage;
        public readonly float? Range;
        public readonly List<Constants.CollisionGroups> WillDamage;
        public readonly List<Constants.CollisionGroups> WillPenetrate;
        public readonly List<Constants.StatusEffects> StatusEffects;
        public readonly Color? ParticleColor;
        
        public ProjectileSetupModel(
            Vector3 direction,
            float? rotation,
            float? speed,
            float? scale,
            float? damage,
            [CanBeNull] Sprite sprite,
            List<Constants.CollisionGroups> willDamage,
            List<Constants.CollisionGroups> willPenetrate,
            Color? particleColor,
            List<Constants.StatusEffects> statusEffects,
            float? range
        )
        {
            this.Direction = direction;
            this.Sprite = sprite;
            this.Speed = speed;
            this.Scale = scale;
            this.Damage = damage;
            this.Rotation = rotation;
            this.WillDamage = willDamage;
            this.WillPenetrate = willPenetrate;
            this.ParticleColor = particleColor;
            this.StatusEffects = statusEffects;
            this.Range = range;
        }
    }
}