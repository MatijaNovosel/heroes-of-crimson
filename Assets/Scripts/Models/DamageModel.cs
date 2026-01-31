using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public readonly struct DamageModel
    {
        public readonly float Value;
        public readonly List<Constants.StatusEffects> StatusEffects;
        
        public DamageModel(
            float value,
            List<Constants.StatusEffects> statusEffects
        )
        {
            this.Value = value;
            this.StatusEffects = statusEffects;
        }
    }
}