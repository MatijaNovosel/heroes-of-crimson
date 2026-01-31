using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public class ActiveStatusEffect
    {
        public Constants.StatusEffects Type;
        public float ExpireTime;

        public ActiveStatusEffect(Constants.StatusEffects type, float duration)
        {
            Type = type;
            ExpireTime = Time.time + duration;
        }
    }
}