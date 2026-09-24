using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Models
{
    public struct ProjectileSetupModel
    {
        public Vector3 Direction;
        [CanBeNull] public readonly Sprite Sprite;
        public readonly float? Rotation;
        public readonly float? Speed;
        public readonly float? Scale;
        public float? Damage;
        public readonly float? Range;
        public readonly List<Constants.CollisionGroups> WillDamage;
        public readonly List<Constants.CollisionGroups> WillPenetrate;
        public readonly List<Constants.StatusEffects> StatusEffects;
        public readonly Color? ParticleColor;
        public List<Sprite> Frames;
        public float? SpinSpeed;

        // Optional weaving motion. Set after construction; 0 amplitude = straight line.
        // Phase is in radians; projectiles with opposite phases weave around each other.
        public float WaveAmplitude;
        public float WaveLength;
        public float WavePhase;
        
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
            float? range,
            List<Sprite> frames,
            float? spinSpeed
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
            this.Frames = frames;
            this.SpinSpeed = spinSpeed;
            this.WaveAmplitude = 0f;
            this.WaveLength = 0f;
            this.WavePhase = 0f;
        }
    }
}