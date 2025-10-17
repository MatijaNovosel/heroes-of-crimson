using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public readonly struct StatusEffectUIModel
    {
        public readonly GameObject Icon;
        public readonly Constants.StatusEffects StatusEffect;
        
        public StatusEffectUIModel(
            GameObject icon,
            Constants.StatusEffects statusEffect
        )
        {
            this.Icon = icon;
            this.StatusEffect = statusEffect;
        }
    }
}