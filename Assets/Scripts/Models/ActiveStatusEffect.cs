using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public class ActiveStatusEffect
    {
        public Constants.StatusEffects Type;
        public float Duration;
        public float ExpireTime;
        public bool Permanent;
        public float NextTickTime;

        public ActiveStatusEffect(
            Constants.StatusEffects type,
            float duration,
            bool permanent = false
        )
        {
            Type = type;
            Duration = duration;
            Permanent = permanent;
            ExpireTime = permanent ? float.PositiveInfinity : Time.time + duration;
            NextTickTime = Time.time;
        }

        public float RemainingTime => Mathf.Max(0, ExpireTime - Time.time);
        public float NormalizedTime => Duration <= 0 ? 0 : RemainingTime / Duration;
    }
}