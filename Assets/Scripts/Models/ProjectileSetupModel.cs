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
        [CanBeNull] public readonly GameObject Source;
        
        public ProjectileSetupModel(
            Vector3 direction,
            float? rotation,
            float? speed,
            [CanBeNull] GameObject source,
            [CanBeNull] Sprite sprite
        )
        {
            this.Direction = direction;
            this.Sprite = sprite;
            this.Source = source;
            this.Speed = speed;
            this.Rotation = rotation;
        }
    }
}