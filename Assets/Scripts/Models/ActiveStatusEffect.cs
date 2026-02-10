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

        public ActiveStatusEffect(
            Constants.StatusEffects type,
            float duration,
            bool permanent = false
        )
        {
            Type = type;
            ExpireTime = Time.time + duration;
            Duration = duration;
            Permanent = permanent;
        }
        
        public float RemainingTime => Mathf.Max(0, ExpireTime - Time.time);
        public float NormalizedTime => Duration <= 0 ? 0 : RemainingTime / Duration;
    }
}